namespace TurismoConecta.api.DTOs.Negocios
{
    /// <summary>Un negocio aprobado con ubicación, para la capa "Dónde dormir / comer" del mapa de viaje.</summary>
    public class NegocioMapaDto
    {
        public int IdNegocio { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int IdMunicipio { get; set; }
        public string NombreMunicipio { get; set; } = string.Empty;
        public int IdCategoria { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string? IconoCategoria { get; set; }
        public string? ImagenUrl { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
    }
}
