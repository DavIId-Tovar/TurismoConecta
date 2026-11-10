using TurismoConecta.api.DTOs.Recomendaciones;

namespace TurismoConecta.api.Services.Interfaces
{
    public interface IRecomendacionService
    {
        Task<List<RecomendacionDto>> ParaUsuarioAsync(int idUsuario, int cantidad, CancellationToken ct = default);
        Task<List<RecomendacionDto>> PorGustosAsync(List<int> idsEtiquetas, int cantidad, CancellationToken ct = default);
        Task<List<RecomendacionDto>> SimilaresAsync(int idMunicipio, int? idUsuario, int cantidad, CancellationToken ct = default);
        Task<List<RecomendacionDto>> CercaAsync(int idMunicipio, int? idUsuario, List<int> excluir, int cantidad, CancellationToken ct = default);
        Task<PropuestaItinerarioDto> SorprendemeAsync(int? idUsuario, List<int> idsEtiquetas, int dias, int variante, CancellationToken ct = default);
        // desde: ubicación del usuario ("Estoy aquí"). Con optimizar, la ruta empieza por la parada más cercana a ella.
        Task<RutaEstimadaDto> EstimarRutaAsync(List<int> idsEnOrden, bool optimizar, CancellationToken ct = default, (double Lat, double Lng)? desde = null);
        Task<List<int>> ObtenerPreferenciasAsync(int idUsuario, CancellationToken ct = default);
        Task<(bool exito, string? error)> GuardarPreferenciasAsync(int idUsuario, List<int> idsEtiquetas, CancellationToken ct = default);
        Task<(bool exito, string? error)> DescartarAsync(int idUsuario, int idMunicipio, CancellationToken ct = default);
        Task<int> RestaurarAsync(int idUsuario, int idMunicipio, CancellationToken ct = default);
        Task<List<MunicipioDescartadoDto>> ListarDescartadosAsync(int idUsuario, CancellationToken ct = default);
        Task<MetricasRecomendacionesDto> MetricasAsync(CancellationToken ct = default);
    }
}
