namespace TurismoConecta.api.DTOs.Recomendaciones
{
    /// <summary>Un municipio que el usuario ocultó de sus recomendaciones (se lista en su Perfil para restaurarlo).</summary>
    public class MunicipioDescartadoDto
    {
        public int IdMunicipio { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
