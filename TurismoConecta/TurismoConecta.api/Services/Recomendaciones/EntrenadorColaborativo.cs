using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using TurismoConecta.api.Data;
using TurismoConecta.api.DTOs.Recomendaciones;

namespace TurismoConecta.api.Services.Recomendaciones
{
    /// <summary>
    /// Entrena el filtrado colaborativo (Matrix Factorization de ML.NET): "a viajeros con gustos como los tuyos
    /// les encantó X". Aprende de reseñas (su calificación), favoritos (cuentan como 5★) e itinerarios (4★).
    /// Solo se activa cuando hay datos suficientes; mientras tanto el motor funciona sin él.
    /// </summary>
    public class EntrenadorColaborativo
    {
        private const float PuntajeFavorito = 5f;
        private const float PuntajeItinerario = 4f;
        private const int MinimoMunicipios = 3;
        private const int MinimoParaMedirError = 50;   // con menos datos, separar un grupo de prueba no dice nada

        private readonly AppDbContext _context;
        private readonly ModeloColaborativo _modelo;
        private readonly ILogger<EntrenadorColaborativo> _logger;
        private readonly int _minimoInteracciones;
        private readonly int _minimoUsuarios;

        public EntrenadorColaborativo(
            AppDbContext context,
            ModeloColaborativo modelo,
            IConfiguration configuracion,
            ILogger<EntrenadorColaborativo> logger)
        {
            _context = context;
            _modelo = modelo;
            _logger = logger;
            _minimoInteracciones = configuracion.GetValue("Recomendaciones:MinimoInteracciones", 30);
            _minimoUsuarios = configuracion.GetValue("Recomendaciones:MinimoUsuarios", 5);
        }

        // ML.NET trabaja con float: una fila = "este usuario le daría esta calificación a este municipio"
        public sealed class Interaccion
        {
            public float IdUsuario { get; set; }
            public float IdMunicipio { get; set; }
            public float Label { get; set; }
        }

        public sealed class Prediccion
        {
            public float IdUsuario { get; set; }
            public float IdMunicipio { get; set; }
            public float Score { get; set; }
        }

        public async Task<EstadoModeloDto> EntrenarAsync(CancellationToken ct = default)
        {
            await _modelo.CandadoEntrenamiento.WaitAsync(ct);
            try
            {
                return await EntrenarSinCandadoAsync(ct);
            }
            finally
            {
                _modelo.CandadoEntrenamiento.Release();
            }
        }

        private async Task<EstadoModeloDto> EntrenarSinCandadoAsync(CancellationToken ct)
        {
            var interacciones = await CargarInteraccionesAsync(ct);

            var usuariosActivos = interacciones.GroupBy(i => i.IdUsuario).Count(g => g.Count() >= 2);
            var municipios = interacciones.Select(i => i.IdMunicipio).Distinct().Count();

            var estado = new EstadoModeloDto
            {
                Interacciones = interacciones.Count,
                Usuarios = usuariosActivos,
                Municipios = municipios,
                MinimoInteracciones = _minimoInteracciones,
                MinimoUsuarios = _minimoUsuarios,
                UltimoEntrenamiento = DateTime.UtcNow
            };

            // Con pocos datos el modelo recomendaría casi al azar: mejor no usarlo todavía
            if (interacciones.Count < _minimoInteracciones || usuariosActivos < _minimoUsuarios || municipios < MinimoMunicipios)
            {
                estado.Mensaje = $"Reuniendo datos: {interacciones.Count} de {_minimoInteracciones} interacciones " +
                                 $"y {usuariosActivos} de {_minimoUsuarios} viajeros activos.";
                _modelo.Publicar(estado, new());
                return estado;
            }

            var ml = new MLContext(seed: 1);
            var datos = ml.Data.LoadFromEnumerable(interacciones);

            // 1. Qué tan bien predice: se entrena con el 80 % y se mide contra el 20 % que no vio
            if (interacciones.Count >= MinimoParaMedirError)
            {
                var particion = ml.Data.TrainTestSplit(datos, testFraction: 0.2, seed: 1);
                var modeloDePrueba = CrearTuberia(ml).Fit(particion.TrainSet);

                // Las filas de prueba con usuarios o municipios que el modelo no vio dan NaN: se descartan
                var calificadas = ml.Data.FilterRowsByMissingValues(modeloDePrueba.Transform(particion.TestSet), "Score");
                var metricas = ml.Regression.Evaluate(calificadas, labelColumnName: nameof(Interaccion.Label), scoreColumnName: "Score");

                if (double.IsFinite(metricas.RootMeanSquaredError))
                    estado.ErrorPromedio = Math.Round(metricas.RootMeanSquaredError, 2);
            }

            // 2. El modelo definitivo se entrena con todos los datos
            var modelo = CrearTuberia(ml).Fit(datos);

            // 3. Se precalcula la calificación estimada de cada viajero para cada municipio
            var idsUsuarios = interacciones.Select(i => i.IdUsuario).Distinct().ToList();
            var idsMunicipios = interacciones.Select(i => i.IdMunicipio).Distinct().ToList();
            var pares = idsUsuarios
                .SelectMany(u => idsMunicipios.Select(m => new Interaccion { IdUsuario = u, IdMunicipio = m }))
                .ToList();

            var predicciones = ml.Data
                .CreateEnumerable<Prediccion>(modelo.Transform(ml.Data.LoadFromEnumerable(pares)), reuseRowObject: false)
                .Where(p => float.IsFinite(p.Score))
                .ToDictionary(
                    p => ((int)p.IdUsuario, (int)p.IdMunicipio),
                    p => Math.Clamp((double)p.Score, 1, 5));

            estado.Activo = true;
            estado.Mensaje = $"Modelo activo: aprendió de {interacciones.Count} interacciones de {idsUsuarios.Count} viajeros.";
            _modelo.Publicar(estado, predicciones);

            _logger.LogInformation("[Recomendaciones] {Mensaje} Error promedio: {Error}", estado.Mensaje, estado.ErrorPromedio);
            return estado;
        }

