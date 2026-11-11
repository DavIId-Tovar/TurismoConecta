using System.Globalization;
using System.Text;

namespace TurismoConecta.api.Services.Exportacion;

/// <summary>
/// Calendario iCalendar (RFC 5545) del itinerario: un evento de día completo por cada día del viaje.
/// Lo abren Google Calendar, Outlook y el Calendario de Apple.
///
/// El UID de cada evento es fijo (itinerario + día): si el viajero vuelve a importar el archivo
/// después de cambiar el itinerario, su calendario actualiza los eventos en vez de duplicarlos.
/// </summary>
public static class ItinerarioIcs
{
    private const int MaxOctetosPorLinea = 75;
    private static readonly CultureInfo Es = CultureInfo.GetCultureInfo("es-CO");

    public static string Generar(ItinerarioExportable itinerario, DateTime ahoraUtc)
    {
        var lineas = new List<string>
        {
            "BEGIN:VCALENDAR",
            "VERSION:2.0",
            "PRODID:-//TurismoConecta//Itinerarios//ES",
            "CALSCALE:GREGORIAN",
            "METHOD:PUBLISH",
            "X-WR-CALNAME:" + Escapar(itinerario.Nombre),
            "X-WR-TIMEZONE:America/Bogota"
        };

        var sello = ahoraUtc.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);

        foreach (var dia in itinerario.Dias.Where(d => d.Fecha is not null))
        {
            var fecha = dia.Fecha!.Value;
            var nombres = string.Join(" → ", dia.Paradas.Select(p => p.Municipio));
            var conCoordenadas = dia.Paradas.FirstOrDefault(p => p.Latitud is not null && p.Longitud is not null);

            lineas.Add("BEGIN:VEVENT");
            lineas.Add($"UID:turismoconecta-itinerario-{itinerario.IdItinerario}-dia-{dia.Numero}@turismoconecta");
            lineas.Add("DTSTAMP:" + sello);
            lineas.Add("DTSTART;VALUE=DATE:" + fecha.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
            lineas.Add("DTEND;VALUE=DATE:" + fecha.AddDays(1).ToString("yyyyMMdd", CultureInfo.InvariantCulture));
            lineas.Add("SUMMARY:" + Escapar($"Día {dia.Numero}: {nombres} · {itinerario.Nombre}"));
            lineas.Add("LOCATION:" + Escapar(string.Join(" / ", dia.Paradas.Select(p => p.Ubicacion))));
            if (conCoordenadas is not null)
                lineas.Add(string.Create(CultureInfo.InvariantCulture,
                    $"GEO:{conCoordenadas.Latitud:0.000000};{conCoordenadas.Longitud:0.000000}"));
            lineas.Add("DESCRIPTION:" + Escapar(Descripcion(itinerario, dia)));
            if (!string.IsNullOrEmpty(itinerario.EnlaceWeb))
                lineas.Add("URL:" + itinerario.EnlaceWeb);
            lineas.Add("TRANSP:TRANSPARENT");   // día de viaje: no marca al usuario como "ocupado"
            lineas.Add("END:VEVENT");
        }

        lineas.Add("END:VCALENDAR");

        var sb = new StringBuilder();
        foreach (var linea in lineas) Plegar(sb, linea);
        return sb.ToString();
    }

    private static string Descripcion(ItinerarioExportable itinerario, DiaExportable dia)
    {
        var sb = new StringBuilder();
        sb.Append($"Día {dia.Numero} de {itinerario.Dias.Count} · {dia.Fecha!.Value.ToString("dddd d 'de' MMMM", Es)}\n");

        foreach (var p in dia.Paradas)
        {
            sb.Append($"\n📍 {p.Municipio}");
            if (p.KmDesdeAnterior is > 0)
                sb.Append($" ({p.KmDesdeAnterior:0} km, {Duracion(p.MinutosDesdeAnterior ?? 0)} desde la parada anterior)");
            sb.Append('\n');

            if (!string.IsNullOrWhiteSpace(p.Clima)) sb.Append($"Clima: {p.Clima}\n");
            if (p.SitiosDestacados.Count > 0) sb.Append($"Qué ver: {string.Join(", ", p.SitiosDestacados)}\n");
            foreach (var a in p.Avisos)
                sb.Append($"⚠ {a.Titulo}{(a.Negocio is null ? "" : $" ({a.Negocio})")}: {a.Descripcion}\n");
        }

        if (!string.IsNullOrEmpty(itinerario.EnlaceWeb))
            sb.Append($"\nVer el itinerario y el mapa: {itinerario.EnlaceWeb}");

        return sb.ToString().TrimEnd();
    }

    public static string Duracion(int minutos) =>
        minutos < 60 ? $"{minutos} min" : $"{minutos / 60} h {minutos % 60:00} min";

    // RFC 5545 §3.3.11: en TEXT se escapan \ ; , y los saltos de línea
    private static string Escapar(string texto) => texto
        .Replace("\\", "\\\\")
        .Replace(";", "\\;")
        .Replace(",", "\\,")
        .Replace("\r\n", "\n")
        .Replace("\r", "\n")
        .Replace("\n", "\\n");

    /// <summary>
    /// RFC 5545 §3.1: líneas de máximo 75 octetos; las siguientes empiezan con un espacio.
    /// Corta por bytes UTF-8 sin partir una tilde ni un emoji por la mitad.
    /// </summary>
    private static void Plegar(StringBuilder sb, string linea)
    {
        var octetos = 0;
        var limite = MaxOctetosPorLinea;

        var elementos = StringInfo.GetTextElementEnumerator(linea);
        while (elementos.MoveNext())
        {
            var elemento = (string)elementos.Current;
            var tamaño = Encoding.UTF8.GetByteCount(elemento);

            if (octetos + tamaño > limite)
            {
                sb.Append("\r\n ");
                octetos = 0;
                limite = MaxOctetosPorLinea - 1;   // el espacio inicial cuenta
            }

            sb.Append(elemento);
            octetos += tamaño;
        }

        sb.Append("\r\n");
    }
}
