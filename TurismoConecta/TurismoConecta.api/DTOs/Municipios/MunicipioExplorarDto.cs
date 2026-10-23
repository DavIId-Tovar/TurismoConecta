namespace TurismoConecta.api.DTOs.Municipios
{
    /// <summary>Tarjeta del catálogo público (/municipios): listado + resumen y calificación de los viajeros.</summary>
    public class MunicipioExplorarDto
    {
        public int IdMunicipio { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public string? Descripcion { get; set; }
        public List<string> Etiquetas { get; set; } = new();

        // Solo cuentan las reseñas visibles (Moderada = true)
        public double? PromedioCalificacion { get; set; }
        public int TotalResenas { get; set; }
    }
}
