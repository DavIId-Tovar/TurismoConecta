using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TurismoConecta.api.Constants;
using TurismoConecta.api.Data;
using TurismoConecta.api.DTOs.Visibilidad;
using TurismoConecta.api.Models;
using TurismoConecta.api.Services.Interfaces;

namespace TurismoConecta.api.Services;

/// <summary>
/// Estadísticas de visibilidad (panel del comercio), reporte municipal y comparativo general.
/// Las vistas y clics salen de EventoVisibilidad; favoritos, reseñas e itinerarios,
/// de sus propias tablas (no se duplican).
/// Todas las consultas agrupan en SQL por día y el resto se suma en memoria:
/// así cada reporte son pocas consultas y no depende de traducciones raras de EF.
/// </summary>
public class VisibilidadService : IVisibilidadService
{
    /// <summary>Periodos que ofrece la interfaz; cualquier otro valor se trata como 30.</summary>
    private static readonly int[] DiasPermitidos = { 7, 30, 90 };

    /// <summary>Una misma vista del mismo visitante no se cuenta dos veces en este lapso.</summary>
    private static readonly TimeSpan VentanaVistas = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan VentanaClics = TimeSpan.FromMinutes(10);

    /// <summary>"Viajeros próximos" = itinerarios con fechas dentro de estos días.</summary>
    private const int DiasViajerosProximos = 30;

    private readonly AppDbContext _context;
    public VisibilidadService(AppDbContext context) => _context = context;

    // ═════════════════════════════════════════════
    // REGISTRO DE EVENTOS
    // ═════════════════════════════════════════════

    public async Task<ResultadoVisibilidad> RegistrarAsync(RegistrarEventoDto dto, SolicitanteVisibilidad quien, CancellationToken ct)
    {
        if (!TiposEventoVisibilidad.EsValido(dto.Tipo)) return ResultadoVisibilidad.Invalido;

        int idMunicipio;
        int? idNegocio = null;

        if (dto.Tipo == TiposEventoVisibilidad.VistaMunicipio)
        {
            if (dto.IdMunicipio is not int id) return ResultadoVisibilidad.Invalido;
            if (!await _context.Municipios.AnyAsync(m => m.IdMunicipio == id && m.Activo, ct))
                return ResultadoVisibilidad.NoEncontrado;
            idMunicipio = id;
        }
        else
        {
            if (dto.IdNegocio is not int id) return ResultadoVisibilidad.Invalido;
            var negocio = await _context.Negocios.AsNoTracking()
                .Where(n => n.IdNegocio == id)
                .Select(n => new { n.IdMunicipio, n.IdUsuario, n.Estado })
                .FirstOrDefaultAsync(ct);
            // Solo cuentan los negocios que el público puede ver
            if (negocio is null || negocio.Estado != EstadosNegocio.Aprobado) return ResultadoVisibilidad.NoEncontrado;

            // El dueño mirando su propio negocio no infla sus números
            if (quien.IdUsuario.HasValue && negocio.IdUsuario == quien.IdUsuario) return ResultadoVisibilidad.Ignorado;

            idMunicipio = negocio.IdMunicipio;
            idNegocio = id;
        }

        // Sin forma de reconocer repetidos no se cuenta (evita que un script sume vistas sin límite)
        if (dto.IdVisitante is null && quien.IdUsuario is null) return ResultadoVisibilidad.Ignorado;

        // El equipo que administra la plataforma o el municipio tampoco cuenta
        if (quien.EsAdminGeneral) return ResultadoVisibilidad.Ignorado;
        if (quien.EsAdminMunicipio && quien.IdUsuario is int idAdmin
            && await MunicipioAsignadoAsync(idAdmin, ct) == idMunicipio)
            return ResultadoVisibilidad.Ignorado;

        var ahora = DateTime.Now;
        var limite = ahora - (dto.Tipo is TiposEventoVisibilidad.VistaMunicipio or TiposEventoVisibilidad.VistaNegocio
            ? VentanaVistas
            : VentanaClics);

        var repetido = await _context.EventosVisibilidad.AnyAsync(e =>
            e.Fecha >= limite
            && e.Tipo == dto.Tipo
            && e.IdMunicipio == idMunicipio
            && e.IdNegocio == idNegocio
            && ((dto.IdVisitante != null && e.IdVisitante == dto.IdVisitante)
                || (quien.IdUsuario != null && e.IdUsuario == quien.IdUsuario)), ct);
        if (repetido) return ResultadoVisibilidad.Ignorado;

        _context.EventosVisibilidad.Add(new EventoVisibilidad
        {
            Tipo = dto.Tipo,
            IdMunicipio = idMunicipio,
            IdNegocio = idNegocio,
            IdUsuario = quien.IdUsuario,
            IdVisitante = dto.IdVisitante,
            Fecha = ahora
        });
        await _context.SaveChangesAsync(ct);
        return ResultadoVisibilidad.Ok;
    }

