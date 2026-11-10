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

        // "Queda cerca" de un municipio, sin repetir los que ya están en la ruta
        public Task<List<RecomendacionDto>?> CercaAsync(int idMunicipio, IEnumerable<int> excluir, int cantidad = 4, CancellationToken ct = default)
        {
            var ids = string.Join(",", excluir);
            var filtro = ids.Length > 0 ? $"&excluir={ids}" : "";
            return ObtenerListaAsync($"api/recomendaciones/cerca/{idMunicipio}?cantidad={cantidad}{filtro}", ct);
        }

        // "Sorpréndeme": ruta de una parada por día. Sin sesión se le pasan los gustos del navegador. null = falló
        public async Task<PropuestaItinerarioDto?> SorprendemeAsync(int dias, int variante, IEnumerable<int>? idsEtiquetas = null, CancellationToken ct = default)
        {
            var ids = string.Join(",", idsEtiquetas ?? Enumerable.Empty<int>());
            var filtro = ids.Length > 0 ? $"&etiquetas={ids}" : "";

            try
            {
                return await _http.GetFromJsonAsync<PropuestaItinerarioDto>(
                    $"api/recomendaciones/sorprendeme?dias={dias}&variante={variante}{filtro}", ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { return null; }
        }

        // Km y tiempo de la ruta. optimizar = la devuelve ordenada por cercanía (la 1.ª parada no se mueve). null = falló
        // desde: ubicación del usuario ("Estoy aquí"); con optimizar, la ruta empieza por la parada más cercana a ella
        public async Task<RutaEstimadaDto?> EstimarRutaAsync(IEnumerable<int> idsEnOrden, bool optimizar, CancellationToken ct = default, (double Lat, double Lng)? desde = null)
        {
            try
            {
                var url = $"api/recomendaciones/ruta?ids={string.Join(",", idsEnOrden)}&optimizar={(optimizar ? "true" : "false")}";
                if (optimizar && desde is (double lat, double lng))
                    url += string.Create(System.Globalization.CultureInfo.InvariantCulture, $"&desdeLat={lat:0.######}&desdeLng={lng:0.######}");

                return await _http.GetFromJsonAsync<RutaEstimadaDto>(url, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { return null; }
        }

        // ---- "No me interesa" ----
        public async Task<bool> DescartarAsync(int idMunicipio, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.PostAsync($"api/recomendaciones/descartar/{idMunicipio}", null, ct);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { return false; }
        }

        public async Task<bool> RestaurarAsync(int idMunicipio, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/recomendaciones/descartar/{idMunicipio}", ct);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { return false; }
        }

        // null = no se pudo cargar
        public async Task<List<MunicipioDescartadoDto>?> ListarDescartadosAsync(CancellationToken ct = default)
        {
            try { return await _http.GetFromJsonAsync<List<MunicipioDescartadoDto>>("api/recomendaciones/descartados", ct); }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { return null; }
        }

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

        public async Task<MetricasRecomendacionesDto?> ObtenerMetricasAsync(CancellationToken ct = default)
        {
            try { return await _http.GetFromJsonAsync<MetricasRecomendacionesDto>("api/recomendaciones/metricas", ct); }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { return null; }
        }

        private async Task<List<RecomendacionDto>?> ObtenerListaAsync(string url, CancellationToken ct)
        {
            try { return await _http.GetFromJsonAsync<List<RecomendacionDto>>(url, ct); }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { return null; }
        }
    }
}
