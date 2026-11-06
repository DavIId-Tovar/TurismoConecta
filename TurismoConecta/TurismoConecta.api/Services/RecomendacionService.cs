using Microsoft.EntityFrameworkCore;
using TurismoConecta.api.Data;
using TurismoConecta.api.DTOs.Recomendaciones;
using TurismoConecta.api.Models;
using TurismoConecta.api.Services.Interfaces;
using TurismoConecta.api.Services.Recomendaciones;

namespace TurismoConecta.api.Services
{
    /// <summary>
    /// Motor de recomendaciones híbrido y explicable:
    ///   puntaje = afinidad (gustos del usuario) + calidad (reseñas) + novedad, y después diversidad.
    /// Cada recomendación lleva la razón principal para mostrarla en pantalla.
    /// </summary>
    public class RecomendacionService : IRecomendacionService
    {
        // ---- Cuánto aporta cada señal al perfil de gustos ----
        private const double PesoGustoDeclarado = 2.0;
        private const double PesoFavorito = 3.0;
        private const double PesoItinerario = 2.0;
        private const double PesoPorEstrella = 1.5;        // 5★ → +3, 4★ → +1.5, 2★ → -1.5, 1★ → -3
        private const double PesoDescartado = -1.0;        // "No me interesa": sus etiquetas pierden peso

        
        // ---- Cuánto pesa cada parte del puntaje final (sin modelo colaborativo) ----
        private const double PesoAfinidad = 0.55;
        private const double PesoCalidad = 0.30;
        private const double PesoNovedad = 0.15;

        // ---- Cuando el modelo de ML.NET conoce al usuario, entra con el 20 % ----
        private const double PesoAfinidadConModelo = 0.45;
        private const double PesoCalidadConModelo = 0.25;
        private const double PesoColaborativo = 0.20;
        private const double PesoNovedadConModelo = 0.10;
        private const double ColaborativoMinimoRazon = 0.8;  // predice 4.2★ o más

        private const double PenalizacionParecidos = 0.25;  // diversidad: baja a los muy parecidos a los ya elegidos
        private const int ConfianzaBayesiana = 3;           // "reseñas imaginarias" de 3.5★ que tiene todo municipio
        private const double PromedioNeutral = 3.5;
        private const double AfinidadMinimaRazon = 0.35;    // por debajo, la razón no se atribuye a los gustos
        private const int MaximoPreferencias = 10;

        // ---- "Queda cerca" ----
        private const double FactorCarretera = 1.3;         // la carretera es ~30 % más larga que la línea recta
        private const double VelocidadPromedioKmH = 45;     // vías de montaña en Boyacá

        // ---- "Sorpréndeme" ----
        private const int MaximoDiasSorpresa = 7;
        private const int OpcionesDeInicio = 3;             // "Otra sorpresa" rota el inicio entre las 3 mejores
        private const double PesoGustoEnRuta = 0.6;
        private const double PesoCercaniaEnRuta = 0.4;

        private readonly AppDbContext _context;
        private readonly ModeloColaborativo _modelo;
        private readonly double _radioCercaKm;
        private readonly ILogger<RecomendacionService> _logger;

        public RecomendacionService(AppDbContext context, ModeloColaborativo modelo, IConfiguration configuracion, ILogger<RecomendacionService> logger)
        {
            _context = context;
            _modelo = modelo;
            _logger = logger;
            _radioCercaKm = Math.Max(5, configuracion.GetValue("Recomendaciones:RadioCercaKm", 60.0));
        }

        // ════════════════════════════════════════════════
        // TIPOS DE TRABAJO
        // ════════════════════════════════════════════════
        private sealed record Candidato(
            int IdMunicipio,
            string Nombre,
            string? ImagenUrl,
            string? Descripcion,
            Dictionary<int, string> Etiquetas,
            double? Promedio,
            int TotalResenas,
            double? Latitud,
            double? Longitud)
        {
            public bool TieneUbicacion => Latitud is double lat && Longitud is double lng && lat != 0 && lng != 0;
        }

        private sealed record Puntuado(Candidato Municipio, double Puntaje, double Afinidad, double? Colaborativo);

        /// <summary>Cuánto pesa una etiqueta en el perfil y qué la originó (para explicar la recomendación).</summary>
        private sealed class AporteEtiqueta
        {
            public double Peso;
            public double MejorAporte;
            public string Tipo = TiposRazon.Gusto;
            public string Origen = "";
        }

        // ════════════════════════════════════════════════
        // API PÚBLICA DEL SERVICIO
        // ════════════════════════════════════════════════
        public async Task<List<RecomendacionDto>> ParaUsuarioAsync(int idUsuario, int cantidad, CancellationToken ct = default)
        {
            cantidad = Math.Clamp(cantidad, 1, 12);
            var candidatos = await CargarCandidatosAsync(ct);
            var (perfil, conocidos, descartados) = await ConstruirPerfilAsync(idUsuario, candidatos, ct);

            // Lo que el usuario marcó "No me interesa" no vuelve a salir
            candidatos.RemoveAll(c => descartados.Contains(c.IdMunicipio));
            return Clasificar(candidatos, perfil, conocidos, cantidad, idUsuario);
        }

        public async Task<List<RecomendacionDto>> PorGustosAsync(List<int> idsEtiquetas, int cantidad, CancellationToken ct = default)
        {
            cantidad = Math.Clamp(cantidad, 1, 12);
            var candidatos = await CargarCandidatosAsync(ct);
            var perfil = PerfilPorGustos(candidatos, idsEtiquetas);

            return Clasificar(candidatos, perfil, new HashSet<int>(), cantidad, idUsuario: null);
        }