    // ═════════════════════════════════════════════
    // PANEL DEL COMERCIO
    // ═════════════════════════════════════════════

    public async Task<ResumenComercioDto> ResumenComercioAsync(int idUsuario, int dias, CancellationToken ct)
    {
        var p = Periodo.De(dias);

        var negocios = await _context.Negocios.AsNoTracking()
            .Where(n => n.IdUsuario == idUsuario)
            .OrderBy(n => n.Nombre)
            .Select(n => new { n.IdNegocio, n.Nombre, n.Estado, n.IdMunicipio, NombreMunicipio = n.IdMunicipioNavigation.Nombre })
            .ToListAsync(ct);

        var resumen = new ResumenComercioDto { Periodo = p.ADto() };
        if (negocios.Count == 0) return resumen;

        var ids = negocios.Select(n => n.IdNegocio).ToList();
        var idsMunicipios = negocios.Select(n => n.IdMunicipio).Distinct().ToList();

        var eventos = await ContarEventosAsync(
            _context.EventosVisibilidad.Where(e => e.IdNegocio != null && ids.Contains(e.IdNegocio.Value)), p, ct);
        var favoritos = await FavoritosAsync(_context.Favoritos.Where(f => f.IdNegocio != null && ids.Contains(f.IdNegocio.Value)), ct);
        var resenas = await ResenasAsync(_context.Reseñas.Where(r => r.IdNegocio != null && ids.Contains(r.IdNegocio.Value)), ct);
        var viajeros = await ViajerosProximosAsync(idsMunicipios, ct);

        foreach (var n in negocios)
        {
            var ev = eventos.Where(e => e.IdNegocio == n.IdNegocio).ToList();
            var fav = favoritos.Where(f => f.IdNegocio == n.IdNegocio).ToList();
            var res = resenas.Where(r => r.IdNegocio == n.IdNegocio).ToList();
            var resPeriodo = res.Where(r => r.Fecha >= p.Desde).ToList();

            resumen.Negocios.Add(new VisibilidadNegocioDto
            {
                IdNegocio = n.IdNegocio,
                Nombre = n.Nombre,
                NombreMunicipio = n.NombreMunicipio,
                Estado = n.Estado,
                Vistas = Medir(ev, p, TiposEventoVisibilidad.VistaNegocio),
                WhatsApp = Medir(ev, p, TiposEventoVisibilidad.ClicWhatsApp),
                Telefono = Medir(ev, p, TiposEventoVisibilidad.ClicTelefono),
                ComoLlegar = Medir(ev, p, TiposEventoVisibilidad.ClicComoLlegar),
                Contactos = Medir(ev, p, TiposEventoVisibilidad.Contactos),
                FavoritosNuevos = MedirFechas(fav.Select(f => f.Fecha), p),
                FavoritosTotal = fav.Count,
                ResenasNuevas = MedirFechas(res.Select(r => r.Fecha), p),
                ResenasTotal = res.Count,
                Promedio = Promedio(res.Select(r => r.Calificacion)),
                PromedioPeriodo = Promedio(resPeriodo.Select(r => r.Calificacion)),
                ViajerosProximos = viajeros.GetValueOrDefault(n.IdMunicipio),
                Serie = ArmarSerie(ev, p)
            });
        }

        return resumen;
    }

    // ═════════════════════════════════════════════
    // REPORTE MUNICIPAL
    // ═════════════════════════════════════════════

