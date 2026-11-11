using System;

namespace TurismoConecta.api.Models;

/// <summary>
/// Una interacción de un visitante con un municipio o un establecimiento
/// (vista de ficha, clic en WhatsApp, teléfono o "Ver en Google Maps").
/// Tabla creada por Database/Scripts/2026-11-10_003_evento_visibilidad.sql.
/// El mapeo está en Data/AppDbContext.Personalizado.cs (partial, no en el archivo generado).
/// </summary>
public partial class EventoVisibilidad
{
    public long IdEventoVisibilidad { get; set; }

    public string Tipo { get; set; } = null!;

    public int IdMunicipio { get; set; }

    public int? IdNegocio { get; set; }

    public int? IdUsuario { get; set; }

    public Guid? IdVisitante { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Municipio IdMunicipioNavigation { get; set; } = null!;

    public virtual Negocio? IdNegocioNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
