using System.ComponentModel.DataAnnotations;

namespace TurismoConecta.api.DTOs.Visibilidad;

// ═════════════ ENTRADA ═════════════

/// <summary>POST api/visibilidad/evento. Lo manda el navegador al ver una ficha o tocar un botón de contacto.</summary>
public class RegistrarEventoDto
{
    [Required, MaxLength(20)]
    public string Tipo { get; set; } = "";

    /// <summary>Obligatorio en VistaMunicipio. En los eventos de negocio se deduce del negocio.</summary>
    public int? IdMunicipio { get; set; }

    public int? IdNegocio { get; set; }

    /// <summary>Identificador aleatorio del navegador (localStorage). Solo sirve para no contar doble.</summary>
    public Guid? IdVisitante { get; set; }
}

// ═════════════ PIEZAS COMUNES ═════════════

/// <summary>Un número del periodo elegido y el del periodo anterior de la misma duración.</summary>
public class MetricaDto
{
    public int Actual { get; set; }
    public int Anterior { get; set; }
}

/// <summary>Un día de la gráfica.</summary>
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

// ═════════════ PANEL DEL COMERCIO ═════════════

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
    /// <summary>WhatsApp + teléfono + cómo llegar.</summary>
    public MetricaDto Contactos { get; set; } = new();

    public MetricaDto FavoritosNuevos { get; set; } = new();
    public int FavoritosTotal { get; set; }

    public MetricaDto ResenasNuevas { get; set; } = new();
    public int ResenasTotal { get; set; }
    public double? Promedio { get; set; }
    public double? PromedioPeriodo { get; set; }

    /// <summary>Itinerarios con fechas en los próximos 30 días que pasan por el municipio del negocio.</summary>
    public int ViajerosProximos { get; set; }

    public List<PuntoSerieDto> Serie { get; set; } = new();
}

public class ResumenComercioDto
{
    public PeriodoDto Periodo { get; set; } = new();
    public List<VisibilidadNegocioDto> Negocios { get; set; } = new();
}

// ═════════════ REPORTE MUNICIPAL ═════════════

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
    /// <summary>Del municipio y de sus negocios.</summary>
    public MetricaDto FavoritosNuevos { get; set; } = new();
    /// <summary>Visibles, del municipio y de sus negocios.</summary>
    public MetricaDto ResenasNuevas { get; set; } = new();
    /// <summary>Itinerarios creados en el periodo que incluyen el municipio.</summary>
    public MetricaDto ItinerariosNuevos { get; set; } = new();

    public int ViajerosProximos { get; set; }
    public double? PromedioMunicipio { get; set; }
    public int ResenasMunicipioTotal { get; set; }
    public int ResenasRetenidas { get; set; }

    public int NegociosAprobados { get; set; }
    public int NegociosPendientes { get; set; }
    public int NegociosRechazados { get; set; }
    /// <summary>Aprobados que nadie vio en el periodo.</summary>
    public int NegociosSinVistas { get; set; }

    public int AlertasVigentes { get; set; }

    public List<PuntoSerieDto> Serie { get; set; } = new();
    /// <summary>Negocios aprobados, de más a menos vistos.</summary>
    public List<FilaNegocioReporteDto> Negocios { get; set; } = new();
}

// ═════════════ PANEL GENERAL (AdminGeneral) ═════════════

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
