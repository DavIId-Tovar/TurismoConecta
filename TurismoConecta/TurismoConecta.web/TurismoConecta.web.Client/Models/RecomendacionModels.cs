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

        // Solo en "Queda cerca": estimación por carretera desde el municipio de origen
        public int? DistanciaKm { get; set; }
        public int? TiempoEstimadoMin { get; set; }
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
        public const string Cerca = "cerca";
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

    // Uso del motor (panel del admin general)
    public class MetricasRecomendacionesDto
    {
        public int UsuariosConGustos { get; set; }
        public int Descartes { get; set; }
        public int UsuariosQueDescartaron { get; set; }
        public int MunicipiosActivos { get; set; }
        public int MunicipiosSinCoordenadas { get; set; }
        public double RadioCercaKm { get; set; }
        public List<string> MasDescartados { get; set; } = new();
    }

    // "Sorpréndeme": ruta propuesta que llena el formulario de Crear itinerario
    public class PropuestaItinerarioDto
    {
        public string Nombre { get; set; } = "";
        public int DiasPedidos { get; set; }
        public List<ParadaPropuestaDto> Paradas { get; set; } = new();
        public int KmTotal { get; set; }
        public int MinutosTotal { get; set; }
        public int OpcionesDeInicio { get; set; }
        public string? Aviso { get; set; }
    }

    // Km y tiempo de la ruta tal como está en pantalla
    public class RutaEstimadaDto
    {
        public List<TramoRutaDto> Tramos { get; set; } = new();
        public int KmTotal { get; set; }
        public int MinutosTotal { get; set; }
        public List<string> SinUbicacion { get; set; } = new();

        // true = carretera real (OpenRouteService); false = estimación en línea recta
        public bool EsCarreteraReal { get; set; }

        // Dibujo de la ruta por carretera como [lat, lng] (vacío si es estimación)
        public List<double[]> Geometria { get; set; } = new();
    }

    public class TramoRutaDto
    {
        public int IdMunicipio { get; set; }
        public string? DesdeNombre { get; set; }
        public int? KmDesdeAnterior { get; set; }
        public int? MinutosDesdeAnterior { get; set; }
    }

    public class ParadaPropuestaDto
    {
        public int IdMunicipio { get; set; }
        public string Nombre { get; set; } = "";
        public string? ImagenUrl { get; set; }
        public List<string> Etiquetas { get; set; } = new();
        public double? PromedioCalificacion { get; set; }
        public int TotalResenas { get; set; }
        public int DiaNumero { get; set; }
        public string Razon { get; set; } = "";
        public string TipoRazon { get; set; } = TiposRazon.Nuevo;
        public int? KmDesdeAnterior { get; set; }
        public int? MinutosDesdeAnterior { get; set; }
    }

    // Un municipio que el usuario ocultó con "No me interesa" (se lista en su Perfil)
    public class MunicipioDescartadoDto
    {
        public int IdMunicipio { get; set; }
        public string Nombre { get; set; } = "";
        public string? ImagenUrl { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

}
