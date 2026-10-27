using TurismoConecta.api.DTOs.Recomendaciones;

namespace TurismoConecta.api.Services.Recomendaciones
{
    /// <summary>
    /// Guarda en memoria el último modelo de ML.NET entrenado: las predicciones ya calculadas
    /// (usuario × municipio) y su estado. Es singleton: lo comparten todas las peticiones.
    /// Cada entrenamiento publica una "foto" nueva completa, así nadie lee un modelo a medio armar.
    /// </summary>
    public class ModeloColaborativo
    {
        private volatile Instantanea _actual = Instantanea.Vacia;

        // Evita que el entrenamiento automático y el manual corran al mismo tiempo
        public SemaphoreSlim CandadoEntrenamiento { get; } = new(1, 1);

        public EstadoModeloDto Estado => _actual.Estado;

        /// <summary>Calificación estimada (1 a 5) que el usuario le daría al municipio. null si el modelo no lo sabe.</summary>
        public double? Predecir(int idUsuario, int idMunicipio) =>
            _actual.Predicciones.TryGetValue((idUsuario, idMunicipio), out var calificacion) ? calificacion : null;

        public void Publicar(EstadoModeloDto estado, Dictionary<(int IdUsuario, int IdMunicipio), double> predicciones) =>
            _actual = new Instantanea(estado, predicciones);

        private sealed record Instantanea(
            EstadoModeloDto Estado,
            IReadOnlyDictionary<(int IdUsuario, int IdMunicipio), double> Predicciones)
        {
            public static readonly Instantanea Vacia = new(
                new EstadoModeloDto { Mensaje = "El modelo todavía no se ha entrenado." },
                new Dictionary<(int, int), double>());
        }
    }
}
