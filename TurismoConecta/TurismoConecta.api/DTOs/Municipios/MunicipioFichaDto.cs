namespace TurismoConecta.api.DTOs.Municipios
{
    public class MunicipioFichaDto : MunicipioListadoDto
    {
        public string? Descripcion { get; set; }
        public string? Clima { get; set; }
        public string? Historia { get; set; }
        public List<FechaRelevanteDto> FechasRelevantes { get; set; } = new(); // antes era string?
        public new decimal? Latitud { get; set; }
        public new decimal? Longitud { get; set; }
    }
}
