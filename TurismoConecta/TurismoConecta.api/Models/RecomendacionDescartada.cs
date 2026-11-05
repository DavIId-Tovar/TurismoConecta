using System;

namespace TurismoConecta.api.Models;

/// <summary>Un municipio que el usuario marcó "No me interesa": no se le vuelve a recomendar.</summary>
public partial class RecomendacionDescartada
{
    public int IdRecomendacionDescartada { get; set; }

    public int IdUsuario { get; set; }

    public int IdMunicipio { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual Municipio IdMunicipioNavigation { get; set; } = null!;
}