        /// <summary>Usuario y municipio se convierten en "claves" y Matrix Factorization aprende sus gustos ocultos.</summary>
        private static IEstimator<ITransformer> CrearTuberia(MLContext ml) =>
            ml.Transforms.Conversion.MapValueToKey("UsuarioClave", nameof(Interaccion.IdUsuario))
                .Append(ml.Transforms.Conversion.MapValueToKey("MunicipioClave", nameof(Interaccion.IdMunicipio)))
                .Append(ml.Recommendation().Trainers.MatrixFactorization(new MatrixFactorizationTrainer.Options
                {
                    MatrixColumnIndexColumnName = "UsuarioClave",
                    MatrixRowIndexColumnName = "MunicipioClave",
                    LabelColumnName = nameof(Interaccion.Label),
                    NumberOfIterations = 40,
                    ApproximationRank = 8,     // cuántos "gustos ocultos" aprende (pocos: hay pocos municipios)
                    Lambda = 0.05,             // regularización: evita que memorice los datos
                    Quiet = true
                }));

        /// <summary>
        /// Una interacción por (usuario, municipio). Si dejó reseña, manda su calificación;
        /// si no, la señal implícita más fuerte (favorito 5★, itinerario 4★).
        /// </summary>
        private async Task<List<Interaccion>> CargarInteraccionesAsync(CancellationToken ct)
        {
            var municipiosActivos = _context.Municipios.Where(m => m.Activo).Select(m => m.IdMunicipio);

            var reseñas = await _context.Reseñas
                .AsNoTracking()
                .Where(r => r.Moderada && r.IdMunicipio != null && municipiosActivos.Contains(r.IdMunicipio.Value))
                .Select(r => new { r.IdUsuario, IdMunicipio = r.IdMunicipio!.Value, Puntaje = (float)r.Calificacion })
                .ToListAsync(ct);

            var favoritos = await _context.Favoritos
                .AsNoTracking()
                .Where(f => f.IdMunicipio != null && municipiosActivos.Contains(f.IdMunicipio.Value))
                .Select(f => new { f.IdUsuario, IdMunicipio = f.IdMunicipio!.Value, Puntaje = PuntajeFavorito })
                .ToListAsync(ct);

            var itinerarios = await _context.ItinerarioDetalles
                .AsNoTracking()
                .Where(d => municipiosActivos.Contains(d.IdMunicipio))
                .Select(d => new { d.IdItinerarioNavigation.IdUsuario, d.IdMunicipio, Puntaje = PuntajeItinerario })
                .Distinct()
                .ToListAsync(ct);

            var combinadas = reseñas
                .GroupBy(r => (r.IdUsuario, r.IdMunicipio))
                .ToDictionary(g => g.Key, g => g.Average(r => r.Puntaje));

            var conReseña = combinadas.Keys.ToHashSet();

            foreach (var implicita in favoritos.Concat(itinerarios))
            {
                var clave = (implicita.IdUsuario, implicita.IdMunicipio);
                if (conReseña.Contains(clave)) continue;

                combinadas[clave] = Math.Max(combinadas.GetValueOrDefault(clave), implicita.Puntaje);
            }

            return combinadas
                .Select(c => new Interaccion { IdUsuario = c.Key.IdUsuario, IdMunicipio = c.Key.IdMunicipio, Label = c.Value })
                .ToList();
        }
    }
}
