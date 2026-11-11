namespace TurismoConecta.web.Client.Models
{
    // Mismo contrato que DTOs/Visibilidad/VisibilidadDtos.cs del API

    public class RegistrarEventoRequest
    {
        public string Tipo { get; set; } = "";
        public int? IdMunicipio { get; set; }
        public int? IdNegocio { get; set; }
        public Guid? IdVisitante { get; set; }
    }

    public class MetricaDto
    {
        public int Actual { get; set; }
        public int Anterior { get; set; }
    }

    public class PuntoSerieDto
    {
        public DateOnly Fecha { get; set; }
        public int Vistas { get; set; }
        public int Contactos { get; set; }
    }

    public class PeriodoDto
    {
        public int Dias { get; set; }
        public DateOnly Desde { get; set; }
        public DateOnly Hasta { get; set; }
    }

    public class VisibilidadNegocioDto
    {
        public int IdNegocio { get; set; }
        public string Nombre { get; set; } = "";
        public string NombreMunicipio { get; set; } = "";
        public string Estado { get; set; } = "";
        public MetricaDto Vistas { get; set; } = new();
        public MetricaDto WhatsApp { get; set; } = new();
        public MetricaDto Telefono { get; set; } = new();
        public MetricaDto ComoLlegar { get; set; } = new();
        public MetricaDto Contactos { get; set; } = new();
        public MetricaDto FavoritosNuevos { get; set; } = new();
        public int FavoritosTotal { get; set; }
        public MetricaDto ResenasNuevas { get; set; } = new();
        public int ResenasTotal { get; set; }
        public double? Promedio { get; set; }
        public double? PromedioPeriodo { get; set; }
        public int ViajerosProximos { get; set; }
        public List<PuntoSerieDto> Serie { get; set; } = new();
    }

    public class ResumenComercioDto
    {
        public PeriodoDto Periodo { get; set; } = new();
        public List<VisibilidadNegocioDto> Negocios { get; set; } = new();
    }

    public class FilaNegocioReporteDto
    {
        public int IdNegocio { get; set; }
        public string Nombre { get; set; } = "";
        public string Categoria { get; set; } = "";
        public int Vistas { get; set; }
        public int Contactos { get; set; }
        public int FavoritosTotal { get; set; }
        public int ResenasTotal { get; set; }
        public double? Promedio { get; set; }
    }

    public class ReporteMunicipalDto
    {
        public int IdMunicipio { get; set; }
        public string NombreMunicipio { get; set; } = "";
        public PeriodoDto Periodo { get; set; } = new();
        public MetricaDto VistasFicha { get; set; } = new();
        public MetricaDto VistasNegocios { get; set; } = new();
        public MetricaDto Contactos { get; set; } = new();
        public MetricaDto FavoritosNuevos { get; set; } = new();
        public MetricaDto ResenasNuevas { get; set; } = new();
        public MetricaDto ItinerariosNuevos { get; set; } = new();
        public int ViajerosProximos { get; set; }
        public double? PromedioMunicipio { get; set; }
        public int ResenasMunicipioTotal { get; set; }
        public int ResenasRetenidas { get; set; }
        public int NegociosAprobados { get; set; }
        public int NegociosPendientes { get; set; }
        public int NegociosRechazados { get; set; }
        public int NegociosSinVistas { get; set; }
        public int AlertasVigentes { get; set; }
        public List<PuntoSerieDto> Serie { get; set; } = new();
        public List<FilaNegocioReporteDto> Negocios { get; set; } = new();
    }

    public class FilaMunicipioGeneralDto
    {
        public int IdMunicipio { get; set; }
        public string Nombre { get; set; } = "";
        public int VistasFicha { get; set; }
        public int VistasNegocios { get; set; }
        public int Contactos { get; set; }
        public int ItinerariosNuevos { get; set; }
        public int NegociosAprobados { get; set; }
        public int NegociosPendientes { get; set; }
        public double? Promedio { get; set; }
        public int ResenasTotal { get; set; }
    }

    public class ResumenGeneralDto
    {
        public PeriodoDto Periodo { get; set; } = new();
        public MetricaDto Vistas { get; set; } = new();
        public MetricaDto Contactos { get; set; } = new();
        public MetricaDto ItinerariosNuevos { get; set; } = new();
        public MetricaDto ResenasNuevas { get; set; } = new();
        public List<PuntoSerieDto> Serie { get; set; } = new();
        public List<FilaMunicipioGeneralDto> Municipios { get; set; } = new();
    }
}
