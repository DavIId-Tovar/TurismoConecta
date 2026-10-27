using System.Net.Http.Json;
using TurismoConecta.web.Client.Models;

namespace TurismoConecta.web.Client.Services
{
    public class RecomendacionApiService
    {
        private readonly HttpClient _http;
        public RecomendacionApiService(HttpClient http) => _http = http;

        // null = no se pudo cargar (la pantalla muestra "Reintentar"); lista vacía = no hay resultados
        public Task<List<RecomendacionDto>?> ParaMiAsync(int cantidad = 6, CancellationToken ct = default) =>
            ObtenerListaAsync($"api/recomendaciones/para-mi?cantidad={cantidad}", ct);

        public Task<List<RecomendacionDto>?> ParaTiAsync(IEnumerable<int> idsEtiquetas, int cantidad = 6, CancellationToken ct = default)
        {
            var ids = string.Join(",", idsEtiquetas);
            var filtro = ids.Length > 0 ? $"&etiquetas={ids}" : "";
            return ObtenerListaAsync($"api/recomendaciones/para-ti?cantidad={cantidad}{filtro}", ct);
        }

        public Task<List<RecomendacionDto>?> SimilaresAsync(int idMunicipio, int cantidad = 4, CancellationToken ct = default) =>
            ObtenerListaAsync($"api/recomendaciones/similares/{idMunicipio}?cantidad={cantidad}", ct);

        public async Task<List<int>> ObtenerPreferenciasAsync(CancellationToken ct = default)
        {
            try
            {
                var respuesta = await _http.GetFromJsonAsync<PreferenciasDto>("api/recomendaciones/preferencias", ct);
                return respuesta?.IdsEtiquetas ?? new();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return new();
            }
        }

        public async Task<bool> GuardarPreferenciasAsync(IEnumerable<int> idsEtiquetas, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.PutAsJsonAsync(
                    "api/recomendaciones/preferencias",
                    new PreferenciasDto { IdsEtiquetas = idsEtiquetas.ToList() },
                    ct);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return false;
            }
        }

        // ---- Panel del admin: estado y entrenamiento del modelo de ML.NET ----
        public async Task<EstadoModeloDto?> ObtenerEstadoModeloAsync(CancellationToken ct = default)
        {
            try { return await _http.GetFromJsonAsync<EstadoModeloDto>("api/recomendaciones/modelo", ct); }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { return null; }
        }

        public async Task<EstadoModeloDto?> EntrenarModeloAsync(CancellationToken ct = default)
        {
            try
            {
                var response = await _http.PostAsync("api/recomendaciones/modelo/entrenar", null, ct);
                return response.IsSuccessStatusCode
                    ? await response.Content.ReadFromJsonAsync<EstadoModeloDto>(cancellationToken: ct)
                    : null;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { return null; }
        }


        private async Task<List<RecomendacionDto>?> ObtenerListaAsync(string url, CancellationToken ct)
        {
            try { return await _http.GetFromJsonAsync<List<RecomendacionDto>>(url, ct); }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { return null; }
        }
    }
}
