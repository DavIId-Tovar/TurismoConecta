// DTOs/Itinerarios/ParadaResponseDto.cs
namespace TurismoConecta.api.DTOs.Itinerarios
{
    public class ParadaResponseDto
    {
        public int IdItinerarioDetalle { get; set; }
        public int IdMunicipio { get; set; }
        public string NombreMunicipio { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public int DiaNumero { get; set; }
        public int Orden { get; set; }
        public DateOnly? FechaVisita { get; set; }
        public decimal? DistanciaKm { get; set; }
        public int? TiempoEstimadoMin { get; set; }

        // Para el mapa de viaje: el pin de la parada (null si el municipio no tiene ubicación)
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
    }
}