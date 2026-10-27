namespace TurismoConecta.web.Client.Models
{
    // Respuesta de api/recomendaciones (para-mi, para-ti y similares)
    public class RecomendacionDto
    {
        public int IdMunicipio { get; set; }
        public string Nombre { get; set; } = "";
        public string? ImagenUrl { get; set; }
        public string? Descripcion { get; set; }
        public List<string> Etiquetas { get; set; } = new();
        public double? PromedioCalificacion { get; set; }
        public int TotalResenas { get; set; }
        public int Coincidencia { get; set; }
        public string Razon { get; set; } = "";
        public string TipoRazon { get; set; } = TiposRazon.Nuevo;
    }

    // Mismos valores que TiposRazon del API: deciden el ícono de la razón en la tarjeta
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

    public class PreferenciasDto
    {
        public List<int> IdsEtiquetas { get; set; } = new();
    }


    // Estado del modelo de ML.NET (panel del admin general)
    public class EstadoModeloDto
    {
        public bool Activo { get; set; }
        public int Interacciones { get; set; }
        public int Usuarios { get; set; }
        public int Municipios { get; set; }
        public int MinimoInteracciones { get; set; }
        public int MinimoUsuarios { get; set; }
        public double? ErrorPromedio { get; set; }
        public DateTime? UltimoEntrenamiento { get; set; }
        public string Mensaje { get; set; } = "";
    }


}
