namespace TurismoConecta.api.DTOs.Recomendaciones
{
    /// <summary>Cómo se está usando el motor de recomendaciones (panel del admin general).</summary>
    public class MetricasRecomendacionesDto
    {
        public int UsuariosConGustos { get; set; }
        public int Descartes { get; set; }
        public int UsuariosQueDescartaron { get; set; }

        // "Queda cerca" solo funciona con municipios que tienen coordenadas
        public int MunicipiosActivos { get; set; }
        public int MunicipiosSinCoordenadas { get; set; }
        public double RadioCercaKm { get; set; }

        // Los 3 municipios más ocultados, ej. "Ráquira (4)": si uno se repite mucho, algo en su ficha no convence
        public List<string> MasDescartados { get; set; } = new();
    }
}
