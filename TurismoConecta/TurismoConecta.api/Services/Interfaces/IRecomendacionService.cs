using TurismoConecta.api.DTOs.Recomendaciones;

namespace TurismoConecta.api.Services.Interfaces
{
    public interface IRecomendacionService
    {
        Task<List<RecomendacionDto>> ParaUsuarioAsync(int idUsuario, int cantidad, CancellationToken ct = default);
        Task<List<RecomendacionDto>> PorGustosAsync(List<int> idsEtiquetas, int cantidad, CancellationToken ct = default);
        Task<List<RecomendacionDto>> SimilaresAsync(int idMunicipio, int cantidad, CancellationToken ct = default);
        Task<List<int>> ObtenerPreferenciasAsync(int idUsuario, CancellationToken ct = default);
        Task<(bool exito, string? error)> GuardarPreferenciasAsync(int idUsuario, List<int> idsEtiquetas, CancellationToken ct = default);
    }
}
