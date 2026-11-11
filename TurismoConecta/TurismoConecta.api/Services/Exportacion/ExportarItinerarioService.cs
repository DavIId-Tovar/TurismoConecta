using Microsoft.EntityFrameworkCore;
using TurismoConecta.api.Data;
using TurismoConecta.api.DTOs.Itinerarios;
using TurismoConecta.api.Services.Interfaces;

namespace TurismoConecta.api.Services.Exportacion;

public interface IExportarItinerarioService
{
    /// <summary>Itinerario del usuario en sesión (null si no existe o no es suyo).</summary>
    Task<ItinerarioExportable?> ObtenerAsync(int idItinerario, int idUsuario, CancellationToken ct = default);

    /// <summary>Itinerario público por su enlace (null si no existe o dejó de ser público).</summary>
    Task<ItinerarioExportable?> ObtenerCompartidoAsync(Guid codigo, CancellationToken ct = default);

    byte[] GenerarPdf(ItinerarioExportable itinerario);

    /// <summary>Archivo .ics; null si el itinerario no tiene fechas.</summary>
    string? GenerarIcs(ItinerarioExportable itinerario);

    /// <summary>Nombre de archivo seguro: "ruta-colonial-y-artesanal.pdf".</summary>
    string NombreArchivo(ItinerarioExportable itinerario, string extension);
}

/// <summary>
/// Exportar itinerario (Módulo 5 del documento técnico): PDF para imprimir o llevar
/// en el celular, y .ics para Google Calendar / Outlook / Calendario de Apple.
/// Reúne los datos UNA vez y ambos formatos los leen del mismo modelo.
/// </summary>
public class ExportarItinerarioService : IExportarItinerarioService
{
    private const int MaxSitiosPorParada = 4;

    private readonly AppDbContext _db;
    private readonly IItinerarioService _itinerarios;
    private readonly IAlertaService _alertas;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public ExportarItinerarioService(AppDbContext db, IItinerarioService itinerarios, IAlertaService alertas,
        IConfiguration config, IWebHostEnvironment env)
    {
        _db = db;
        _itinerarios = itinerarios;
        _alertas = alertas;
        _config = config;
        _env = env;
    }

    public async Task<ItinerarioExportable?> ObtenerAsync(int idItinerario, int idUsuario, CancellationToken ct = default)
    {
        // Mismo servicio que la página de detalle: así los tramos (km/min) salen iguales
        var dto = await _itinerarios.ObtenerDetalleAsync(idItinerario, idUsuario);
        return dto is null ? null : await ArmarAsync(dto, ct);
    }

    public async Task<ItinerarioExportable?> ObtenerCompartidoAsync(Guid codigo, CancellationToken ct = default)
    {
        var dto = await _itinerarios.ObtenerPorCodigoCompartirAsync(codigo);
        return dto is null ? null : await ArmarAsync(dto, ct);
    }

    public byte[] GenerarPdf(ItinerarioExportable itinerario) => new ItinerarioPdf(itinerario).Generar();

    public string? GenerarIcs(ItinerarioExportable itinerario) =>
        itinerario.TieneFechas ? ItinerarioIcs.Generar(itinerario, DateTime.UtcNow) : null;

    public string NombreArchivo(ItinerarioExportable itinerario, string extension)
    {
        var limpio = new string(itinerario.Nombre.Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                        != System.Globalization.UnicodeCategory.NonSpacingMark)
            .Select(c => char.IsLetterOrDigit(c) && c < 128 ? char.ToLowerInvariant(c) : '-')
            .ToArray());

        while (limpio.Contains("--")) limpio = limpio.Replace("--", "-");
        limpio = limpio.Trim('-');
        if (limpio.Length > 60) limpio = limpio[..60].TrimEnd('-');

