namespace TurismoConecta.api.DTOs.Recomendaciones
{
    /// <summary>Un municipio recomendado, con qué tanto encaja con el usuario y la razón que se le muestra.</summary>
    public class RecomendacionDto
    {
        public int IdMunicipio { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public string? Descripcion { get; set; }
        public List<string> Etiquetas { get; set; } = new();
        public double? PromedioCalificacion { get; set; }
        public int TotalResenas { get; set; }

        // 0 a 100: qué tanto encaja con el usuario
        public int Coincidencia { get; set; }

        // La frase que se ve en la tarjeta ("Porque te gusta Ecoturismo") y su tipo (para el ícono)
        public string Razon { get; set; } = string.Empty;
        public string TipoRazon { get; set; } = TiposRazon.Nuevo;
    }

    /// <summary>De dónde sale la razón principal de una recomendación.</summary>
    public static class TiposRazon
    {
        public const string Gusto = "gusto";
        public const string Favorito = "favorito";
        public const string Calificado = "calificado";
        public const string Itinerario = "itinerario";
        public const string Similar = "similar";
        public const string Comunidad = "comunidad";
        public const string Joya = "joya";
        public const string Popular = "popular";
        public const string Nuevo = "nuevo";
    }
}