        /// <summary>
        /// "Sorpréndeme": arma una ruta de una parada por día con los mejores destinos para el usuario,
        /// en un orden que evita ir y volver (cada parada combina gusto 60 % + cercanía a la anterior 40 %).
        /// variante cambia el punto de partida entre las mejores opciones ("Otra sorpresa").
        /// </summary>
        public async Task<PropuestaItinerarioDto> SorprendemeAsync(
            int? idUsuario, List<int> idsEtiquetas, int dias, int variante, CancellationToken ct = default)
        {
            dias = Math.Clamp(dias, 1, MaximoDiasSorpresa);
            var candidatos = await CargarCandidatosAsync(ct);

            Dictionary<int, AporteEtiqueta> perfil;
            var conocidos = new HashSet<int>();

            if (idUsuario is int id)
            {
                (perfil, conocidos, var descartados) = await ConstruirPerfilAsync(id, candidatos, ct);
                candidatos.RemoveAll(c => descartados.Contains(c.IdMunicipio));
            }
            else
            {
                perfil = PerfilPorGustos(candidatos, idsEtiquetas);
            }

            // Sin coordenadas no se puede ordenar la ruta ni calcular tramos
            candidatos.RemoveAll(c => !c.TieneUbicacion);

            var propuesta = new PropuestaItinerarioDto { DiasPedidos = dias };
            if (candidatos.Count == 0)
            {
                propuesta.Aviso = "Aún no hay destinos con ubicación para armar una ruta.";
                return propuesta;
            }

            var puntuados = Puntuar(candidatos, perfil, conocidos, idUsuario);
            var maximo = Math.Max(puntuados[0].Puntaje, 0.0001);

            // 1. Punto de partida: una de las mejores opciones, según la variante
            var opcionesInicio = puntuados.Take(Math.Min(OpcionesDeInicio, puntuados.Count)).ToList();
            var ruta = new List<Puntuado> { opcionesInicio[Math.Abs(variante) % opcionesInicio.Count] };
            var restantes = puntuados.Where(p => p != ruta[0]).ToList();

            // 2. Siguientes paradas: lo que más te gusta, sin alejarse demasiado de la parada anterior
            while (ruta.Count < dias && restantes.Count > 0)
            {
                var anterior = ruta[^1].Municipio;
                var siguiente = restantes.MaxBy(p =>
                    PesoGustoEnRuta * (p.Puntaje / maximo)
                    + PesoCercaniaEnRuta * Math.Clamp(1 - DistanciaKm(anterior, p.Municipio) / (2 * _radioCercaKm), 0, 1))!;

                ruta.Add(siguiente);
                restantes.Remove(siguiente);
            }

            // 3. Paradas con su razón y el tramo desde la anterior
            for (var i = 0; i < ruta.Count; i++)
            {
                var p = ruta[i];
                var (razon, tipo) = Explicar(p, perfil, conocidos, esJoya: false);
                var parada = new ParadaPropuestaDto
                {
                    IdMunicipio = p.Municipio.IdMunicipio,
                    Nombre = p.Municipio.Nombre,
                    ImagenUrl = p.Municipio.ImagenUrl,
                    Etiquetas = p.Municipio.Etiquetas.Values.ToList(),
                    PromedioCalificacion = p.Municipio.Promedio,
                    TotalResenas = p.Municipio.TotalResenas,
                    DiaNumero = i + 1,
                    Razon = razon,
                    TipoRazon = tipo
                };

                if (i > 0)
                {
                    var (km, minutos) = EstimarTramo(DistanciaKm(ruta[i - 1].Municipio, p.Municipio));
                    parada.KmDesdeAnterior = km;
                    parada.MinutosDesdeAnterior = minutos;

                    // Sin una razón personal, lo que lo explica es que queda de camino
                    if (tipo == TiposRazon.Nuevo)
                    {
                        parada.Razon = $"Queda de camino: ~{km} km desde {ruta[i - 1].Municipio.Nombre}";
                        parada.TipoRazon = TiposRazon.Cerca;
                    }
                }

                propuesta.Paradas.Add(parada);
            }

            propuesta.KmTotal = propuesta.Paradas.Sum(p => p.KmDesdeAnterior ?? 0);
            propuesta.MinutosTotal = propuesta.Paradas.Sum(p => p.MinutosDesdeAnterior ?? 0);
            propuesta.Nombre = NombrarRuta(ruta, perfil);
            propuesta.OpcionesDeInicio = opcionesInicio.Count;

            if (ruta.Count < dias)
                propuesta.Aviso = $"Por ahora hay {ruta.Count} destino(s) con ubicación para ti: la ruta tiene una parada por día hasta donde alcanza.";

            return propuesta;
        }

