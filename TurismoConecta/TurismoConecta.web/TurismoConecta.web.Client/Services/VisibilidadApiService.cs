using System.Net.Http.Json;
using Microsoft.JSInterop;
using TurismoConecta.web.Client.Constants;
using TurismoConecta.web.Client.Models;

namespace TurismoConecta.web.Client.Services
{
    /// <summary>
    /// Cliente de api/visibilidad: registra vistas/clics y trae los paneles de estadísticas.
    /// </summary>
    public class VisibilidadApiService
    {
        /// <summary>Identificador aleatorio del navegador; no identifica a la persona.</summary>
        private const string ClaveVisitante = "tc_visitante";

        private readonly HttpClient _http;
        private readonly IJSRuntime _js;
        private Guid? _visitante;

        public VisibilidadApiService(HttpClient http, IJSRuntime js)
        {
            _http = http;
            _js = js;
        }

        // ─────────────── REGISTRO (dispara y olvida) ───────────────

        public Task VistaMunicipioAsync(int idMunicipio) =>
            RegistrarAsync(TiposEventoVisibilidad.VistaMunicipio, idMunicipio, null);

        public Task VistaNegocioAsync(int idNegocio) =>
            RegistrarAsync(TiposEventoVisibilidad.VistaNegocio, null, idNegocio);

        /// <summary>tipo = ClicWhatsApp | ClicTelefono | ClicComoLlegar.</summary>
        public Task ClicNegocioAsync(string tipo, int idNegocio) =>
            RegistrarAsync(tipo, null, idNegocio);

        /// <summary>
        /// Nunca lanza ni bloquea la página: si falla (sin red, sin localStorage) simplemente no se cuenta.
        /// </summary>
        private async Task RegistrarAsync(string tipo, int? idMunicipio, int? idNegocio)
        {
            try
            {
                var request = new RegistrarEventoRequest
                {
                    Tipo = tipo,
                    IdMunicipio = idMunicipio,
                    IdNegocio = idNegocio,
                    IdVisitante = await ObtenerVisitanteAsync()
                };
                using var _ = await _http.PostAsJsonAsync("api/visibilidad/evento", request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Visibilidad] No se registró {tipo}: {ex.Message}");
            }
        }

        private async Task<Guid?> ObtenerVisitanteAsync()
        {
            if (_visitante.HasValue) return _visitante;
            try
            {
                var guardado = await _js.InvokeAsync<string?>("localStorage.getItem", ClaveVisitante);
                if (Guid.TryParse(guardado, out var id))
                {
                    _visitante = id;
                }
                else
                {
                    _visitante = Guid.NewGuid();
                    await _js.InvokeVoidAsync("localStorage.setItem", ClaveVisitante, _visitante.ToString());
                }
            }
            catch (Exception)
            {
                // Navegación privada o almacenamiento bloqueado: id solo para esta pestaña
                _visitante ??= Guid.NewGuid();
            }
            return _visitante;
        }

        // ─────────────── PANELES ───────────────

        public Task<(ResumenComercioDto? Datos, string? Error)> MisNegociosAsync(int dias, CancellationToken ct = default) =>
            ObtenerAsync<ResumenComercioDto>($"api/visibilidad/mis-negocios?dias={dias}", ct);

        /// <summary>idMunicipio null = el municipio asignado al admin municipal.</summary>
        public Task<(ReporteMunicipalDto? Datos, string? Error)> ReporteMunicipalAsync(int? idMunicipio, int dias, CancellationToken ct = default) =>
            ObtenerAsync<ReporteMunicipalDto>(
                idMunicipio is int id ? $"api/visibilidad/municipio/{id}?dias={dias}" : $"api/visibilidad/mi-municipio?dias={dias}", ct);

        public Task<(ResumenGeneralDto? Datos, string? Error)> GeneralAsync(int dias, CancellationToken ct = default) =>
            ObtenerAsync<ResumenGeneralDto>($"api/visibilidad/general?dias={dias}", ct);

        /// <summary>Baja el CSV con el token y lo entrega al navegador como descarga.</summary>
        public async Task<string?> DescargarCsvAsync(int idMunicipio, int dias)
        {
            try
            {
                var respuesta = await _http.GetAsync($"api/visibilidad/municipio/{idMunicipio}/csv?dias={dias}");
                if (!respuesta.IsSuccessStatusCode)
                    return await ErrorApi.LeerMensajeAsync(respuesta, "No se pudo generar el reporte.");

                var bytes = await respuesta.Content.ReadAsByteArrayAsync();
                var nombre = respuesta.Content.Headers.ContentDisposition?.FileNameStar
                             ?? respuesta.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                             ?? $"reporte-municipal-{DateTime.Today:yyyy-MM-dd}.csv";

                using var flujo = new DotNetStreamReference(new MemoryStream(bytes));
                await _js.InvokeVoidAsync("archivos.descargar", nombre, "text/csv;charset=utf-8", flujo);
                return null;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return "No se pudo conectar con el servidor.";
            }
        }

        private async Task<(T? Datos, string? Error)> ObtenerAsync<T>(string url, CancellationToken ct) where T : class
        {
            try
            {
                var respuesta = await _http.GetAsync(url, ct);
                if (!respuesta.IsSuccessStatusCode)
                    return (null, await ErrorApi.LeerMensajeAsync(respuesta, "No se pudieron cargar las estadísticas."));
                return (await respuesta.Content.ReadFromJsonAsync<T>(cancellationToken: ct), null);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return (null, "No se pudo conectar con el servidor.");
            }
        }
    }
}
