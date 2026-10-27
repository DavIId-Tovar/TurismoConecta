namespace TurismoConecta.api.DTOs.Recomendaciones
{
    /// <summary>Estado del modelo colaborativo de ML.NET (lo ve el admin general en su panel).
    ///  que se alimentar de datos relaes y interaccion de los usuarios que prueben el sistema-- </summary>
    public class EstadoModeloDto
    {
        public bool Activo { get; set; }

        // Datos con los que se entrenó y lo mínimo que necesita para activarse
        public int Interacciones { get; set; }
        public int Usuarios { get; set; }
        public int Municipios { get; set; }
        public int MinimoInteracciones { get; set; }
        public int MinimoUsuarios { get; set; }

        // Error promedio de la predicción, en estrellas. null si aún no hay datos para medirlo
        public double? ErrorPromedio { get; set; }

        public DateTime? UltimoEntrenamiento { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