        /// <summary>
        /// Km y tiempo estimados de una ruta en el orden dado (para el resumen de Crear itinerario).
        /// Los municipios sin coordenadas se saltan: el tramo se mide desde la última parada con ubicación.
        /// </summary>
        public async Task<RutaEstimadaDto> EstimarRutaAsync(List<int> idsEnOrden, bool optimizar, CancellationToken ct = default)
        {
            var resultado = new RutaEstimadaDto();
            if (idsEnOrden.Count == 0) return resultado;

            var porId = (await CargarCandidatosAsync(ct))
                .Where(c => idsEnOrden.Contains(c.IdMunicipio))
                .ToDictionary(c => c.IdMunicipio);

            // Orden por cercanía: la primera parada con ubicación es el punto de partida;
            // las que no tienen coordenadas van al final, en el orden en que estaban
            if (optimizar)
            {
                var conUbicacion = idsEnOrden.Where(id => porId.TryGetValue(id, out var m) && m.TieneUbicacion).Select(id => porId[id]).ToList();
                var sinUbicacion = idsEnOrden.Where(id => porId.TryGetValue(id, out var m) && !m.TieneUbicacion);

                idsEnOrden = OrdenarPorCercania(conUbicacion).Select(c => c.IdMunicipio).Concat(sinUbicacion).ToList();
            }

            Candidato? anterior = null;
            foreach (var id in idsEnOrden)
            {
                if (!porId.TryGetValue(id, out var municipio)) continue;

                var tramo = new TramoRutaDto { IdMunicipio = id };

                if (!municipio.TieneUbicacion)
                {
                    resultado.SinUbicacion.Add(municipio.Nombre);
                }
                else
                {
                    if (anterior is not null)
                    {
                        var (km, minutos) = EstimarTramo(DistanciaKm(anterior, municipio));
                        tramo.KmDesdeAnterior = km;
                        tramo.MinutosDesdeAnterior = minutos;
                        tramo.DesdeNombre = anterior.Nombre;
                    }
                    anterior = municipio;
                }

                resultado.Tramos.Add(tramo);
            }

            resultado.KmTotal = resultado.Tramos.Sum(t => t.KmDesdeAnterior ?? 0);
            resultado.MinutosTotal = resultado.Tramos.Sum(t => t.MinutosDesdeAnterior ?? 0);
            return resultado;
        }

        /// <summary>
        /// Ruta más corta que pasa por todas las paradas empezando por la primera (no vuelve al inicio).
        /// 1) Vecino más cercano: desde cada parada se va a la más cercana que falte.
        /// 2) Mejora 2-opt: invierte tramos mientras eso acorte el total (quita los cruces de la ruta).
        /// Con las pocas paradas de un viaje (máx. 30) es instantáneo.
        /// </summary>
        private static List<Candidato> OrdenarPorCercania(List<Candidato> paradas)
        {
            if (paradas.Count <= 2) return paradas;

            var ruta = new List<Candidato> { paradas[0] };
            var faltan = paradas.Skip(1).ToList();
            while (faltan.Count > 0)
            {
                var siguiente = faltan.MinBy(p => DistanciaKm(ruta[^1], p))!;
                ruta.Add(siguiente);
                faltan.Remove(siguiente);
            }

            var mejoro = true;
            while (mejoro)
            {
                mejoro = false;
                for (var i = 1; i < ruta.Count - 1; i++)
                {
                    for (var k = i + 1; k < ruta.Count; k++)
                    {
                        // Invertir ruta[i..k]: cambian el tramo que entra a i y el que sale de k
                        var antes = DistanciaKm(ruta[i - 1], ruta[i])
                                  + (k + 1 < ruta.Count ? DistanciaKm(ruta[k], ruta[k + 1]) : 0);
                        var despues = DistanciaKm(ruta[i - 1], ruta[k])
                                    + (k + 1 < ruta.Count ? DistanciaKm(ruta[i], ruta[k + 1]) : 0);

                        if (despues < antes - 0.01)
                        {
                            ruta.Reverse(i, k - i + 1);
                            mejoro = true;
                        }
                    }
                }
            }

            return ruta;
        }

        /// <summary>Perfil de gustos a partir de etiquetas marcadas en pantalla (sin sesión).</summary>
        private static Dictionary<int, AporteEtiqueta> PerfilPorGustos(List<Candidato> candidatos, List<int> idsEtiquetas)
        {
            var nombres = candidatos
                .SelectMany(c => c.Etiquetas)
                .GroupBy(e => e.Key)
                .ToDictionary(g => g.Key, g => g.First().Value);

            var perfil = new Dictionary<int, AporteEtiqueta>();
            foreach (var idEtiqueta in idsEtiquetas.Distinct().Take(MaximoPreferencias))
                Sumar(perfil, idEtiqueta, PesoGustoDeclarado, TiposRazon.Gusto, nombres.GetValueOrDefault(idEtiqueta, ""));

            return perfil;
        }

        /// <summary>"Ruta de Artesanías: Ráquira → Villa de Leyva → Tunja" (la etiqueta que más te gusta de la ruta).</summary>
        private static string NombrarRuta(List<Puntuado> ruta, Dictionary<int, AporteEtiqueta> perfil)
        {
            var etiquetas = ruta.SelectMany(p => p.Municipio.Etiquetas).ToList();

            var tema = etiquetas
                .GroupBy(e => e.Key)
                .OrderByDescending(g => perfil.TryGetValue(g.Key, out var a) ? a.Peso : 0)
                .ThenByDescending(g => g.Count())
                .Select(g => g.First().Value)
                .FirstOrDefault();

            var nombres = ruta.Select(p => p.Municipio.Nombre).ToList();
            var recorrido = nombres.Count <= 3
                ? string.Join(" → ", nombres)
                : $"{nombres[0]} → … → {nombres[^1]}";

            var nombre = tema is null ? $"Ruta sorpresa: {recorrido}" : $"Ruta de {tema}: {recorrido}";
            return nombre.Length <= 100 ? nombre : nombre[..100];
        }

