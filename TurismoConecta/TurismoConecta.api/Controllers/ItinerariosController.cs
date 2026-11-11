
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurismoConecta.api.DTOs.Itinerarios;
using TurismoConecta.api.Services.Interfaces;
using TurismoConecta.api.Services.Exportacion;


namespace TurismoConecta.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItinerariosController : ControllerBase
    {
        private readonly IItinerarioService _service;
        private readonly IExportarItinerarioService _exportar;

        public ItinerariosController(IItinerarioService service, IExportarItinerarioService exportar)
        {
            _service = service;
            _exportar = exportar;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Listar()
        {
            var idUsuario = ObtenerIdUsuarioActual();
            return Ok(await _service.ListarPorUsuarioAsync(idUsuario));
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> ObtenerDetalle(int id)
        {
            var idUsuario = ObtenerIdUsuarioActual();
            var itinerario = await _service.ObtenerDetalleAsync(id, idUsuario);
            return itinerario is null ? NotFound() : Ok(itinerario);
        }

        [HttpGet("compartido/{codigo:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerCompartido(Guid codigo)
        {
            var itinerario = await _service.ObtenerPorCodigoCompartirAsync(codigo);
            return itinerario is null ? NotFound("El enlace no es válido o el itinerario ya no está disponible.") : Ok(itinerario);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Crear([FromBody] ItinerarioCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var idUsuario = ObtenerIdUsuarioActual();
            var (exito, error, itinerario) = await _service.CrearAsync(idUsuario, dto);

            if (!exito) return BadRequest(new { mensaje = error });

            return CreatedAtAction(nameof(ObtenerDetalle), new { id = itinerario!.IdItinerario }, itinerario);
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Eliminar(int id)
        {
            var idUsuario = ObtenerIdUsuarioActual();
            var (exito, error) = await _service.EliminarAsync(id, idUsuario);

            if (!exito)
                return error!.Contains("permiso") ? Forbid() : NotFound(error);

            return NoContent();
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ItinerarioUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var idUsuario = ObtenerIdUsuarioActual();
            var (exito, error, itinerario) = await _service.ActualizarAsync(id, idUsuario, dto);

            if (!exito)
                return error!.Contains("permiso") ? Forbid() : NotFound(new { mensaje = error });

            return Ok(itinerario);
        }


        [HttpPost("{id:int}/compartir")]
        [Authorize]
        public async Task<IActionResult> ToggleCompartir(int id)
        {
            var idUsuario = ObtenerIdUsuarioActual();
            var (exito, error, compartido, codigoCompartir) = await _service.ToggleCompartirAsync(id, idUsuario);

            if (!exito)
                return error!.Contains("permiso") ? Forbid() : NotFound(new { mensaje = error });

            return Ok(new { compartido, codigoCompartir });
        }


        // ═════════════ EXPORTAR (Módulo 5) ═════════════

        // GET api/itinerarios/7/exportar/pdf  |  .../exportar/ics
        [HttpGet("{id:int}/exportar/{formato}")]
        [Authorize]
        public async Task<IActionResult> Exportar(int id, string formato, CancellationToken ct)
        {
            var itinerario = await _exportar.ObtenerAsync(id, ObtenerIdUsuarioActual(), ct);
            return itinerario is null
                ? NotFound(new { mensaje = "El itinerario no existe." })
                : ArchivoExportado(itinerario, formato);
        }

        // GET api/itinerarios/compartido/{codigo}/exportar/pdf  → para quien recibe el enlace público
        [HttpGet("compartido/{codigo:guid}/exportar/{formato}")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportarCompartido(Guid codigo, string formato, CancellationToken ct)
        {
            var itinerario = await _exportar.ObtenerCompartidoAsync(codigo, ct);
            return itinerario is null
                ? NotFound(new { mensaje = "El enlace no es válido o el itinerario ya no es público." })
                : ArchivoExportado(itinerario, formato);
        }

        private IActionResult ArchivoExportado(ItinerarioExportable itinerario, string formato)
        {
            switch (formato.ToLowerInvariant())
            {
                case "pdf":
                    return File(_exportar.GenerarPdf(itinerario), "application/pdf",
                        _exportar.NombreArchivo(itinerario, "pdf"));

                case "ics":
                    var ics = _exportar.GenerarIcs(itinerario);
                    if (ics is null)
                        return BadRequest(new { mensaje = "Agrega las fechas del viaje para pasarlo a tu calendario." });
                    return File(System.Text.Encoding.UTF8.GetBytes(ics), "text/calendar; charset=utf-8",
                        _exportar.NombreArchivo(itinerario, "ics"));

                default:
                    return BadRequest(new { mensaje = "Formato no soportado. Usa pdf o ics." });
            }
        }

        private int ObtenerIdUsuarioActual()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(idClaim!);
        }


    }
}