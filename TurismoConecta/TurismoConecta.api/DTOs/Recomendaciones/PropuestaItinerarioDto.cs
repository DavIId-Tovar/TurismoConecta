namespace TurismoConecta.api.DTOs.Recomendaciones
{
    /// <summary>
    /// Ruta que arma "Sorpréndeme": no se guarda sola, llena el formulario de Crear itinerario
    /// para que el usuario la revise y la guarde.
    /// </summary>
    public class PropuestaItinerarioDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int DiasPedidos { get; set; }
        public List<ParadaPropuestaDto> Paradas { get; set; } = new();

        // Suma de los tramos estimados (sin mapa: línea recta × factor de carretera)
        public int KmTotal { get; set; }
        public int MinutosTotal { get; set; }

        // Cuántos puntos de partida distintos puede dar "Otra sorpresa"
        public int OpcionesDeInicio { get; set; }

        // Ej. "Por ahora hay 5 destinos con ubicación…". null si todo salió completo
        public string? Aviso { get; set; }
    }

    public class ParadaPropuestaDto
    {
        public int IdMunicipio { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public List<string> Etiquetas { get; set; } = new();
        public double? PromedioCalificacion { get; set; }
        public int TotalResenas { get; set; }

        public int DiaNumero { get; set; }
        public string Razon { get; set; } = string.Empty;
        public string TipoRazon { get; set; } = TiposRazon.Nuevo;

        // Tramo desde la parada anterior (null en la primera)
        public int? KmDesdeAnterior { get; set; }
        public int? MinutosDesdeAnterior { get; set; }
    }
}
