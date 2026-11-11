namespace TurismoConecta.api.Services.Exportacion;

/// <summary>
/// Todo lo que necesitan el PDF y el calendario (.ics), ya reunido y ordenado.
/// No es un DTO de la API: nunca se devuelve como JSON.
/// </summary>
public class ItinerarioExportable
{
    public int IdItinerario { get; init; }
    public string Nombre { get; init; } = "";
    public DateOnly? FechaInicio { get; init; }
    public DateOnly? FechaFin { get; init; }
    public string? Observaciones { get; init; }

    /// <summary>Enlace público si el itinerario está compartido; si no, el enlace privado del dueño.</summary>
    public string EnlaceWeb { get; init; } = "";
    public bool EsCompartido { get; init; }

    public List<DiaExportable> Dias { get; init; } = new();

    public decimal KmTotales => Dias.SelectMany(d => d.Paradas).Sum(p => p.KmDesdeAnterior ?? 0);
    public int MinutosTotales => Dias.SelectMany(d => d.Paradas).Sum(p => p.MinutosDesdeAnterior ?? 0);
    public int CantidadParadas => Dias.Sum(d => d.Paradas.Count);

    /// <summary>El calendario necesita una fecha por día: sin fechas no se puede exportar.</summary>
    public bool TieneFechas => Dias.Count > 0 && Dias.All(d => d.Fecha is not null);
}

public class DiaExportable
{
    public int Numero { get; init; }
    public DateOnly? Fecha { get; init; }
    public List<ParadaExportable> Paradas { get; init; } = new();
}

public class ParadaExportable
{
    public int IdMunicipio { get; init; }
    public string Municipio { get; init; } = "";
    public string? Departamento { get; init; }
    public string? Descripcion { get; init; }
    public string? Clima { get; init; }
    public decimal? Latitud { get; init; }
    public decimal? Longitud { get; init; }
    public decimal? KmDesdeAnterior { get; init; }
    public int? MinutosDesdeAnterior { get; init; }

    /// <summary>Ruta física de la foto del municipio (null si no está en el disco del API).</summary>
    public string? RutaImagen { get; init; }

    public List<string> SitiosDestacados { get; init; } = new();
    public List<AvisoExportable> Avisos { get; init; } = new();

    public string Ubicacion => string.IsNullOrWhiteSpace(Departamento)
        ? $"{Municipio}, Colombia"
        : $"{Municipio}, {Departamento}, Colombia";
}

public class AvisoExportable
{
    public string Titulo { get; init; } = "";
    public string Descripcion { get; init; } = "";
    public string Severidad { get; init; } = "";
    public DateTime Desde { get; init; }
    public DateTime Hasta { get; init; }
    public string? Negocio { get; init; }
}
