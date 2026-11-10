namespace TurismoConecta.web.Client.Models
{
    // Mismo contrato que DTOs/Alertas/AlertaDtos.cs del API
    public class AlertaDto
    {
        public int IdAlerta { get; set; }
        public int IdMunicipio { get; set; }
        public string NombreMunicipio { get; set; } = "";
        public int? IdNegocio { get; set; }
        public string? NombreNegocio { get; set; }
        public string Tipo { get; set; } = "";
        public string Severidad { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activa { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? NombreCreador { get; set; }
        public bool PuedeGestionar { get; set; }

        /// <summary>"EnCurso", "Proxima", "Vencida" o "Finalizada".</summary>
        public string Estado { get; set; } = "";

        public bool Vigente => Estado is "EnCurso" or "Proxima";

        /// <summary>¿La alerta toca ese día? (para marcar una parada del itinerario)</summary>
        public bool CubreDia(DateOnly dia) =>
            DateOnly.FromDateTime(FechaInicio) <= dia && dia <= DateOnly.FromDateTime(FechaFin);

        /// <summary>"12 nov, 08:00–18:00" o "12 nov al 15 nov".</summary>
        public string Rango
        {
            get
            {
                var es = new System.Globalization.CultureInfo("es-CO");
                return FechaInicio.Date == FechaFin.Date
                    ? $"{FechaInicio.ToString("d MMM", es)}, {FechaInicio:HH:mm}–{FechaFin:HH:mm}"
                    : $"{FechaInicio.ToString("d MMM", es)} al {FechaFin.ToString("d MMM", es)}";
            }
        }
    }

    public class GuardarAlertaRequest
    {
        public int? IdMunicipio { get; set; }
        public int? IdNegocio { get; set; }
        public string Tipo { get; set; } = "";
        public string Severidad { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }

    public class OpcionesAlertaDto
    {
        /// <summary>"General", "Municipio" o "Comercio".</summary>
        public string Alcance { get; set; } = "";
        public List<OpcionDto> Municipios { get; set; } = new();
        public List<OpcionNegocioDto> Negocios { get; set; } = new();
        public List<string> Tipos { get; set; } = new();
        public List<string> Severidades { get; set; } = new();
    }

    public class OpcionDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
    }

    public class OpcionNegocioDto : OpcionDto
    {
        public int IdMunicipio { get; set; }
    }
}