        public async Task<List<RecomendacionDto>> SimilaresAsync(int idMunicipio, int? idUsuario, int cantidad, CancellationToken ct = default)
        {
            cantidad = Math.Clamp(cantidad, 1, 8);
            var candidatos = await CargarCandidatosAsync(ct);

            var origen = candidatos.FirstOrDefault(c => c.IdMunicipio == idMunicipio);
            if (origen is null) return new();

            // Con sesión, lo que marcó "No me interesa" tampoco sale en "Si te gustó X…"
            var descartados = idUsuario is int id
                ? (await _context.RecomendacionesDescartadas
                    .AsNoTracking()
                    .Where(d => d.IdUsuario == id)
                    .Select(d => d.IdMunicipio)
                    .ToListAsync(ct)).ToHashSet()
                : new HashSet<int>();

            return candidatos
                .Where(c => c.IdMunicipio != idMunicipio && !descartados.Contains(c.IdMunicipio))
                .Select(c => new { Municipio = c, Puntaje = 0.65 * Jaccard(origen, c) + 0.35 * Calidad(c) })
                .OrderByDescending(x => x.Puntaje)
                .ThenBy(x => x.Municipio.Nombre)
                .Take(cantidad)
                .Select(x =>
                {
                    var comunes = origen.Etiquetas.Keys
                        .Intersect(x.Municipio.Etiquetas.Keys)
                        .Select(id => x.Municipio.Etiquetas[id])
                        .Take(2)
                        .ToList();

                    var (razon, tipo) = comunes.Count switch
                    {
                        0 when x.Municipio.TotalResenas > 0 =>
                            ($"Los viajeros lo califican {x.Municipio.Promedio:0.0} ★", TiposRazon.Popular),
                        0 => ("Otro rincón de Boyacá por descubrir", TiposRazon.Nuevo),
                        1 => ($"También es destino de {comunes[0]}", TiposRazon.Similar),
                        _ => ($"Comparte {comunes[0]} y {comunes[1]}", TiposRazon.Similar)
                    };

                    return CrearDto(x.Municipio, x.Puntaje, razon, tipo);
                })
                .ToList();
        }

        public Task<List<int>> ObtenerPreferenciasAsync(int idUsuario, CancellationToken ct = default) =>
            _context.PreferenciasUsuario
                .AsNoTracking()
                .Where(p => p.IdUsuario == idUsuario)
                .OrderBy(p => p.IdEtiqueta)
                .Select(p => p.IdEtiqueta)
                .ToListAsync(ct);

        public async Task<(bool exito, string? error)> GuardarPreferenciasAsync(int idUsuario, List<int> idsEtiquetas, CancellationToken ct = default)
        {
            var deseadas = (idsEtiquetas ?? new()).Distinct().ToList();
            if (deseadas.Count > MaximoPreferencias)
                return (false, $"Puedes elegir hasta {MaximoPreferencias} gustos.");

            var validas = await _context.Etiqueta
                .Where(e => deseadas.Contains(e.IdEtiqueta) && e.Activo)
                .CountAsync(ct);

            if (validas != deseadas.Count)
                return (false, "Uno o más gustos no existen o ya no están disponibles.");

            var actuales = await _context.PreferenciasUsuario
                .Where(p => p.IdUsuario == idUsuario)
                .ToListAsync(ct);

            // Solo se toca lo que cambió: se quitan las desmarcadas y se agregan las nuevas
            _context.PreferenciasUsuario.RemoveRange(actuales.Where(p => !deseadas.Contains(p.IdEtiqueta)));

            var yaGuardadas = actuales.Select(p => p.IdEtiqueta).ToHashSet();
            _context.PreferenciasUsuario.AddRange(deseadas
                .Where(id => !yaGuardadas.Contains(id))
                .Select(id => new PreferenciaUsuario
                {
                    IdUsuario = idUsuario,
                    IdEtiqueta = id,
                    FechaRegistro = DateTime.UtcNow
                }));

            try
            {
                await _context.SaveChangesAsync(ct);
                return (true, null);
            }
            catch (DbUpdateException)
            {
                // Dos guardados al mismo tiempo pueden chocar con el índice único: el cliente reintenta
                return (false, "No pudimos guardar tus gustos. Inténtalo de nuevo.");
            }
        }

        public async Task<List<RecomendacionDto>> CercaAsync(int idMunicipio, int? idUsuario, List<int> excluir, int cantidad, CancellationToken ct = default)
        {
            cantidad = Math.Clamp(cantidad, 1, 8);
            var candidatos = await CargarCandidatosAsync(ct);

            var origen = candidatos.FirstOrDefault(c => c.IdMunicipio == idMunicipio);
            if (origen is null || !origen.TieneUbicacion) return new();

            // Se ocultan el origen, lo que ya está en la ruta y lo que el usuario descartó
            var ocultos = new HashSet<int>(excluir) { idMunicipio };
            var afinidades = new Dictionary<int, double>();

            if (idUsuario is int id)
            {
                var (perfil, _, descartados) = await ConstruirPerfilAsync(id, candidatos, ct);
                ocultos.UnionWith(descartados);
                if (perfil.Values.Any(a => a.Peso > 0))
                    afinidades = CalcularAfinidades(candidatos, perfil);
            }

            var conDistancia = candidatos
                .Where(c => !ocultos.Contains(c.IdMunicipio) && c.TieneUbicacion)
                .Select(c => (Municipio: c, Km: DistanciaKm(origen, c)))
                .ToList();

            // Los que están dentro del radio; si ninguno lo está, al menos los más cercanos
            var opciones = conDistancia.Where(x => x.Km <= _radioCercaKm).ToList();
            if (opciones.Count == 0)
                opciones = conDistancia.OrderBy(x => x.Km).Take(cantidad).ToList();

            return opciones
                .Select(x =>
                {
                    var cercania = Math.Clamp(1 - x.Km / _radioCercaKm, 0, 1);
                    var puntaje = afinidades.Count > 0
                        ? 0.5 * cercania + 0.3 * afinidades[x.Municipio.IdMunicipio] + 0.2 * Calidad(x.Municipio)
                        : 0.6 * cercania + 0.4 * Calidad(x.Municipio);

                    return (x.Municipio, x.Km, Puntaje: puntaje);
                })
                .OrderByDescending(x => x.Puntaje)
                .ThenBy(x => x.Km)
                .Take(cantidad)
                .Select(x =>
                {
                    var (kmCarretera, minutos) = EstimarTramo(x.Km);

                    var dto = CrearDto(x.Municipio, x.Puntaje,
                        $"~{kmCarretera} km de {origen.Nombre} · {FormatoDuracion(minutos)}", TiposRazon.Cerca);
                    dto.DistanciaKm = kmCarretera;
                    dto.TiempoEstimadoMin = minutos;
                    return dto;
                })
                .ToList();
        }

