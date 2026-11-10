namespace TurismoConecta.web.Client.Constants;

/// <summary>Mismos valores que Constants/AlertasMunicipales.cs del API (CHECK de la tabla AlertaMunicipal).</summary>
public static class TiposAlerta
{
    public const string Evento = "Evento";
    public const string CambioHorario = "CambioHorario";
    public const string CierreTemporal = "CierreTemporal";
    public const string Movilidad = "Movilidad";

    public static string Nombre(string tipo) => tipo switch
    {
        Evento => "Evento",
        CambioHorario => "Cambio de horario",
        CierreTemporal => "Cierre temporal",
        Movilidad => "Movilidad",
        _ => tipo
    };

    /// <summary>Ícono de Phosphor para cada tipo.</summary>
    public static string Icono(string tipo) => tipo switch
    {
        Evento => "ph-fill ph-confetti",
        CambioHorario => "ph-fill ph-clock-countdown",
        CierreTemporal => "ph-fill ph-prohibit",
        Movilidad => "ph-fill ph-traffic-cone",
        _ => "ph-fill ph-warning"
    };
}

public static class SeveridadesAlerta
{
    public const string Informativa = "Informativa";
    public const string Precaucion = "Precaucion";
    public const string Importante = "Importante";

    public static string Nombre(string severidad) => severidad switch
    {
        Precaucion => "Precaución",
        _ => severidad
    };

    /// <summary>Clase CSS: sev-info | sev-precaucion | sev-importante.</summary>
    public static string Clase(string severidad) => severidad switch
    {
        Importante => "sev-importante",
        Precaucion => "sev-precaucion",
        _ => "sev-info"
    };

    public static int Peso(string severidad) => severidad switch
    {
        Importante => 0,
        Precaucion => 1,
        _ => 2
    };
}
