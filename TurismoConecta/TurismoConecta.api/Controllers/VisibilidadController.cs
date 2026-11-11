using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurismoConecta.api.Constants;
using TurismoConecta.api.DTOs.Visibilidad;
using TurismoConecta.api.Services.Interfaces;

namespace TurismoConecta.api.Controllers
{
    /// <summary>
    /// Estadísticas de visibilidad (comercio), reportes municipales y comparativo general.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class VisibilidadController : ControllerBase
    {
        private readonly IVisibilidadService _servicio;
        public VisibilidadController(IVisibilidadService servicio) => _servicio = servicio;

        private int? UsuarioActualOpcional =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

        private int UsuarioActual => UsuarioActualOpcional!.Value;

        private SolicitanteVisibilidad Solicitante => new(
            UsuarioActualOpcional,
            User.IsInRole(Roles.AdminGeneral),
            User.IsInRole(Roles.AdminMunicipio));

        // POST api/visibilidad/evento   { tipo, idMunicipio?, idNegocio?, idVisitante? }
        // Público. Responde 204 aunque el evento se ignore (repetido, del dueño…):
        // al navegador no le importa y así no se revela nada.
        [HttpPost("evento")]
        [AllowAnonymous]
        public async Task<IActionResult> Registrar([FromBody] RegistrarEventoDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var resultado = await _servicio.RegistrarAsync(dto, Solicitante, ct);
            return resultado switch
            {
                ResultadoVisibilidad.Invalido => BadRequest(new { mensaje = "Evento no válido." }),
                ResultadoVisibilidad.NoEncontrado => NotFound(new { mensaje = "El lugar no existe o no es público." }),
                _ => NoContent()
            };
        }

        // GET api/visibilidad/mis-negocios?dias=30   → panel del comercio
        [HttpGet("mis-negocios")]
        [Authorize(Roles = Roles.GeneralOEstablecimiento)]
        public async Task<IActionResult> MisNegocios([FromQuery] int dias = 30, CancellationToken ct = default) =>
            Ok(await _servicio.ResumenComercioAsync(UsuarioActual, dias, ct));

        // GET api/visibilidad/mi-municipio?dias=30   → reporte del municipio asignado
        [HttpGet("mi-municipio")]
        [Authorize(Roles = Roles.AdminMunicipio)]
        public async Task<IActionResult> MiMunicipio([FromQuery] int dias = 30, CancellationToken ct = default)
        {
            var idMunicipio = await _servicio.MunicipioAsignadoAsync(UsuarioActual, ct);
            if (idMunicipio is null) return NotFound(new { mensaje = "Tu cuenta no tiene un municipio asignado." });
            return await Reporte(idMunicipio.Value, dias, ct);
        }

        // GET api/visibilidad/municipio/5?dias=30
        [HttpGet("municipio/{idMunicipio:int}")]
        [Authorize(Roles = Roles.GeneralOMunicipio)]
        public async Task<IActionResult> Reporte(int idMunicipio, [FromQuery] int dias = 30, CancellationToken ct = default)
        {
            var (resultado, reporte) = await _servicio.ReporteMunicipalAsync(idMunicipio, Solicitante, dias, ct);
            return resultado switch
            {
                ResultadoVisibilidad.Ok => Ok(reporte),
                ResultadoVisibilidad.SinPermiso => StatusCode(StatusCodes.Status403Forbidden, new { mensaje = "Solo puedes ver el reporte de tu municipio." }),
                _ => NotFound(new { mensaje = "El municipio no existe." })
            };
        }

        // GET api/visibilidad/municipio/5/csv?dias=30   → descarga para Excel
        [HttpGet("municipio/{idMunicipio:int}/csv")]
        [Authorize(Roles = Roles.GeneralOMunicipio)]
        public async Task<IActionResult> ReporteCsv(int idMunicipio, [FromQuery] int dias = 30, CancellationToken ct = default)
        {
            var (resultado, reporte) = await _servicio.ReporteMunicipalAsync(idMunicipio, Solicitante, dias, ct);
            if (resultado == ResultadoVisibilidad.SinPermiso)
                return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = "Solo puedes descargar el reporte de tu municipio." });
            if (reporte is null) return NotFound(new { mensaje = "El municipio no existe." });

            // Con BOM para que Excel reconozca las tildes
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(_servicio.GenerarCsv(reporte))).ToArray();
            var nombre = $"reporte-{Slug(reporte.NombreMunicipio)}-{reporte.Periodo.Hasta:yyyy-MM-dd}.csv";
            return File(bytes, "text/csv; charset=utf-8", nombre);
        }

        // GET api/visibilidad/general?dias=30   → comparativo de todos los municipios
        [HttpGet("general")]
        [Authorize(Roles = Roles.AdminGeneral)]
        public async Task<IActionResult> General([FromQuery] int dias = 30, CancellationToken ct = default) =>
            Ok(await _servicio.ResumenGeneralAsync(dias, ct));

        /// <summary>"Villa de Leyva" → "villa-de-leyva" (nombre de archivo sin tildes ni espacios).</summary>
        private static string Slug(string texto)
        {
            var normalizado = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalizado)
            {
                if (char.IsAsciiLetterOrDigit(c)) sb.Append(char.ToLowerInvariant(c));
                else if ((c == ' ' || c == '-') && sb.Length > 0 && sb[^1] != '-') sb.Append('-');
            }
            return sb.ToString().Trim('-');
        }
    }
}
