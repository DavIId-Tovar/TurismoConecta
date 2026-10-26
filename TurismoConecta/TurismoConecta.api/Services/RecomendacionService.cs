using Microsoft.EntityFrameworkCore;
using TurismoConecta.api.Data;
using TurismoConecta.api.DTOs.Recomendaciones;
using TurismoConecta.api.Models;
using TurismoConecta.api.Services.Interfaces;

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

        // ---- Cuánto pesa cada parte del puntaje final (la Entrega 2 suma el colaborativo de ML.NET) ----
        private const double PesoAfinidad = 0.55;
        private const double PesoCalidad = 0.30;
        private const double PesoNovedad = 0.15;

        private const double PenalizacionParecidos = 0.25;  // diversidad: baja a los muy parecidos a los ya elegidos
        private const int ConfianzaBayesiana = 3;           // "reseñas imaginarias" de 3.5★ que tiene todo municipio
        private const double PromedioNeutral = 3.5;
        private const double AfinidadMinimaRazon = 0.35;    // por debajo, la razón no se atribuye a los gustos
        private const int MaximoPreferencias = 10;

        private readonly AppDbContext _context;
        public RecomendacionService(AppDbContext context) => _context = context;

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
            int TotalResenas);

        private sealed record Puntuado(Candidato Municipio, double Puntaje, double Afinidad);

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
            var (perfil, conocidos) = await ConstruirPerfilAsync(idUsuario, candidatos, ct);
            return Clasificar(candidatos, perfil, conocidos, cantidad);
        }

        public async Task<List<RecomendacionDto>> PorGustosAsync(List<int> idsEtiquetas, int cantidad, CancellationToken ct = default)
        {
            cantidad = Math.Clamp(cantidad, 1, 12);
            var candidatos = await CargarCandidatosAsync(ct);

            var nombres = candidatos
                .SelectMany(c => c.Etiquetas)
                .GroupBy(e => e.Key)
                .ToDictionary(g => g.Key, g => g.First().Value);

            var perfil = new Dictionary<int, AporteEtiqueta>();
            foreach (var idEtiqueta in idsEtiquetas.Distinct().Take(MaximoPreferencias))
                Sumar(perfil, idEtiqueta, PesoGustoDeclarado, TiposRazon.Gusto, nombres.GetValueOrDefault(idEtiqueta, ""));

            return Clasificar(candidatos, perfil, new HashSet<int>(), cantidad);
        }

        public async Task<List<RecomendacionDto>> SimilaresAsync(int idMunicipio, int cantidad, CancellationToken ct = default)
        {
            cantidad = Math.Clamp(cantidad, 1, 8);
            var candidatos = await CargarCandidatosAsync(ct);

            var origen = candidatos.FirstOrDefault(c => c.IdMunicipio == idMunicipio);
            if (origen is null) return new();

            return candidatos
                .Where(c => c.IdMunicipio != idMunicipio)
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
                    Total = m.Reseñas.Count(r => r.Moderada)
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
                    d.Total))
                .ToList();
        }

        /// <summary>
        /// El perfil de gustos sale de 4 señales: gustos elegidos, favoritos, reseñas propias e itinerarios.
        /// "Conocidos" son los municipios con los que el usuario ya interactuó (se recomiendan al final).
        /// </summary>
        private async Task<(Dictionary<int, AporteEtiqueta> Perfil, HashSet<int> Conocidos)> ConstruirPerfilAsync(
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

            return (perfil, conocidos);

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
            List<Candidato> candidatos, Dictionary<int, AporteEtiqueta> perfil, HashSet<int> conocidos, int cantidad)
        {
            var hayPerfil = perfil.Values.Any(a => a.Peso > 0);

            // Afinidad: suma de los pesos de sus etiquetas, dividida por √(cantidad de etiquetas)
            // para no premiar al municipio que tiene muchas etiquetas. Luego se normaliza de 0 a 1.
            var crudas = candidatos.ToDictionary(
                c => c.IdMunicipio,
                c => c.Etiquetas.Count == 0
                    ? 0
                    : Math.Max(0, c.Etiquetas.Keys.Sum(id => perfil.TryGetValue(id, out var a) ? a.Peso : 0)
                                  / Math.Sqrt(c.Etiquetas.Count)));

            var maximaCruda = crudas.Values.DefaultIfEmpty(0).Max();

            var puntuados = candidatos
                .Select(c =>
                {
                    var afinidad = maximaCruda > 0 ? crudas[c.IdMunicipio] / maximaCruda : 0;
                    var novedad = conocidos.Contains(c.IdMunicipio) ? 0 : 1;

                    // Sin gustos todavía: lo mejor calificado por los viajeros
                    var puntaje = hayPerfil
                        ? PesoAfinidad * afinidad + PesoCalidad * Calidad(c) + PesoNovedad * novedad
                        : 0.85 * Calidad(c) + 0.15 * novedad;

                    return new Puntuado(c, puntaje, afinidad);
                })
                .OrderByDescending(p => p.Puntaje)
                .ThenBy(p => p.Municipio.Nombre)
                .ToList();

            // Primero lo nuevo para el usuario; lo que ya conoce solo completa si faltan opciones
            var nuevos = puntuados.Where(p => !conocidos.Contains(p.Municipio.IdMunicipio)).ToList();
            // Diversidad solo cuando hay gustos; sin gustos el orden es "mejor calificados" y se respeta tal cual
            var elegidos = hayPerfil ? Diversificar(nuevos, cantidad) : nuevos.Take(cantidad).ToList();
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
