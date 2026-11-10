using System.Globalization;
using System.Net.Http.Json;
using TurismoConecta.web.Client.Models;

namespace TurismoConecta.web.Client.Services
{
    /// <summary>Cliente de api/alertas (alertas municipales).</summary>
    public class AlertaApiService
    {
        private readonly HttpClient _http;
        public AlertaApiService(HttpClient http) => _http = http;

        // ─────────────── PÚBLICO ───────────────

        // Vigentes y próximas de un municipio (franja de la ficha)
        public Task<List<AlertaDto>> ListarDeMunicipioAsync(int idMunicipio, CancellationToken ct = default) =>
            ObtenerListaAsync($"api/alertas/municipio/{idMunicipio}", ct);

        // Vigentes de varios municipios; desde/hasta = fechas del viaje (opcionales)
        public Task<List<AlertaDto>> ListarVigentesAsync(IEnumerable<int> idsMunicipios, DateOnly? desde = null, DateOnly? hasta = null, CancellationToken ct = default)
        {
            var ids = idsMunicipios.Distinct().ToList();
            if (ids.Count == 0) return Task.FromResult(new List<AlertaDto>());

            var url = $"api/alertas/vigentes?municipios={string.Join(",", ids)}";
            if (desde.HasValue) url += $"&desde={desde.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
            if (hasta.HasValue) url += $"&hasta={hasta.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
            return ObtenerListaAsync(url, ct);
        }

        // ─────────────── GESTIÓN ───────────────

        public Task<List<AlertaDto>> ListarGestionAsync(CancellationToken ct = default) =>
            ObtenerListaAsync("api/alertas/gestion", ct);

        public async Task<OpcionesAlertaDto?> ObtenerOpcionesAsync(CancellationToken ct = default)
        {
            try
            {
                return await _http.GetFromJsonAsync<OpcionesAlertaDto>("api/alertas/opciones", ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return null;
            }
        }

        public Task<(bool Exito, AlertaDto? Alerta, string? Error)> CrearAsync(GuardarAlertaRequest request, CancellationToken ct = default) =>
            GuardarAsync(() => _http.PostAsJsonAsync("api/alertas", request, ct), "No se pudo publicar la alerta.", ct);

        public Task<(bool Exito, AlertaDto? Alerta, string? Error)> EditarAsync(int idAlerta, GuardarAlertaRequest request, CancellationToken ct = default) =>
            GuardarAsync(() => _http.PutAsJsonAsync($"api/alertas/{idAlerta}", request, ct), "No se pudo guardar la alerta.", ct);

        public async Task<(bool Exito, string? Error)> FinalizarAsync(int idAlerta, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.PostAsync($"api/alertas/{idAlerta}/finalizar", null, ct);
                return response.IsSuccessStatusCode
                    ? (true, null)
                    : (false, await ErrorApi.LeerMensajeAsync(response, "No se pudo finalizar la alerta."));
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return (false, "No se pudo conectar con el servidor.");
            }
        }

        // ─────────────── AYUDANTES ───────────────

        private async Task<List<AlertaDto>> ObtenerListaAsync(string url, CancellationToken ct)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<AlertaDto>>(url, ct) ?? new();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return new();
            }
        }

        private static async Task<(bool Exito, AlertaDto? Alerta, string? Error)> GuardarAsync(
            Func<Task<HttpResponseMessage>> peticion, string porDefecto, CancellationToken ct)
        {
            try
            {
                var response = await peticion();
                if (!response.IsSuccessStatusCode)
                    return (false, null, await ErrorApi.LeerMensajeAsync(response, porDefecto));

                return (true, await response.Content.ReadFromJsonAsync<AlertaDto>(cancellationToken: ct), null);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return (false, null, "No se pudo conectar con el servidor.");
            }
        }
    }
}
