using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TurismoConecta.api.Constants;

namespace TurismoConecta.api.Services.Exportacion;

/// <summary>
/// PDF del itinerario con QuestPDF: portada con el resumen del viaje y, por cada día,
/// sus paradas con foto, tramo desde la anterior, clima, qué ver y avisos municipales.
/// Pensado para imprimirse en A4 o leerse en el celular sin conexión.
/// </summary>
public class ItinerarioPdf : IDocument
{
    // Paleta de la web (FormularioResena / visibilidad)
    private const string Bosque = "#213B31";
    private const string Tierra = "#9E5B3D";
    private const string Azul = "#1A6FA8";
    private const string Crema = "#FAF7F2";
    private const string Borde = "#EAE6DF";
    private const string TextoSecundario = "#5A6961";

    private const int MaxCaracteresDescripcion = 420;
    private static readonly CultureInfo Es = CultureInfo.GetCultureInfo("es-CO");

    private readonly ItinerarioExportable _it;
    private readonly DateTime _generado;

    public ItinerarioPdf(ItinerarioExportable itinerario)
    {
        _it = itinerario;
        _generado = DateTime.Now;
    }

    public byte[] Generar() => this.GeneratePdf();

    public DocumentMetadata GetMetadata() => new()
    {
        Title = _it.Nombre,
        Author = "TurismoConecta",
        Subject = "Itinerario de viaje por Boyacá",
        CreationDate = _generado
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(36);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(t => t.FontSize(10).FontColor("#1F2937").LineHeight(1.35f));

            page.Header().Element(Encabezado);
            page.Content().PaddingTop(14).Element(Contenido);
            page.Footer().Element(Pie);
        });
    }

    // ───────────────────────── Encabezado y pie ─────────────────────────

    private void Encabezado(IContainer c)
    {
        c.BorderBottom(1).BorderColor(Borde).PaddingBottom(6).Row(fila =>
        {
            fila.RelativeItem().Text(t =>
            {
                t.Span("TurismoConecta").Bold().FontColor(Bosque);
                t.Span("  ·  Itinerario de viaje").FontColor(TextoSecundario);
            });
            fila.AutoItem().AlignRight().Text(_it.Nombre).FontSize(9).FontColor(TextoSecundario);
        });
    }

    private void Pie(IContainer c)
    {
        c.BorderTop(1).BorderColor(Borde).PaddingTop(6).Row(fila =>
        {
            fila.RelativeItem().Text(t =>
            {
                t.DefaultTextStyle(s => s.FontSize(8).FontColor(TextoSecundario));
                t.Span($"Generado el {_generado.ToString("d 'de' MMMM 'de' yyyy", Es)} · ");
                t.Span("Distancias y tiempos estimados por carretera.");
            });
            fila.AutoItem().AlignRight().Text(t =>
            {
                t.DefaultTextStyle(s => s.FontSize(8).FontColor(TextoSecundario));
                t.Span("Página ");
                t.CurrentPageNumber();
                t.Span(" de ");
                t.TotalPages();
            });
        });
    }

    // ───────────────────────── Contenido ─────────────────────────

    private void Contenido(IContainer c)
    {
        c.Column(col =>
        {
            col.Spacing(14);
            col.Item().Element(Portada);

            if (!string.IsNullOrWhiteSpace(_it.Observaciones))
            {
                col.Item().Background(Crema).Border(1).BorderColor(Borde).CornerRadius(6).Padding(10).Column(obs =>
                {
                    obs.Item().Text("NOTAS DEL VIAJE").FontSize(8).Bold().LetterSpacing(0.08f).FontColor(Tierra);
                    obs.Item().PaddingTop(3).Text(_it.Observaciones!.Trim());
                });
            }

            foreach (var dia in _it.Dias)
                col.Item().Element(x => Dia(x, dia));

            col.Item().PaddingTop(6).Element(Enlace);
        });
    }

    private void Portada(IContainer c)
    {
        c.Background(Bosque).CornerRadius(8).Padding(18).Column(col =>
        {
            col.Item().Text("RUTA BOYACENSE").FontSize(8).Bold().LetterSpacing(0.12f).FontColor("#C9B79C");
            col.Item().PaddingTop(2).Text(_it.Nombre).FontSize(22).Bold().FontColor(Colors.White);

            var fechas = RangoFechas();
            if (fechas is not null)
                col.Item().PaddingTop(2).Text(fechas).FontSize(11).FontColor("#E5EDE8");

            col.Item().PaddingTop(12).Row(fila =>
            {
                fila.Spacing(8);
                Metrica(fila.RelativeItem(), _it.Dias.Count.ToString(), _it.Dias.Count == 1 ? "día" : "días");
                Metrica(fila.RelativeItem(), _it.CantidadParadas.ToString(), _it.CantidadParadas == 1 ? "parada" : "paradas");
                Metrica(fila.RelativeItem(), _it.KmTotales > 0 ? $"{_it.KmTotales:0}" : "—", "km por carretera");
                Metrica(fila.RelativeItem(), _it.MinutosTotales > 0 ? ItinerarioIcs.Duracion(_it.MinutosTotales) : "—", "al volante");
            });
        });
    }

    private static void Metrica(IContainer c, string valor, string etiqueta)
    {
        c.Background("#2C4A3E").CornerRadius(6).PaddingVertical(8).PaddingHorizontal(10).Column(col =>
        {
            col.Item().Text(valor).FontSize(15).Bold().FontColor(Colors.White);
            col.Item().Text(etiqueta).FontSize(8).FontColor("#C9D6CF");
        });
    }

    private void Dia(IContainer c, DiaExportable dia)
    {
        c.Column(col =>
        {
            col.Spacing(8);

            // El título va pegado a la primera parada: nunca queda solo al final de una página
            col.Item().PreventPageBreak().Column(inicio =>
            {
                inicio.Spacing(8);
                inicio.Item().Element(x => TituloDia(x, dia));
                if (dia.Paradas.Count > 0) inicio.Item().Element(x => Parada(x, dia.Paradas[0]));
            });

            foreach (var parada in dia.Paradas.Skip(1))
                col.Item().PreventPageBreak().Element(x => Parada(x, parada));
        });
    }

    private void TituloDia(IContainer c, DiaExportable dia)
    {
        c.BorderBottom(2).BorderColor(Tierra).PaddingBottom(4).Row(fila =>
        {
            fila.AutoItem().Text($"Día {dia.Numero}").FontSize(15).Bold().FontColor(Tierra);
            if (dia.Fecha is not null)
                fila.RelativeItem().PaddingLeft(8).AlignBottom()
                    .Text(Capitalizar(dia.Fecha.Value.ToString("dddd d 'de' MMMM", Es)))
                    .FontSize(11).FontColor(TextoSecundario);
        });
    }

    private void Parada(IContainer c, ParadaExportable p)
    {
        c.Border(1).BorderColor(Borde).CornerRadius(6).Padding(10).Row(fila =>
        {
            var imagen = CargarImagen(p.RutaImagen);
            if (imagen is not null)
            {
                fila.ConstantItem(118).Height(88).CornerRadius(4).Image(imagen).FitArea();
                fila.ConstantItem(12);
            }

            fila.RelativeItem().Column(col =>
            {
                col.Spacing(3);

                col.Item().Text(t =>
                {
                    t.Span(p.Municipio).FontSize(13).Bold().FontColor(Bosque);
                    if (!string.IsNullOrWhiteSpace(p.Departamento))
                        t.Span($"  ·  {p.Departamento}").FontSize(9).FontColor(TextoSecundario);
                });

                if (p.KmDesdeAnterior is > 0)
                    col.Item().Text($"{p.KmDesdeAnterior:0} km · {ItinerarioIcs.Duracion(p.MinutosDesdeAnterior ?? 0)} desde la parada anterior")
                        .FontSize(9).Bold().FontColor(Azul);

                if (!string.IsNullOrWhiteSpace(p.Clima))
                    col.Item().Text(t =>
                    {
                        t.Span("Clima: ").Bold().FontSize(9);
                        t.Span(p.Clima!).FontSize(9).FontColor(TextoSecundario);
                    });

                if (!string.IsNullOrWhiteSpace(p.Descripcion))
                    col.Item().PaddingTop(2).Text(Recortar(p.Descripcion!, MaxCaracteresDescripcion));

                if (p.SitiosDestacados.Count > 0)
                    col.Item().PaddingTop(2).Text(t =>
                    {
                        t.Span("Qué ver: ").Bold().FontColor(Tierra);
                        t.Span(string.Join(" · ", p.SitiosDestacados));
                    });

                foreach (var aviso in p.Avisos)
                    col.Item().PaddingTop(4).Element(x => Aviso(x, aviso));
            });
        });
    }

    private void Aviso(IContainer c, AvisoExportable a)
    {
        var (fondo, color, etiqueta) = a.Severidad switch
        {
            SeveridadesAlerta.Importante => ("#FDECEA", "#B3261E", "IMPORTANTE"),
            SeveridadesAlerta.Precaucion => ("#FFF4E0", "#8A5A00", "PRECAUCIÓN"),
            _ => ("#E8F1F8", Azul, "AVISO")
        };

        c.Background(fondo).CornerRadius(4).Padding(6).Column(col =>
        {
            col.Item().Text(t =>
            {
                t.Span($"{etiqueta}  ").FontSize(7).Bold().FontColor(color).LetterSpacing(0.06f);
                t.Span(a.Titulo).FontSize(9).Bold().FontColor(color);
                if (a.Negocio is not null) t.Span($" · {a.Negocio}").FontSize(9).FontColor(color);
            });
            col.Item().Text(Recortar(a.Descripcion, 260)).FontSize(8.5f);
            col.Item().Text($"{a.Desde.ToString("d MMM, h:mm tt", Es)} → {a.Hasta.ToString("d MMM, h:mm tt", Es)}")
                .FontSize(7.5f).FontColor(TextoSecundario);
        });
    }

    private void Enlace(IContainer c)
    {
        if (string.IsNullOrEmpty(_it.EnlaceWeb)) return;

        c.Background(Crema).CornerRadius(6).Padding(10).Column(col =>
        {
            col.Item().Text(_it.EsCompartido
                    ? "Mapa de la ruta, negocios y avisos actualizados (enlace público):"
                    : "Mapa de la ruta, negocios y avisos actualizados (requiere iniciar sesión):")
                .FontSize(9).FontColor(TextoSecundario);
            col.Item().Hyperlink(_it.EnlaceWeb).Text(_it.EnlaceWeb).FontSize(9).FontColor(Azul).Underline();
        });
    }

    // ───────────────────────── Apoyo ─────────────────────────

    private string? RangoFechas()
    {
        if (_it.FechaInicio is null) return null;
        var inicio = _it.FechaInicio.Value;
        var fin = _it.FechaFin;

        if (fin is null || fin == inicio) return Capitalizar(inicio.ToString("dddd d 'de' MMMM 'de' yyyy", Es));
        return inicio.Year == fin.Value.Year
            ? $"{inicio.ToString("d 'de' MMMM", Es)} al {fin.Value.ToString("d 'de' MMMM 'de' yyyy", Es)}"
            : $"{inicio.ToString("d 'de' MMMM 'de' yyyy", Es)} al {fin.Value.ToString("d 'de' MMMM 'de' yyyy", Es)}";
    }

    // Una foto dañada no debe tumbar todo el PDF: se omite
    private static Image? CargarImagen(string? ruta)
    {
        if (ruta is null) return null;
        try { return Image.FromFile(ruta); }
        catch { return null; }
    }

    private static string Recortar(string texto, int maximo)
    {
        texto = texto.Trim();
        if (texto.Length <= maximo) return texto;
        var corte = texto.LastIndexOf(' ', maximo);
        return texto[..(corte > maximo / 2 ? corte : maximo)].TrimEnd(',', '.', ';') + "…";
    }

    private static string Capitalizar(string texto) =>
        texto.Length == 0 ? texto : char.ToUpper(texto[0], Es) + texto[1..];
}
