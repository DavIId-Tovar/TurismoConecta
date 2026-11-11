using TurismoConecta.api.DTOs.Visibilidad;

namespace TurismoConecta.api.Services.Interfaces;

public enum ResultadoVisibilidad
{
    Ok,
    /// <summary>Evento válido que no se cuenta (repetido, del dueño, de un admin o sin visitante).</summary>
    Ignorado,
    Invalido,
    NoEncontrado,
    SinPermiso
}

/// <summary>Quién hace la petición (sale del token; null = visitante sin sesión).</summary>
public record SolicitanteVisibilidad(int? IdUsuario, bool EsAdminGeneral, bool EsAdminMunicipio);

public interface IVisibilidadService
{
    /// <summary>Registra una vista o un clic. Nunca falla por duplicados: los ignora.</summary>
    Task<ResultadoVisibilidad> RegistrarAsync(RegistrarEventoDto dto, SolicitanteVisibilidad quien, CancellationToken ct);

    /// <summary>Estadísticas de los establecimientos del usuario (panel del comercio).</summary>
    Task<ResumenComercioDto> ResumenComercioAsync(int idUsuario, int dias, CancellationToken ct);

    /// <summary>Municipio asignado al administrador municipal (null si no tiene).</summary>
    Task<int?> MunicipioAsignadoAsync(int idUsuario, CancellationToken ct);

    /// <summary>Reporte de un municipio. El admin municipal solo puede pedir el suyo.</summary>
    Task<(ResultadoVisibilidad Resultado, ReporteMunicipalDto? Reporte)> ReporteMunicipalAsync(
        int idMunicipio, SolicitanteVisibilidad quien, int dias, CancellationToken ct);

    /// <summary>Comparativo de todos los municipios activos (AdminGeneral).</summary>
    Task<ResumenGeneralDto> ResumenGeneralAsync(int dias, CancellationToken ct);

    /// <summary>El reporte municipal en CSV (separador ';' para que Excel en español lo abra bien).</summary>
    string GenerarCsv(ReporteMunicipalDto reporte);
}
