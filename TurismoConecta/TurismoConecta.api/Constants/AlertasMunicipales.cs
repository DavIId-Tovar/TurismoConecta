namespace TurismoConecta.api.Constants;

/// <summary>Valores permitidos en AlertaMunicipal.tipo (mismo CHECK que en SQL).</summary>
public static class TiposAlerta
{
    public const string Evento = "Evento";
    public const string CambioHorario = "CambioHorario";
    public const string CierreTemporal = "CierreTemporal";
    public const string Movilidad = "Movilidad";

    public static readonly string[] Todos = { Evento, CambioHorario, CierreTemporal, Movilidad };

    /// <summary>Un comercio solo avisa sobre su propio negocio: cierres y cambios de horario.</summary>
    public static readonly string[] DeComercio = { CambioHorario, CierreTemporal };

    public static string Nombre(string tipo) => tipo switch
    {
        Evento => "Evento",
        CambioHorario => "Cambio de horario",
        CierreTemporal => "Cierre temporal",
        Movilidad => "Movilidad",
        _ => tipo
    };
}

/// <summary>Valores permitidos en AlertaMunicipal.severidad (mismo CHECK que en SQL).</summary>
public static class SeveridadesAlerta
{
    public const string Informativa = "Informativa";
    public const string Precaucion = "Precaucion";
    public const string Importante = "Importante";

    public static readonly string[] Todas = { Informativa, Precaucion, Importante };
}
