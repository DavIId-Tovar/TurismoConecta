using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TurismoConecta.web.Client.Models;

namespace TurismoConecta.web.Client.Services
{
    /// <summary>Verificación de correo: confirmar el enlace, pedir otro y consultar el estado.</summary>
    public class VerificacionCorreoApiService
    {
        private readonly HttpClient _http;
        public VerificacionCorreoApiService(HttpClient http) => _http = http;

        // Lo usa la página /verificar-correo (funciona con o sin sesión)
        public async Task<(bool Exito, string Mensaje, bool Vencido, string? Email)> VerificarAsync(string token, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/verificar-correo", new { Token = token }, ct);
                var texto = await response.Content.ReadAsStringAsync(ct);
                var (mensaje, codigo, email) = LeerMensaje(texto);

                return response.IsSuccessStatusCode
                    ? (true, mensaje ?? "Tu correo quedó confirmado.", false, email)
                    : (false, mensaje ?? "El enlace no es válido.", codigo == "expirado", null);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return (false, "No se pudo conectar con el servidor.", false, null);
            }
        }

        // null = no se pudo consultar (sin sesión o sin conexión)
        public async Task<EstadoCorreoResponse?> ObtenerEstadoAsync(CancellationToken ct = default)
        {
            try
            {
                return await _http.GetFromJsonAsync<EstadoCorreoResponse>("api/auth/estado-correo", ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                return null;
            }
        }

        public async Task<(bool Exito, string Mensaje, int SegundosEspera)> ReenviarAsync(CancellationToken ct = default)
        {
            try
            {
                var response = await _http.PostAsync("api/auth/reenviar-verificacion", null, ct);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return (false, "Inicia sesión para pedir un enlace nuevo.", 0);

                var respuesta = await response.Content.ReadFromJsonAsync<ReenvioVerificacionResponse>(cancellationToken: ct);
                return (response.IsSuccessStatusCode,
                        respuesta?.Mensaje ?? "No se pudo enviar el correo.",
                        respuesta?.SegundosEspera ?? 0);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                return (false, "No se pudo conectar con el servidor.", 0);
            }
        }

        private static (string? Mensaje, string? Codigo, string? Email) LeerMensaje(string texto)
        {
            try
            {
                using var doc = JsonDocument.Parse(texto);
                var raiz = doc.RootElement;
                string? Leer(string nombre) =>
                    raiz.ValueKind == JsonValueKind.Object && raiz.TryGetProperty(nombre, out var v)
                    && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
                return (Leer("mensaje"), Leer("codigo"), Leer("email"));
            }
            catch (JsonException)
            {
                return (null, null, null);
            }
        }
    }
}