    public Task<int?> MunicipioAsignadoAsync(int idUsuario, CancellationToken ct) =>
        _context.Usuarios
            .Where(u => u.IdUsuario == idUsuario)
            .Select(u => u.MunicipioAsignadoId)
            .FirstOrDefaultAsync(ct);

    public async Task<(ResultadoVisibilidad Resultado, ReporteMunicipalDto? Reporte)> ReporteMunicipalAsync(
        int idMunicipio, SolicitanteVisibilidad quien, int dias, CancellationToken ct)
    {
        if (!quien.EsAdminGeneral)
        {
            if (!quien.EsAdminMunicipio || quien.IdUsuario is not int idUsuario
                || await MunicipioAsignadoAsync(idUsuario, ct) != idMunicipio)
                return (ResultadoVisibilidad.SinPermiso, null);
        }

        var nombre = await _context.Municipios.AsNoTracking()
            .Where(m => m.IdMunicipio == idMunicipio)
            .Select(m => m.Nombre)
            .FirstOrDefaultAsync(ct);
        if (nombre is null) return (ResultadoVisibilidad.NoEncontrado, null);

        var p = Periodo.De(dias);
        var ahora = DateTime.Now;

        var eventos = await ContarEventosAsync(_context.EventosVisibilidad.Where(e => e.IdMunicipio == idMunicipio), p, ct);

        // Favoritos y reseñas: del municipio o de cualquiera de sus negocios
        var favoritos = await FavoritosAsync(_context.Favoritos.Where(f =>
            f.IdMunicipio == idMunicipio || (f.IdNegocio != null && f.IdNegocioNavigation!.IdMunicipio == idMunicipio)), ct);
        var resenas = await ResenasAsync(_context.Reseñas.Where(r =>
            r.IdMunicipio == idMunicipio || (r.IdNegocio != null && r.IdNegocioNavigation!.IdMunicipio == idMunicipio)), ct);
        var retenidas = await _context.Reseñas.CountAsync(r => !r.Moderada &&
            (r.IdMunicipio == idMunicipio || (r.IdNegocio != null && r.IdNegocioNavigation!.IdMunicipio == idMunicipio)), ct);

        var itinerarios = await ItinerariosCreadosAsync(new[] { idMunicipio }, p, ct);
        var viajeros = await ViajerosProximosAsync(new[] { idMunicipio }, ct);

        var estados = await _context.Negocios.AsNoTracking()
            .Where(n => n.IdMunicipio == idMunicipio)
            .GroupBy(n => n.Estado)
            .Select(g => new { Estado = g.Key, Total = g.Count() })
            .ToListAsync(ct);

        var alertasVigentes = await _context.AlertasMunicipales
            .CountAsync(a => a.IdMunicipio == idMunicipio && a.Activa && a.FechaFin > ahora, ct);

        var aprobados = await _context.Negocios.AsNoTracking()
            .Where(n => n.IdMunicipio == idMunicipio && n.Estado == EstadosNegocio.Aprobado)
            .Select(n => new { n.IdNegocio, n.Nombre, Categoria = n.IdCategoriaNavigation.Nombre })
            .ToListAsync(ct);

        var resMunicipio = resenas.Where(r => r.IdNegocio == null).ToList();

        var filas = aprobados.Select(n =>
        {
            var ev = eventos.Where(e => e.IdNegocio == n.IdNegocio).ToList();
            var res = resenas.Where(r => r.IdNegocio == n.IdNegocio).ToList();
            return new FilaNegocioReporteDto
            {
                IdNegocio = n.IdNegocio,
                Nombre = n.Nombre,
                Categoria = n.Categoria,
                Vistas = Medir(ev, p, TiposEventoVisibilidad.VistaNegocio).Actual,
                Contactos = Medir(ev, p, TiposEventoVisibilidad.Contactos).Actual,
                FavoritosTotal = favoritos.Count(f => f.IdNegocio == n.IdNegocio),
                ResenasTotal = res.Count,
                Promedio = Promedio(res.Select(r => r.Calificacion))
            };
        })
        .OrderByDescending(f => f.Vistas)
        .ThenByDescending(f => f.Contactos)
        .ThenBy(f => f.Nombre)
        .ToList();

        var reporte = new ReporteMunicipalDto
        {
            IdMunicipio = idMunicipio,
            NombreMunicipio = nombre,
            Periodo = p.ADto(),
            VistasFicha = Medir(eventos, p, TiposEventoVisibilidad.VistaMunicipio),
            VistasNegocios = Medir(eventos, p, TiposEventoVisibilidad.VistaNegocio),
            Contactos = Medir(eventos, p, TiposEventoVisibilidad.Contactos),
            FavoritosNuevos = MedirFechas(favoritos.Select(f => f.Fecha), p),
            ResenasNuevas = MedirFechas(resenas.Select(r => r.Fecha), p),
            ItinerariosNuevos = MedirFechas(itinerarios.Where(i => i.IdMunicipio == idMunicipio).Select(i => i.Fecha), p),
            ViajerosProximos = viajeros.GetValueOrDefault(idMunicipio),
            PromedioMunicipio = Promedio(resMunicipio.Select(r => r.Calificacion)),
            ResenasMunicipioTotal = resMunicipio.Count,
            ResenasRetenidas = retenidas,
            NegociosAprobados = estados.Where(e => e.Estado == EstadosNegocio.Aprobado).Sum(e => e.Total),
            NegociosPendientes = estados.Where(e => e.Estado == EstadosNegocio.Pendiente).Sum(e => e.Total),
            NegociosRechazados = estados.Where(e => e.Estado == EstadosNegocio.Rechazado).Sum(e => e.Total),
            NegociosSinVistas = filas.Count(f => f.Vistas == 0),
            AlertasVigentes = alertasVigentes,
            Serie = ArmarSerie(eventos, p),
            Negocios = filas
        };

        return (ResultadoVisibilidad.Ok, reporte);
    }

