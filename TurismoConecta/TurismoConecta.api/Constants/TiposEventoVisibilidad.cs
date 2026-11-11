namespace TurismoConecta.api.Constants;

/// <summary>
/// Valores de EventoVisibilidad.tipo (deben coincidir con CK_EventoVisibilidad_tipo
/// del script 2026-11-10_003). Copia en el Client: Constants/TiposEventoVisibilidad.cs.
/// </summary>
public static class TiposEventoVisibilidad
{
    public const string VistaMunicipio = "VistaMunicipio";
    public const string VistaNegocio = "VistaNegocio";
    public const string ClicWhatsApp = "ClicWhatsApp";
    public const string ClicTelefono = "ClicTelefono";
    public const string ClicComoLlegar = "ClicComoLlegar";

    /// <summary>Los que se refieren a un establecimiento (llevan idNegocio).</summary>
    public static readonly IReadOnlySet<string> DeNegocio =
        new HashSet<string> { VistaNegocio, ClicWhatsApp, ClicTelefono, ClicComoLlegar };

    /// <summary>Los clics que significan "este turista quiso contactar o llegar".</summary>
    public static readonly IReadOnlySet<string> Contactos =
        new HashSet<string> { ClicWhatsApp, ClicTelefono, ClicComoLlegar };

    public static bool EsValido(string? tipo) =>
        tipo == VistaMunicipio || (tipo is not null && DeNegocio.Contains(tipo));
}
