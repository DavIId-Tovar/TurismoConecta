using System.ComponentModel.DataAnnotations;

namespace TurismoConecta.api.DTOs.Alertas
{
    /// <summary>Una alerta tal como la ven el turista (ficha, itinerario, mapa) y los paneles de gestión.</summary>
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

        /// <summary>Quién la publicó (solo se llena en los paneles de gestión).</summary>
        public string? NombreCreador { get; set; }

        /// <summary>Paneles: si quien consulta puede editarla o finalizarla.</summary>
        public bool PuedeGestionar { get; set; }

        /// <summary>Solo para calcular PuedeGestionar; no se envía al navegador.</summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public int IdUsuarioCreador { get; set; }

        /// <summary>"EnCurso", "Proxima", "Vencida" o "Finalizada", calculado al consultar.</summary>
        public string Estado { get; set; } = "";
    }

    /// <summary>Lo que envía el formulario para crear o editar.</summary>
    public class GuardarAlertaDto
    {
        /// <summary>Lo usa el admin principal. El municipal queda fijo en su municipio y el comercio en el de su negocio.</summary>
        public int? IdMunicipio { get; set; }

        /// <summary>Obligatorio para el comercio (solo sus negocios); opcional para los admins.</summary>
        public int? IdNegocio { get; set; }

        [Required(ErrorMessage = "Elige el tipo de alerta.")]
        public string Tipo { get; set; } = "";

        [Required(ErrorMessage = "Elige la severidad.")]
        public string Severidad { get; set; } = "";

        [Required(ErrorMessage = "Escribe un título.")]
        [StringLength(120, MinimumLength = 5, ErrorMessage = "El título debe tener entre 5 y 120 caracteres.")]
        public string Titulo { get; set; } = "";

        [Required(ErrorMessage = "Escribe la descripción.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 1000 caracteres.")]
        public string Descripcion { get; set; } = "";

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }

    /// <summary>Qué puede elegir el usuario en el formulario según su rol.</summary>
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

    /// <summary>Cómo terminó una operación; el controlador lo traduce a un código HTTP.</summary>
    public enum ResultadoAlerta
    {
        Ok,
        Invalida,       // 400
        NoEncontrada,   // 404
        SinPermiso      // 403
    }
}