    // ═════════════════════════════════════════════
    // PANEL GENERAL
    // ═════════════════════════════════════════════

    public async Task<ResumenGeneralDto> ResumenGeneralAsync(int dias, CancellationToken ct)
    {
        var p = Periodo.De(dias);

        var municipios = await _context.Municipios.AsNoTracking()
            .Where(m => m.Activo)
            .OrderBy(m => m.Nombre)
            .Select(m => new { m.IdMunicipio, m.Nombre })
            .ToListAsync(ct);
        var ids = municipios.Select(m => m.IdMunicipio).ToList();

        var eventos = await ContarEventosAsync(_context.EventosVisibilidad.Where(e => ids.Contains(e.IdMunicipio)), p, ct);
        var itinerarios = await ItinerariosCreadosAsync(ids, p, ct);
        var resenas = await ResenasAsync(_context.Reseñas, ct);

        // Reseñas por municipio: las del municipio y las de sus negocios
        var municipioDeNegocio = await _context.Negocios.AsNoTracking()
            .Select(n => new { n.IdNegocio, n.IdMunicipio })
            .ToDictionaryAsync(n => n.IdNegocio, n => n.IdMunicipio, ct);
        int? MunicipioDe(FilaResena r) =>
            r.IdMunicipio ?? (r.IdNegocio is int idN && municipioDeNegocio.TryGetValue(idN, out var m) ? m : null);

        var negocios = await _context.Negocios.AsNoTracking()
            .GroupBy(n => new { n.IdMunicipio, n.Estado })
            .Select(g => new { g.Key.IdMunicipio, g.Key.Estado, Total = g.Count() })
            .ToListAsync(ct);

        var resumen = new ResumenGeneralDto
        {
            Periodo = p.ADto(),
            Vistas = Medir(eventos, p, TiposEventoVisibilidad.VistaMunicipio, TiposEventoVisibilidad.VistaNegocio),
            Contactos = Medir(eventos, p, TiposEventoVisibilidad.Contactos),
            // Un itinerario que pasa por 3 municipios cuenta una vez
            ItinerariosNuevos = MedirFechas(itinerarios.DistinctBy(i => i.IdItinerario).Select(i => i.Fecha), p),
            ResenasNuevas = MedirFechas(resenas.Select(r => r.Fecha), p),
            Serie = ArmarSerie(eventos, p)
        };

        foreach (var m in municipios)
        {
            var ev = eventos.Where(e => e.IdMunicipio == m.IdMunicipio).ToList();
            var res = resenas.Where(r => MunicipioDe(r) == m.IdMunicipio).ToList();
            resumen.Municipios.Add(new FilaMunicipioGeneralDto
            {
                IdMunicipio = m.IdMunicipio,
                Nombre = m.Nombre,
                VistasFicha = Medir(ev, p, TiposEventoVisibilidad.VistaMunicipio).Actual,
                VistasNegocios = Medir(ev, p, TiposEventoVisibilidad.VistaNegocio).Actual,
                Contactos = Medir(ev, p, TiposEventoVisibilidad.Contactos).Actual,
                ItinerariosNuevos = MedirFechas(itinerarios.Where(i => i.IdMunicipio == m.IdMunicipio).Select(i => i.Fecha), p).Actual,
                NegociosAprobados = negocios.Where(n => n.IdMunicipio == m.IdMunicipio && n.Estado == EstadosNegocio.Aprobado).Sum(n => n.Total),
                NegociosPendientes = negocios.Where(n => n.IdMunicipio == m.IdMunicipio && n.Estado == EstadosNegocio.Pendiente).Sum(n => n.Total),
                Promedio = Promedio(res.Select(r => r.Calificacion)),
                ResenasTotal = res.Count
            });
        }

        resumen.Municipios = resumen.Municipios
            .OrderByDescending(f => f.VistasFicha + f.VistasNegocios)
            .ThenBy(f => f.Nombre)
            .ToList();

        return resumen;
    }

