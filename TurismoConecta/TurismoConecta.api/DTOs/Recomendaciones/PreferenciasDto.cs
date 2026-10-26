namespace TurismoConecta.api.DTOs.Recomendaciones
{
    ///Los gustos (ids de etiquetas) que el usuario eligió.</summary>
    public class PreferenciasDto
    {
        public List<int> IdsEtiquetas { get; set; } = new();
    }
}