        public async Task<(bool exito, string? error)> DescartarAsync(int idUsuario, int idMunicipio, CancellationToken ct = default)
        {
            if (!await _context.Municipios.AnyAsync(m => m.IdMunicipio == idMunicipio && m.Activo, ct))
                return (false, "El municipio no existe o ya no está disponible.");

            // Descartar dos veces no es un error: el resultado es el mismo
            if (await _context.RecomendacionesDescartadas.AnyAsync(d => d.IdUsuario == idUsuario && d.IdMunicipio == idMunicipio, ct))
                return (true, null);

            _context.RecomendacionesDescartadas.Add(new RecomendacionDescartada
            {
                IdUsuario = idUsuario,
                IdMunicipio = idMunicipio,
                FechaRegistro = DateTime.UtcNow
            });

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 })
            {
                // Doble clic: el otro guardado ganó con el índice único, el municipio ya quedó descartado
            }
            catch (DbUpdateException ex)
            {
                // Cualquier otro fallo (tabla, llaves foráneas…) se informa: la pantalla no debe decir "Ocultamos" sin guardarlo
                _logger.LogError(ex, "[Recomendaciones] No se pudo guardar el descarte del municipio {IdMunicipio} del usuario {IdUsuario}", idMunicipio, idUsuario);
                return (false, "No pudimos ocultar ese destino. Inténtalo de nuevo.");
            }

