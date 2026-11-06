namespace TurismoConecta.web.Client.Models
{
    /// <summary>
    /// Una parada para el componente MapaViaje. Es común a Crear itinerario, el detalle y el compartido:
    /// cada página arma la lista con lo que tiene (los tramos pueden venir vacíos).
    /// </summary>
    public record ParadaMapa(
        int IdMunicipio,
        string Nombre,
        int Dia,
        double? Latitud,
        double? Longitud,
        string? ImagenUrl = null,
        int? KmDesdeAnterior = null,
        int? MinutosDesdeAnterior = null)
    {
        public bool TieneUbicacion => Latitud is double lat && Longitud is double lng && lat != 0 && lng != 0;

        /// <summary>Las paradas de un itinerario guardado (detalle y compartido), en el orden de la ruta.</summary>
        public static List<ParadaMapa> DeItinerario(TurismoConecta.web.Client.Services.ItinerarioDetalleDto itinerario) =>
            itinerario.Paradas
                .OrderBy(p => p.DiaNumero).ThenBy(p => p.Orden)
                .Select(p => new ParadaMapa(
                    p.IdMunicipio,
                    p.NombreMunicipio,
                    p.DiaNumero,
                    p.Latitud,
                    p.Longitud,
                    p.ImagenUrl,
                    p.DistanciaKm is decimal km ? (int)Math.Round(km) : null,
                    p.TiempoEstimadoMin))
                .ToList();
    }
}
