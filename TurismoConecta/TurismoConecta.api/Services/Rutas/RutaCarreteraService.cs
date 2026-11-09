using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace TurismoConecta.api.Services.Rutas
{
    /// <summary>Ruta real por carretera: el dibujo (lat, lng) y cada tramo en metros y segundos.</summary>
    public sealed record TrazadoCarretera(List<double[]> Geometria, List<(double Metros, double Segundos)> Tramos);

    /// <summary>
    /// Consulta OpenRouteService (rutas por carretera). La clave vive en user-secrets/variables de entorno
    /// (OpenRouteService:ApiKey) y nunca llega al navegador.
    /// Devuelve null si no hay clave, si el servicio falla o tarda: quien llama usa la estimación en línea recta.
    /// Las respuestas se guardan en memoria para no gastar el límite gratuito (2.000 rutas/día, 40/min).
    /// </summary>
    public class RutaCarreteraService
    {
        private const int MaximoPuntos = 50;            // límite de OpenRouteService por consulta
        private const int RadioBusquedaMetros = 5000;   // distancia máxima del punto a la vía más cercana

        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RutaCarreteraService> _logger;
        private readonly string? _apiKey;
        private readonly string _perfil;
        private readonly TimeSpan _duracionCache;

        public RutaCarreteraService(HttpClient http, IMemoryCache cache, IConfiguration configuracion, ILogger<RutaCarreteraService> logger)
        {
            _http = http;
            _cache = cache;
            _logger = logger;
            _apiKey = configuracion["OpenRouteService:ApiKey"];
            _perfil = configuracion["OpenRouteService:Perfil"] ?? "driving-car";
            _duracionCache = TimeSpan.FromHours(configuracion.GetValue("OpenRouteService:HorasCache", 24.0));
        }

        public bool Configurado => !string.IsNullOrWhiteSpace(_apiKey);

        /// <param name="puntos">Las paradas en orden (lat, lng). Hacen falta al menos 2.</param>
        public async Task<TrazadoCarretera?> CalcularAsync(IReadOnlyList<(double Lat, double Lng)> puntos, CancellationToken ct = default)
        {
            if (!Configurado || puntos.Count < 2 || puntos.Count > MaximoPuntos) return null;

            // Misma ruta = misma respuesta: la clave de caché son las coordenadas en orden
            var clave = "ors:" + _perfil + ":" + string.Join("|", puntos.Select(p =>
                string.Create(CultureInfo.InvariantCulture, $"{p.Lat:0.#####},{p.Lng:0.#####}")));

            if (_cache.TryGetValue(clave, out TrazadoCarretera? guardado)) return guardado;

            try
            {
                var cuerpo = new
                {
                    // OpenRouteService pide [longitud, latitud] (al revés que Leaflet)
                    coordinates = puntos.Select(p => new[] { p.Lng, p.Lat }).ToArray(),
                    // Busca la vía más cercana hasta 5 km del punto: el centro del pueblo puede quedar a más
                    // de 350 m de una vía (valor por defecto, la consulta fallaría). No se usa -1 ("sin límite")
                    // porque con varias paradas tarda ~30 s; con 5 km responde en ~1 s con los mismos tramos
                    radiuses = puntos.Select(_ => RadioBusquedaMetros).ToArray(),
                    // Con instructions = false OpenRouteService NO devuelve "segments" (los tramos entre paradas),
                    // solo el total; por eso se piden aunque las indicaciones paso a paso no se usen
                    instructions = true,
                    geometry_simplify = true
                };

                using var solicitud = new HttpRequestMessage(HttpMethod.Post, $"v2/directions/{_perfil}/geojson")
                {
                    Content = JsonContent.Create(cuerpo)
                };
                solicitud.Headers.TryAddWithoutValidation("Authorization", _apiKey);
                solicitud.Headers.TryAddWithoutValidation("Accept", "application/geo+json, application/json");

                using var respuesta = await _http.SendAsync(solicitud, ct);
                if (!respuesta.IsSuccessStatusCode)
                {
                    // 401/403 = clave mala, 429 = límite gastado, 404 = no hay ruta entre los puntos
                    _logger.LogWarning("[Rutas] OpenRouteService respondió {Codigo}: {Detalle}",
                        (int)respuesta.StatusCode, await respuesta.Content.ReadAsStringAsync(ct));
                    return null;
                }

                await using var flujo = await respuesta.Content.ReadAsStreamAsync(ct);
                using var json = await JsonDocument.ParseAsync(flujo, cancellationToken: ct);
                var trazado = Leer(json.RootElement, puntos.Count);

                if (trazado is not null)
                    _cache.Set(clave, trazado, _duracionCache);

                return trazado;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException or InvalidOperationException)
            {
                // Sin internet, tiempo agotado o respuesta inesperada: se usa la estimación
                if (!ct.IsCancellationRequested)
                    _logger.LogWarning(ex, "[Rutas] No se pudo consultar OpenRouteService; se usa la estimación");
                return null;
            }
        }

        /// <summary>GeoJSON: features[0].geometry.coordinates = [[lng, lat], …]; properties.segments = un tramo por cada par de paradas.</summary>
        private static TrazadoCarretera? Leer(JsonElement raiz, int cantidadPuntos)
        {
            var ruta = raiz.GetProperty("features")[0];

            var geometria = ruta.GetProperty("geometry").GetProperty("coordinates")
                .EnumerateArray()
                .Select(c => new[] { c[1].GetDouble(), c[0].GetDouble() })   // → [lat, lng] para Leaflet
                .ToList();

            var tramos = ruta.GetProperty("properties").GetProperty("segments")
                .EnumerateArray()
                .Select(s => (
                    Metros: s.TryGetProperty("distance", out var d) ? d.GetDouble() : 0,
                    Segundos: s.TryGetProperty("duration", out var t) ? t.GetDouble() : 0))
                .ToList();

            // Debe haber exactamente un tramo entre cada par de paradas; si no, mejor la estimación
            return tramos.Count == cantidadPuntos - 1 && geometria.Count > 1
                ? new TrazadoCarretera(geometria, tramos)
                : null;
        }
    }
}