            return (true, null);
        }

        public Task<int> RestaurarAsync(int idUsuario, int idMunicipio, CancellationToken ct = default) =>
            _context.RecomendacionesDescartadas
                .Where(d => d.IdUsuario == idUsuario && d.IdMunicipio == idMunicipio)
                .ExecuteDeleteAsync(ct);

        public Task<List<MunicipioDescartadoDto>> ListarDescartadosAsync(int idUsuario, CancellationToken ct = default) =>
            _context.RecomendacionesDescartadas
                .AsNoTracking()
                .Where(d => d.IdUsuario == idUsuario)
                .OrderByDescending(d => d.FechaRegistro)
                .Select(d => new MunicipioDescartadoDto
                {
                    IdMunicipio = d.IdMunicipio,
                    Nombre = d.IdMunicipioNavigation.Nombre,
                    ImagenUrl = d.IdMunicipioNavigation.ImagenUrl,
                    FechaRegistro = d.FechaRegistro
                })
                .ToListAsync(ct);

        public async Task<MetricasRecomendacionesDto> MetricasAsync(CancellationToken ct = default)
        {
            var masDescartados = await _context.RecomendacionesDescartadas
                .AsNoTracking()
                .GroupBy(d => d.IdMunicipioNavigation.Nombre)
                .Select(g => new { Nombre = g.Key, Total = g.Count() })
                .OrderByDescending(g => g.Total)
                .ThenBy(g => g.Nombre)
                .Take(3)
                .ToListAsync(ct);

            return new MetricasRecomendacionesDto
            {
                UsuariosConGustos = await _context.PreferenciasUsuario.Select(p => p.IdUsuario).Distinct().CountAsync(ct),
                Descartes = await _context.RecomendacionesDescartadas.CountAsync(ct),
                UsuariosQueDescartaron = await _context.RecomendacionesDescartadas.Select(d => d.IdUsuario).Distinct().CountAsync(ct),
                MunicipiosActivos = await _context.Municipios.CountAsync(m => m.Activo, ct),
                MunicipiosSinCoordenadas = await _context.Municipios.CountAsync(m => m.Activo
                    && (m.Latitud == null || m.Longitud == null || m.Latitud == 0 || m.Longitud == 0), ct),
                MasDescartados = masDescartados.Select(m => $"{m.Nombre} ({m.Total})").ToList(),
                RadioCercaKm = _radioCercaKm
            };
        }

        // ════════════════════════════════════════════════
        // DATOS
        // ════════════════════════════════════════════════

        /// <summary>Todos los municipios activos con sus etiquetas y su calificación (solo reseñas visibles).</summary>
        private async Task<List<Candidato>> CargarCandidatosAsync(CancellationToken ct)
        {
            var datos = await _context.Municipios
                .AsNoTracking()
                .Where(m => m.Activo)
                .Select(m => new
                {
                    m.IdMunicipio,
                    m.Nombre,
                    m.ImagenUrl,
                    m.Descripcion,
                    Etiquetas = m.MunicipioEtiqueta
                        .Where(me => me.IdEtiquetaNavigation.Activo)
                        .Select(me => new { me.IdEtiqueta, me.IdEtiquetaNavigation.Nombre })
                        .ToList(),
                    Promedio = m.Reseñas.Where(r => r.Moderada).Average(r => (double?)r.Calificacion),
                    Total = m.Reseñas.Count(r => r.Moderada),
                    m.Latitud,
                    m.Longitud
                })
                .ToListAsync(ct);

            return datos
                .Select(d => new Candidato(
                    d.IdMunicipio,
                    d.Nombre,
                    d.ImagenUrl,
                    d.Descripcion,
                    d.Etiquetas.GroupBy(e => e.IdEtiqueta).ToDictionary(g => g.Key, g => g.First().Nombre),
                    d.Promedio,
                    d.Total,
                    (double?)d.Latitud,
                    (double?)d.Longitud))
                .ToList();
        }

        /// <summary>
        /// El perfil de gustos sale de 5 señales: gustos elegidos, favoritos, reseñas propias, itinerarios
        /// y lo que marcó "No me interesa" (resta).
        /// "Conocidos" son los municipios con los que el usuario ya interactuó (se recomiendan al final).
        /// "Descartados" no se le vuelven a recomendar.
        /// </summary>
        private async Task<(Dictionary<int, AporteEtiqueta> Perfil, HashSet<int> Conocidos, HashSet<int> Descartados)> ConstruirPerfilAsync(
            int idUsuario, List<Candidato> candidatos, CancellationToken ct)
        {
            var perfil = new Dictionary<int, AporteEtiqueta>();
            var conocidos = new HashSet<int>();
            var porId = candidatos.ToDictionary(c => c.IdMunicipio);

            // 1. Gustos que eligió
            var gustos = await _context.PreferenciasUsuario
                .AsNoTracking()
                .Where(p => p.IdUsuario == idUsuario)
                .Select(p => new { p.IdEtiqueta, p.IdEtiquetaNavigation.Nombre })
                .ToListAsync(ct);

            foreach (var gusto in gustos)
                Sumar(perfil, gusto.IdEtiqueta, PesoGustoDeclarado, TiposRazon.Gusto, gusto.Nombre);

            // 2. Municipios guardados en favoritos
            var favoritos = await _context.Favoritos
                .AsNoTracking()
                .Where(f => f.IdUsuario == idUsuario && f.IdMunicipio != null)
                .Select(f => f.IdMunicipio!.Value)
                .ToListAsync(ct);

            foreach (var idMunicipio in favoritos)
                SumarMunicipio(idMunicipio, PesoFavorito, TiposRazon.Favorito);

            // 3. Sus reseñas: 4-5★ suman, 1-2★ restan (cuenta también las retenidas: es su opinión)
            var reseñas = await _context.Reseñas
                .AsNoTracking()
                .Where(r => r.IdUsuario == idUsuario && r.IdMunicipio != null)
                .Select(r => new { IdMunicipio = r.IdMunicipio!.Value, r.Calificacion })
                .ToListAsync(ct);

            foreach (var reseña in reseñas)
                SumarMunicipio(reseña.IdMunicipio, (reseña.Calificacion - 3) * PesoPorEstrella, TiposRazon.Calificado);

            // 4. Municipios que puso en sus itinerarios
            var enItinerarios = await _context.ItinerarioDetalles
                .AsNoTracking()
                .Where(d => d.IdItinerarioNavigation.IdUsuario == idUsuario)
                .Select(d => d.IdMunicipio)
                .Distinct()
                .ToListAsync(ct);

            foreach (var idMunicipio in enItinerarios)
                SumarMunicipio(idMunicipio, PesoItinerario, TiposRazon.Itinerario);

            // 5. Lo que marcó "No me interesa": sus etiquetas restan (no cuenta como conocido)
            var descartados = (await _context.RecomendacionesDescartadas
                .AsNoTracking()
                .Where(d => d.IdUsuario == idUsuario)
                .Select(d => d.IdMunicipio)
                .ToListAsync(ct))
                .ToHashSet();

            foreach (var idMunicipio in descartados)
            {
                if (!porId.TryGetValue(idMunicipio, out var municipio)) continue;
                foreach (var idEtiqueta in municipio.Etiquetas.Keys)
                    Sumar(perfil, idEtiqueta, PesoDescartado, TiposRazon.Gusto, municipio.Nombre);
            }

            return (perfil, conocidos, descartados);

            void SumarMunicipio(int idMunicipio, double aporte, string tipo)
            {
                if (!porId.TryGetValue(idMunicipio, out var municipio)) return;
                conocidos.Add(idMunicipio);
                if (aporte == 0) return;

                foreach (var idEtiqueta in municipio.Etiquetas.Keys)
                    Sumar(perfil, idEtiqueta, aporte, tipo, municipio.Nombre);
            }
        }

        private static void Sumar(Dictionary<int, AporteEtiqueta> perfil, int idEtiqueta, double aporte, string tipo, string origen)
        {
            if (!perfil.TryGetValue(idEtiqueta, out var actual))
                perfil[idEtiqueta] = actual = new AporteEtiqueta();

            actual.Peso += aporte;

            // La razón que se muestra es la señal que más aportó a esta etiqueta
            if (aporte > actual.MejorAporte)
            {
                actual.MejorAporte = aporte;
                actual.Tipo = tipo;
                actual.Origen = origen;
            }
        }

        // ════════════════════════════════════════════════
        // PUNTAJE, DIVERSIDAD Y RAZONES
        // ════════════════════════════════════════════════
        private List<RecomendacionDto> Clasificar(
            List<Candidato> candidatos, Dictionary<int, AporteEtiqueta> perfil, HashSet<int> conocidos, int cantidad, int? idUsuario)
        {
            var hayPerfil = perfil.Values.Any(a => a.Peso > 0);
            var puntuados = Puntuar(candidatos, perfil, conocidos, idUsuario);

            // Primero lo nuevo para el usuario; lo que ya conoce solo completa si faltan opciones
            var nuevos = puntuados.Where(p => !conocidos.Contains(p.Municipio.IdMunicipio)).ToList();
            // Diversidad solo cuando hay algo personal detrás; si no, el orden "mejor calificados" se respeta tal cual
            var esPersonal = hayPerfil || puntuados.Any(p => p.Colaborativo.HasValue);
            var elegidos = esPersonal ? Diversificar(nuevos, cantidad) : nuevos.Take(cantidad).ToList();
            elegidos.AddRange(puntuados
                .Where(p => conocidos.Contains(p.Municipio.IdMunicipio))
                .Take(cantidad - elegidos.Count));

            // Exploración: una "joya escondida" (pocas reseñas, encaja muy bien) en el último lugar
            var joya = hayPerfil && elegidos.Count >= 3
                ? nuevos.FirstOrDefault(p => p.Municipio.TotalResenas <= 1 && p.Afinidad >= 0.5 && !elegidos.Contains(p))
                : null;

            if (joya is not null && !elegidos.Any(p => p.Municipio.TotalResenas <= 1 && p.Afinidad >= 0.5))
                elegidos[^1] = joya;

            return elegidos
                .Select(p =>
                {
                    var (razon, tipo) = Explicar(p, perfil, conocidos, esJoya: p == joya);
                    return CrearDto(p.Municipio, p.Puntaje, razon, tipo);
                })
                .ToList();
        }

        /// <summary>Puntaje de cada candidato (afinidad + calidad + ML.NET + novedad), del mejor al peor.</summary>
        private List<Puntuado> Puntuar(
            List<Candidato> candidatos, Dictionary<int, AporteEtiqueta> perfil, HashSet<int> conocidos, int? idUsuario)
        {
            var hayPerfil = perfil.Values.Any(a => a.Peso > 0);
            var afinidades = CalcularAfinidades(candidatos, perfil);

            return candidatos
                .Select(c =>
                {
                    var afinidad = afinidades[c.IdMunicipio];
                    var novedad = conocidos.Contains(c.IdMunicipio) ? 0 : 1;

                    // Lo que ML.NET estima que le daría este usuario (1-5★), llevado a 0-1. null si no lo conoce
                    var colaborativo = idUsuario is int id && _modelo.Predecir(id, c.IdMunicipio) is double estimada
                        ? (estimada - 1) / 4
                        : (double?)null;

                    var puntaje = (hayPerfil, colaborativo) switch
                    {
                        (true, double colab) => PesoAfinidadConModelo * afinidad + PesoCalidadConModelo * Calidad(c)
                                              + PesoColaborativo * colab + PesoNovedadConModelo * novedad,
                        (true, null) => PesoAfinidad * afinidad + PesoCalidad * Calidad(c) + PesoNovedad * novedad,
                        (false, double colab) => 0.5 * colab + 0.4 * Calidad(c) + 0.1 * novedad,
                        // Sin gustos ni modelo: lo mejor calificado por los viajeros
                        (false, null) => 0.85 * Calidad(c) + 0.15 * novedad
                    };

                    return new Puntuado(c, puntaje, afinidad, colaborativo);
                })
                .OrderByDescending(p => p.Puntaje)
                .ThenBy(p => p.Municipio.Nombre)
                .ToList();
        }

        /// <summary>
        /// Afinidad de 0 a 1: suma de los pesos de sus etiquetas en el perfil, dividida por √(cantidad de etiquetas)
        /// para no premiar al municipio que tiene muchas etiquetas. Luego se normaliza contra el mejor.
        /// </summary>
        private static Dictionary<int, double> CalcularAfinidades(List<Candidato> candidatos, Dictionary<int, AporteEtiqueta> perfil)
        {
            var crudas = candidatos.ToDictionary(
                c => c.IdMunicipio,
                c => c.Etiquetas.Count == 0
                    ? 0
                    : Math.Max(0, c.Etiquetas.Keys.Sum(id => perfil.TryGetValue(id, out var a) ? a.Peso : 0)
                                  / Math.Sqrt(c.Etiquetas.Count)));

            var maximaCruda = crudas.Values.DefaultIfEmpty(0).Max();
            return crudas.ToDictionary(c => c.Key, c => maximaCruda > 0 ? c.Value / maximaCruda : 0);
        }

        /// <summary>Distancia en línea recta entre dos puntos de la Tierra (fórmula de Haversine), en km.</summary>
        private static double DistanciaKm(Candidato a, Candidato b)
        {
            const double RadioTierraKm = 6371;
            static double Radianes(double grados) => grados * Math.PI / 180;

            var dLat = Radianes(b.Latitud!.Value - a.Latitud!.Value);
            var dLng = Radianes(b.Longitud!.Value - a.Longitud!.Value);

            var h = Math.Pow(Math.Sin(dLat / 2), 2)
                  + Math.Cos(Radianes(a.Latitud.Value)) * Math.Cos(Radianes(b.Latitud.Value)) * Math.Pow(Math.Sin(dLng / 2), 2);

            return 2 * RadioTierraKm * Math.Asin(Math.Sqrt(h));
        }

        /// <summary>Elige uno por uno el mejor, restando puntos a los que se parecen a los ya elegidos.</summary>
        private static List<Puntuado> Diversificar(List<Puntuado> opciones, int cantidad)
        {
            var elegidos = new List<Puntuado>();
            var restantes = new List<Puntuado>(opciones);

            while (elegidos.Count < cantidad && restantes.Count > 0)
            {
                var mejor = restantes.MaxBy(p =>
                    p.Puntaje - PenalizacionParecidos * elegidos.Select(e => Jaccard(e.Municipio, p.Municipio)).DefaultIfEmpty(0).Max())!;

                elegidos.Add(mejor);
                restantes.Remove(mejor);
            }

            return elegidos;
        }

        private static (string Razon, string Tipo) Explicar(
            Puntuado puntuado, Dictionary<int, AporteEtiqueta> perfil, HashSet<int> conocidos, bool esJoya)
        {
            var m = puntuado.Municipio;

            if (esJoya)
                return ("Joya escondida: encaja contigo y pocos la conocen", TiposRazon.Joya);

            if (conocidos.Contains(m.IdMunicipio))
                return ("Ya lo conoces: vale la pena volver", TiposRazon.Favorito);

            // ML.NET pesa más que los gustos: viajeros parecidos a este usuario lo califican muy alto
            if (puntuado.Colaborativo is double colab && colab >= ColaborativoMinimoRazon
                && PesoColaborativo * colab > PesoAfinidadConModelo * puntuado.Afinidad)
                return ("Viajeros con gustos como los tuyos lo califican alto", TiposRazon.Comunidad);

            // La etiqueta de este municipio que más pesa en los gustos del usuario
            var clave = m.Etiquetas.Keys
                .Where(id => perfil.TryGetValue(id, out var a) && a.Peso > 0)
                .OrderByDescending(id => perfil[id].Peso)
                .Select(id => (int?)id)
                .FirstOrDefault();

            if (clave is int idEtiqueta && puntuado.Afinidad >= AfinidadMinimaRazon)
            {
                var aporte = perfil[idEtiqueta];
                return aporte.Tipo switch
                {
                    TiposRazon.Favorito => ($"Porque guardaste {aporte.Origen} en favoritos", TiposRazon.Favorito),
                    TiposRazon.Calificado => ($"Porque calificaste bien {aporte.Origen}", TiposRazon.Calificado),
                    TiposRazon.Itinerario => ($"Parecido a {aporte.Origen}, de tu itinerario", TiposRazon.Itinerario),
                    _ => ($"Porque te gusta {m.Etiquetas[idEtiqueta]}", TiposRazon.Gusto)
                };
            }

            if (m.TotalResenas > 0)
                return ($"Los viajeros lo califican {m.Promedio:0.0} ★", TiposRazon.Popular);

            return ("Un destino por descubrir en Boyacá", TiposRazon.Nuevo);
        }

        /// <summary>
        /// Calidad de 0 a 1 con promedio bayesiano: cada municipio arranca con 3 reseñas imaginarias de 3.5★.
        /// Así 1 reseña de 5★ no le gana a 20 de 4.7★, y uno sin reseñas no supera a uno con 4★ reales.
        /// </summary>
        private static double Calidad(Candidato c)
        {
            var bayesiano = (ConfianzaBayesiana * PromedioNeutral + (c.Promedio ?? 0) * c.TotalResenas)
                            / (ConfianzaBayesiana + c.TotalResenas);

            return Math.Clamp((bayesiano - 1) / 4, 0, 1);
        }

        /// <summary>Qué tanto se parecen dos municipios por sus etiquetas (0 = nada, 1 = iguales).</summary>
        private static double Jaccard(Candidato a, Candidato b)
        {
            var union = a.Etiquetas.Keys.Union(b.Etiquetas.Keys).Count();
            return union == 0 ? 0 : (double)a.Etiquetas.Keys.Intersect(b.Etiquetas.Keys).Count() / union;
        }

        /// <summary>Estimación sin mapa: línea recta × factor de carretera, a velocidad de montaña (redondeado a 5 min).</summary>
        private static (int Km, int Minutos) EstimarTramo(double kmLineaRecta)
        {
            var km = (int)Math.Round(kmLineaRecta * FactorCarretera);
            var minutos = Math.Max(5, (int)Math.Round(km / VelocidadPromedioKmH * 60 / 5) * 5);
            return (km, minutos);
        }

        private static string FormatoDuracion(int minutos) =>
            minutos < 60 ? $"{minutos} min" : $"{minutos / 60} h {minutos % 60:00} min";

        private static RecomendacionDto CrearDto(Candidato c, double puntaje, string razon, string tipo) => new()
        {
            IdMunicipio = c.IdMunicipio,
            Nombre = c.Nombre,
            ImagenUrl = c.ImagenUrl,
            Descripcion = c.Descripcion,
            Etiquetas = c.Etiquetas.Values.ToList(),
            PromedioCalificacion = c.Promedio,
            TotalResenas = c.TotalResenas,
            Coincidencia = (int)Math.Round(Math.Clamp(puntaje, 0, 1) * 100),
            Razon = razon,
            TipoRazon = tipo
        };
    }
}