    // ═════════════════════════════════════════════
    // CSV
    // ═════════════════════════════════════════════

    public string GenerarCsv(ReporteMunicipalDto r)
    {
        var sb = new StringBuilder();
        void Linea(params object?[] celdas) => sb.AppendLine(string.Join(";", celdas.Select(Celda)));

        Linea("Reporte municipal", r.NombreMunicipio);
        Linea("Periodo", $"{r.Periodo.Desde:yyyy-MM-dd} a {r.Periodo.Hasta:yyyy-MM-dd} ({r.Periodo.Dias} días)");
        Linea("Generado", DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
        sb.AppendLine();

        Linea("Indicador", "Periodo actual", "Periodo anterior");
        Linea("Vistas de la ficha del municipio", r.VistasFicha.Actual, r.VistasFicha.Anterior);
        Linea("Vistas de establecimientos", r.VistasNegocios.Actual, r.VistasNegocios.Anterior);
        Linea("Contactos (WhatsApp, llamada, cómo llegar)", r.Contactos.Actual, r.Contactos.Anterior);
        Linea("Favoritos nuevos", r.FavoritosNuevos.Actual, r.FavoritosNuevos.Anterior);
        Linea("Reseñas nuevas", r.ResenasNuevas.Actual, r.ResenasNuevas.Anterior);
        Linea("Itinerarios nuevos que incluyen el municipio", r.ItinerariosNuevos.Actual, r.ItinerariosNuevos.Anterior);
        sb.AppendLine();

        Linea("Situación actual", "Valor");
        Linea($"Viajeros con visita en los próximos {DiasViajerosProximos} días", r.ViajerosProximos);
        Linea("Calificación promedio del municipio", r.PromedioMunicipio);
        Linea("Reseñas del municipio", r.ResenasMunicipioTotal);
        Linea("Reseñas retenidas por moderación", r.ResenasRetenidas);
        Linea("Establecimientos aprobados", r.NegociosAprobados);
        Linea("Establecimientos pendientes", r.NegociosPendientes);
        Linea("Establecimientos rechazados", r.NegociosRechazados);
        Linea("Aprobados sin vistas en el periodo", r.NegociosSinVistas);
        Linea("Alertas vigentes o próximas", r.AlertasVigentes);
        sb.AppendLine();

        Linea("Establecimiento", "Categoría", "Vistas", "Contactos", "Favoritos", "Reseñas", "Calificación");
        foreach (var n in r.Negocios)
            Linea(n.Nombre, n.Categoria, n.Vistas, n.Contactos, n.FavoritosTotal, n.ResenasTotal, n.Promedio);
        sb.AppendLine();

        Linea("Día", "Vistas", "Contactos");
        foreach (var d in r.Serie)
            Linea(d.Fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), d.Vistas, d.Contactos);

        return sb.ToString();
    }

