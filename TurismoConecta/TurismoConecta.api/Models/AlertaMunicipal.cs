using System;

namespace TurismoConecta.api.Models;

/// <summary>
/// Aviso de un municipio (evento, cierre, cambio de horario, movilidad).
/// Tabla creada por Database/Scripts/2026-11-10_001_alerta_municipal.sql.
/// El mapeo está en Data/AppDbContext.Personalizado.cs (partial, no en el archivo generado).
/// </summary>
public partial class AlertaMunicipal
{
    public int IdAlertaMunicipal { get; set; }

    public int IdMunicipio { get; set; }

    public int? IdNegocio { get; set; }

    public string Tipo { get; set; } = null!;

    public string Severidad { get; set; } = null!;

    public string Titulo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public bool Activa { get; set; }

    public int IdUsuarioCreador { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual Municipio IdMunicipioNavigation { get; set; } = null!;

    public virtual Negocio? IdNegocioNavigation { get; set; }

    public virtual Usuario IdUsuarioCreadorNavigation { get; set; } = null!;
}
