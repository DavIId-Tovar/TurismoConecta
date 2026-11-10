using TurismoConecta.api.DTOs.Alertas;

namespace TurismoConecta.api.Services.Interfaces
{
    public interface IAlertaService
    {
        // Público: alertas vigentes (activas y sin vencer) de uno o varios municipios.
        // desde/hasta (opcionales) dejan solo las que se cruzan con ese rango de fechas.
        Task<List<AlertaDto>> ListarVigentesAsync(IReadOnlyCollection<int> idsMunicipios, DateTime? desde, DateTime? hasta, CancellationToken ct = default);

        // Paneles: lo que el usuario puede gestionar según su rol (vigentes primero, luego el historial)
        Task<List<AlertaDto>> ListarGestionAsync(int idUsuario, string rol, CancellationToken ct = default);

        // Qué municipios, negocios y tipos puede elegir en el formulario
        Task<OpcionesAlertaDto> ObtenerOpcionesAsync(int idUsuario, string rol, CancellationToken ct = default);

        Task<(ResultadoAlerta resultado, string? error, AlertaDto? alerta)> CrearAsync(int idUsuario, string rol, GuardarAlertaDto dto, CancellationToken ct = default);

        Task<(ResultadoAlerta resultado, string? error, AlertaDto? alerta)> EditarAsync(int idAlerta, int idUsuario, string rol, GuardarAlertaDto dto, CancellationToken ct = default);

        // La quita antes de su fecha de fin (por ejemplo, la vía se habilitó antes)
        Task<(ResultadoAlerta resultado, string? error)> FinalizarAsync(int idAlerta, int idUsuario, string rol, CancellationToken ct = default);
    }
}