    /// <summary>Escapa ; " y saltos de línea, y evita que Excel ejecute fórmulas (=, +, -, @).</summary>
    private static string Celda(object? valor)
    {
        var texto = valor switch
        {
            null => "",
            double d => d.ToString("0.0", CultureInfo.GetCultureInfo("es-CO")),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => valor.ToString() ?? ""
        };

        if (valor is string && texto.Length > 0 && "=+-@".Contains(texto[0])) texto = "'" + texto;

        return texto.IndexOfAny(new[] { ';', '"', '\n', '\r' }) >= 0
            ? "\"" + texto.Replace("\"", "\"\"") + "\""
            : texto;
    }

    // ═════════════════════════════════════════════
    // CONSULTAS Y AYUDANTES
    // ═════════════════════════════════════════════

    /// <summary>Periodo actual [Desde, Hasta) y el anterior [DesdeAnterior, Desde), ambos de Dias días.</summary>
    private sealed record Periodo(int Dias, DateTime Desde, DateTime Hasta, DateTime DesdeAnterior)
    {
        public static Periodo De(int dias)
        {
            dias = DiasPermitidos.Contains(dias) ? dias : 30;
            var hasta = DateTime.Today.AddDays(1);   // incluye todo el día de hoy
            var desde = hasta.AddDays(-dias);
            return new Periodo(dias, desde, hasta, desde.AddDays(-dias));
        }

        public PeriodoDto ADto() => new()
        {
            Dias = Dias,
            Desde = DateOnly.FromDateTime(Desde),
            Hasta = DateOnly.FromDateTime(Hasta.AddDays(-1))
        };
    }

    private sealed record FilaEvento(int IdMunicipio, int? IdNegocio, string Tipo, DateTime Dia, int Total);
    private sealed record FilaFavorito(int? IdNegocio, DateTime Fecha);
    private sealed record FilaResena(int? IdMunicipio, int? IdNegocio, byte Calificacion, DateTime Fecha);
    private sealed record FilaItinerario(int IdItinerario, int IdMunicipio, DateTime Fecha);

    /// <summary>Eventos de los dos periodos agrupados por lugar, tipo y día.</summary>
    private static Task<List<FilaEvento>> ContarEventosAsync(IQueryable<EventoVisibilidad> query, Periodo p, CancellationToken ct) =>
        query.AsNoTracking()
            .Where(e => e.Fecha >= p.DesdeAnterior && e.Fecha < p.Hasta)
            .GroupBy(e => new { e.IdMunicipio, e.IdNegocio, e.Tipo, Dia = e.Fecha.Date })
            .Select(g => new FilaEvento(g.Key.IdMunicipio, g.Key.IdNegocio, g.Key.Tipo, g.Key.Dia, g.Count()))
            .ToListAsync(ct);

    /// <summary>Todos los favoritos (para el total); la fecha permite separar los del periodo.</summary>
    private static Task<List<FilaFavorito>> FavoritosAsync(IQueryable<Favorito> query, CancellationToken ct) =>
        query.AsNoTracking()
            .Select(f => new FilaFavorito(f.IdNegocio, f.FechaGuardado))
            .ToListAsync(ct);

    /// <summary>Reseñas visibles (las retenidas por moderación no cuentan para la calificación).</summary>
    private static Task<List<FilaResena>> ResenasAsync(IQueryable<Reseña> query, CancellationToken ct) =>
        query.AsNoTracking()
            .Where(r => r.Moderada)
            .Select(r => new FilaResena(r.IdMunicipio, r.IdNegocio, r.Calificacion, r.FechaCreacion))
            .ToListAsync(ct);