        return $"{(limpio.Length == 0 ? "itinerario" : limpio)}.{extension}";
    }

    // ───────────────────────── Reunir los datos ─────────────────────────

    private async Task<ItinerarioExportable> ArmarAsync(ItinerarioResponseDto dto, CancellationToken ct)
    {
        var ids = dto.Paradas.Select(p => p.IdMunicipio).Distinct().ToList();

        var municipios = await _db.Municipios.AsNoTracking()
            .Where(m => ids.Contains(m.IdMunicipio))
            .Select(m => new
            {
                m.IdMunicipio,
                m.Descripcion,
                m.Clima,
                m.ImagenUrl,
                m.Latitud,
                m.Longitud,
                Departamento = m.IdDepartamentoNavigation.Nombre
            })
            .ToDictionaryAsync(m => m.IdMunicipio, ct);

        // Sitios: primero los destacados, luego por nombre
        var sitios = (await _db.SitiosTuristicos.AsNoTracking()
                .Where(s => ids.Contains(s.IdMunicipio) && s.Activo)
                .OrderByDescending(s => s.Destacado).ThenBy(s => s.Nombre)
                .Select(s => new { s.IdMunicipio, s.Nombre })
                .ToListAsync(ct))
            .GroupBy(s => s.IdMunicipio)
            .ToDictionary(g => g.Key, g => g.Select(s => s.Nombre).Take(MaxSitiosPorParada).ToList());

        // Fecha de cada día: la de la parada o, si no la tiene, inicio del viaje + (día - 1)
        DateOnly? FechaDe(ParadaResponseDto p) =>
            p.FechaVisita ?? dto.FechaInicio?.AddDays(p.DiaNumero - 1);

        var fechas = dto.Paradas.Select(FechaDe).Where(f => f is not null).Select(f => f!.Value).ToList();
        DateTime? desde = fechas.Count > 0 ? fechas.Min().ToDateTime(TimeOnly.MinValue) : null;
        DateTime? hasta = fechas.Count > 0 ? fechas.Max().ToDateTime(TimeOnly.MinValue).AddDays(1) : null;

        // Sin fechas: los avisos vigentes desde hoy (mismo criterio que AvisosRuta en la web)
        var alertas = await _alertas.ListarVigentesAsync(ids, desde, hasta, ct);

        var dias = dto.Paradas
            .OrderBy(p => p.DiaNumero).ThenBy(p => p.Orden)
            .GroupBy(p => p.DiaNumero)
            .Select(g =>
            {
                var fecha = g.Select(FechaDe).FirstOrDefault(f => f is not null);
                return new DiaExportable
                {
                    Numero = g.Key,
                    Fecha = fecha,
                    Paradas = g.Select(p =>
                    {
                        municipios.TryGetValue(p.IdMunicipio, out var m);
                        return new ParadaExportable
                        {
                            IdMunicipio = p.IdMunicipio,
                            Municipio = p.NombreMunicipio,
                            Departamento = m?.Departamento,
                            Descripcion = m?.Descripcion,
                            Clima = m?.Clima,
                            Latitud = m?.Latitud,
                            Longitud = m?.Longitud,
                            KmDesdeAnterior = p.DistanciaKm,
                            MinutosDesdeAnterior = p.TiempoEstimadoMin,
                            RutaImagen = RutaFisica(m?.ImagenUrl ?? p.ImagenUrl),
                            SitiosDestacados = sitios.GetValueOrDefault(p.IdMunicipio) ?? new(),
                            Avisos = alertas
                                .Where(a => a.IdMunicipio == p.IdMunicipio && CruzaConElDia(a.FechaInicio, a.FechaFin, fecha))
                                .Select(a => new AvisoExportable
                                {
                                    Titulo = a.Titulo,
                                    Descripcion = a.Descripcion,
                                    Severidad = a.Severidad,
                                    Desde = a.FechaInicio,
                                    Hasta = a.FechaFin,
                                    Negocio = a.NombreNegocio
                                })
                                .ToList()
                        };
                    }).ToList()
                };
            })
            .ToList();

        var frontend = (_config["App:FrontendUrl"] ?? "").TrimEnd('/');
        return new ItinerarioExportable
        {
            IdItinerario = dto.IdItinerario,
            Nombre = dto.Nombre,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            Observaciones = dto.Observaciones,
            EsCompartido = dto.Compartido,
            EnlaceWeb = dto.Compartido
                ? $"{frontend}/itinerarios/compartido/{dto.CodigoCompartir}"
                : $"{frontend}/itinerarios/{dto.IdItinerario}",
            Dias = dias
        };
    }

    // Un aviso aplica al día si se cruza con él; sin fecha, todos los vigentes
    private static bool CruzaConElDia(DateTime inicio, DateTime fin, DateOnly? dia)
    {
        if (dia is null) return true;
        var desde = dia.Value.ToDateTime(TimeOnly.MinValue);
        return inicio < desde.AddDays(1) && fin > desde;
    }

    /// <summary>
    /// "/images/municipios/x.jpg" o "https://localhost:7078/images/municipios/x.jpg" → archivo en wwwroot.
    /// Solo lee archivos dentro de wwwroot (nunca rutas que se salgan con "..").
    /// </summary>
    private string? RutaFisica(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrEmpty(_env.WebRootPath)) return null;

        var ruta = Uri.TryCreate(url, UriKind.Absolute, out var absoluta) ? absoluta.AbsolutePath : url;
        ruta = Uri.UnescapeDataString(ruta.Split('?', '#')[0]).TrimStart('/', '\\');

        var raiz = Path.GetFullPath(_env.WebRootPath);
        var completa = Path.GetFullPath(Path.Combine(raiz, ruta));

        return completa.StartsWith(raiz + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
               && File.Exists(completa)
            ? completa
            : null;
    }
}
