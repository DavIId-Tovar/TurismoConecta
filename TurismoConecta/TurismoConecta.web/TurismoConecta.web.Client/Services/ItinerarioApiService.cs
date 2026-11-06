using System.Net.Http.Json;

namespace TurismoConecta.web.Client.Services
{
    public class ItinerarioApiService
    {
        private readonly HttpClient _http;

        // El HttpClient se inyecta por DI — ya tiene el token JWT adjunto
        // gracias al AuthorizationMessageHandler registrado en Program.cs
        public ItinerarioApiService(HttpClient http) => _http = http;

        // ── LISTAR mis itinerarios ──────────────────────────────────────────
        // Llama a: GET api/itinerarios
        // Devuelve la lista o null si hubo error de red
        public async Task<List<ItinerarioListadoDto>?> ListarAsync(CancellationToken ct = default)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<ItinerarioListadoDto>>("api/itinerarios", ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return null;
            }
        }

        // ── OBTENER detalle de UN itinerario (con sus paradas) ─────────────
        // Llama a: GET api/itinerarios/{id}
        public async Task<ItinerarioDetalleDto?> ObtenerDetalleAsync(int id, CancellationToken ct = default)
        {
            try
            {
                return await _http.GetFromJsonAsync<ItinerarioDetalleDto>($"api/itinerarios/{id}", ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return null;
            }
        }

        // ── OBTENER itinerario compartido (sin login) ──────────────────────
        // Llama a: GET api/itinerarios/compartido/{codigo}
        // Este endpoint es AllowAnonymous — cualquier persona con el link puede verlo
        public async Task<ItinerarioDetalleDto?> ObtenerCompartidoAsync(Guid codigo, CancellationToken ct = default)
        {
            try
            {
                return await _http.GetFromJsonAsync<ItinerarioDetalleDto>($"api/itinerarios/compartido/{codigo}", ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return null;
            }
        }

        // ── CREAR un itinerario nuevo ──────────────────────────────────────
        // Llama a: POST api/itinerarios
        public async Task<(bool Exito, string? Error, ItinerarioDetalleDto? Itinerario)> CrearAsync(
            ItinerarioCreateRequest dto, CancellationToken ct = default)
        {
            try
            {
                var respuesta = await _http.PostAsJsonAsync("api/itinerarios", dto, ct);
                if (respuesta.IsSuccessStatusCode)
                {
                    var data = await respuesta.Content.ReadFromJsonAsync<ItinerarioDetalleDto>(cancellationToken: ct);
                    return (true, null, data);
                }

                if (respuesta.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return (false, "Debes iniciar sesión para guardar tu itinerario.", null);
                }

                var err = await respuesta.Content.ReadAsStringAsync(ct);
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(err);
                    if (doc.RootElement.TryGetProperty("mensaje", out var msgProp))
                    {
                        return (false, msgProp.GetString(), null);
                    }
                }
                catch { }

                return (false, !string.IsNullOrWhiteSpace(err) ? err : "No se pudo crear el itinerario.", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        // ── ELIMINAR un itinerario ─────────────────────────────────────────
        // Llama a: DELETE api/itinerarios/{id}
        // Devuelve true si se eliminó, false si hubo error
        public async Task<bool> EliminarAsync(int id, CancellationToken ct = default)
        {
            var respuesta = await _http.DeleteAsync($"api/itinerarios/{id}", ct);
            return respuesta.IsSuccessStatusCode;
        }

        // ── TOGGLE COMPARTIR ───────────────────────────────────────────────
        // Llama a: POST api/itinerarios/{id}/compartir
        // Devuelve null si falló, o un objeto con el nuevo estado
        public async Task<CompartirResultado?> ToggleCompartirAsync(int id, CancellationToken ct = default)
        {
            var respuesta = await _http.PostAsync($"api/itinerarios/{id}/compartir", null, ct);
            if (!respuesta.IsSuccessStatusCode) return null;
            return await respuesta.Content.ReadFromJsonAsync<CompartirResultado>();
        }
    }

    // ── DTOs del cliente ───────────────────────────────────────────────────

    public class ItinerarioListadoDto
    {
        public int IdItinerario { get; set; }
        public string Nombre { get; set; } = "";
        public DateOnly? FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }
        public int CantidadParadas { get; set; }
    }

    public class ParadaDto
    {
        public int IdItinerarioDetalle { get; set; }
        public int IdMunicipio { get; set; }
        public string NombreMunicipio { get; set; } = "";
        public string? ImagenUrl { get; set; }
        public int DiaNumero { get; set; }
        public int Orden { get; set; }
        public DateOnly? FechaVisita { get; set; }
        public decimal? DistanciaKm { get; set; }
        public int? TiempoEstimadoMin { get; set; }

        // Pin de la parada en el mapa de viaje (null si el municipio no tiene ubicación)
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
    }

    public class ItinerarioDetalleDto
    {
        public int IdItinerario { get; set; }
        public string Nombre { get; set; } = "";
        public DateOnly? FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }
        public bool Compartido { get; set; }
        public Guid CodigoCompartir { get; set; }
        public string? Observaciones { get; set; }
        public List<ParadaDto> Paradas { get; set; } = new();
    }

    public class ItinerarioCreateRequest
    {
        public string Nombre { get; set; } = "";
        public DateOnly? FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }
        public string? Observaciones { get; set; }
        public List<ParadaCreateRequest> Paradas { get; set; } = new();
    }

    public class ParadaCreateRequest
    {
        public int IdMunicipio { get; set; }
        public int DiaNumero { get; set; }
        public int Orden { get; set; }
        public DateOnly? FechaVisita { get; set; }
    }

    public class CompartirResultado
    {
        public bool Compartido { get; set; }
        public Guid CodigoCompartir { get; set; }
    }
}