    /// <summary>Itinerarios creados en los dos periodos que pasan por esos municipios (uno por municipio).</summary>
    private async Task<List<FilaItinerario>> ItinerariosCreadosAsync(IReadOnlyCollection<int> idsMunicipios, Periodo p, CancellationToken ct)
    {
        var filas = await _context.ItinerarioDetalles.AsNoTracking()
            .Where(d => idsMunicipios.Contains(d.IdMunicipio)
                        && d.IdItinerarioNavigation.FechaCreacion >= p.DesdeAnterior
                        && d.IdItinerarioNavigation.FechaCreacion < p.Hasta)
            .Select(d => new { d.IdItinerario, d.IdMunicipio, d.IdItinerarioNavigation.FechaCreacion })
            .Distinct()
            .ToListAsync(ct);

        return filas.Select(f => new FilaItinerario(f.IdItinerario, f.IdMunicipio, f.FechaCreacion)).ToList();
    }

    /// <summary>Por municipio: itinerarios cuyas fechas caen (o se cruzan) con los próximos 30 días.</summary>
    private async Task<Dictionary<int, int>> ViajerosProximosAsync(IReadOnlyCollection<int> idsMunicipios, CancellationToken ct)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var limite = hoy.AddDays(DiasViajerosProximos);

        var pares = await _context.ItinerarioDetalles.AsNoTracking()
            .Where(d => idsMunicipios.Contains(d.IdMunicipio)
                        && d.IdItinerarioNavigation.FechaInicio != null
                        && d.IdItinerarioNavigation.FechaInicio <= limite
                        && (d.IdItinerarioNavigation.FechaFin ?? d.IdItinerarioNavigation.FechaInicio) >= hoy)
            .Select(d => new { d.IdMunicipio, d.IdItinerario })
            .Distinct()
            .ToListAsync(ct);

        return pares.GroupBy(x => x.IdMunicipio).ToDictionary(g => g.Key, g => g.Count());
    }

    private static MetricaDto Medir(IEnumerable<FilaEvento> filas, Periodo p, params string[] tipos) =>
        Medir(filas, p, (IReadOnlySet<string>)tipos.ToHashSet());

    private static MetricaDto Medir(IEnumerable<FilaEvento> filas, Periodo p, IReadOnlySet<string> tipos)
    {
        var m = new MetricaDto();
        foreach (var f in filas)
        {
            if (!tipos.Contains(f.Tipo)) continue;
            if (f.Dia >= p.Desde) m.Actual += f.Total;
            else m.Anterior += f.Total;
        }
        return m;
    }

    private static MetricaDto MedirFechas(IEnumerable<DateTime> fechas, Periodo p)
    {
        var m = new MetricaDto();
        foreach (var f in fechas)
        {
            if (f >= p.Desde && f < p.Hasta) m.Actual++;
            else if (f >= p.DesdeAnterior && f < p.Desde) m.Anterior++;
        }
        return m;
    }

    private static double? Promedio(IEnumerable<byte> calificaciones)
    {
        var lista = calificaciones.ToList();
        return lista.Count == 0 ? null : Math.Round(lista.Average(c => (double)c), 1);
    }

    /// <summary>Un punto por día del periodo actual (también los días sin movimiento).</summary>
    private static List<PuntoSerieDto> ArmarSerie(IEnumerable<FilaEvento> filas, Periodo p)
    {
        var porDia = filas.Where(f => f.Dia >= p.Desde)
            .GroupBy(f => f.Dia)
            .ToDictionary(g => g.Key, g => g.ToList());

        var serie = new List<PuntoSerieDto>(p.Dias);
        for (var dia = p.Desde; dia < p.Hasta; dia = dia.AddDays(1))
        {
            var punto = new PuntoSerieDto { Fecha = DateOnly.FromDateTime(dia) };
            if (porDia.TryGetValue(dia, out var delDia))
            {
                punto.Vistas = delDia.Where(f => f.Tipo is TiposEventoVisibilidad.VistaMunicipio or TiposEventoVisibilidad.VistaNegocio).Sum(f => f.Total);
                punto.Contactos = delDia.Where(f => TiposEventoVisibilidad.Contactos.Contains(f.Tipo)).Sum(f => f.Total);
            }
            serie.Add(punto);
        }
        return serie;
    }
}
