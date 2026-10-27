using System;

namespace TurismoConecta.api.Models;

///Una etiqueta que le interesa. Alimenta las recomendaciones.</summary>
public partial class PreferenciaUsuario
{
    public int IdPreferenciaUsuario { get; set; }

    public int IdUsuario { get; set; }

    public int IdEtiqueta { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual Etiqueta IdEtiquetaNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
