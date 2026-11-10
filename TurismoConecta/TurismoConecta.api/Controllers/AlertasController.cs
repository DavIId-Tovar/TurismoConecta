using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurismoConecta.api.Constants;
using TurismoConecta.api.DTOs.Alertas;
using TurismoConecta.api.Services.Interfaces;

namespace TurismoConecta.api.Controllers
{
    /// <summary>Alertas municipales: eventos, cierres, cambios de horario y movilidad.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AlertasController : ControllerBase
    {
        private const int MaxMunicipiosPorConsulta = 30;

        private readonly IAlertaService _alertaService;
        public AlertasController(IAlertaService alertaService) => _alertaService = alertaService;

        private int UsuarioActual => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        /// <summary>El rol con más alcance que tenga el usuario.</summary>
        private string RolActual =>
            User.IsInRole(Roles.AdminGeneral) ? Roles.AdminGeneral
            : User.IsInRole(Roles.AdminMunicipio) ? Roles.AdminMunicipio
            : User.IsInRole(Roles.AdminEstablecimiento) ? Roles.AdminEstablecimiento
            : Roles.Usuario;

        // ═════════════ PÚBLICO ═════════════

        // GET api/alertas/municipio/5  → vigentes y próximas de la ficha del municipio
        [HttpGet("municipio/{idMunicipio:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeMunicipio(int idMunicipio, CancellationToken ct) =>
            Ok(await _alertaService.ListarVigentesAsync(new[] { idMunicipio }, null, null, ct));

        // GET api/alertas/vigentes?municipios=1,4,7&desde=2026-11-12&hasta=2026-11-15
        // Para el itinerario (guardado, compartido o en construcción) y el mapa.
        [HttpGet("vigentes")]
        [AllowAnonymous]
        public async Task<IActionResult> Vigentes([FromQuery] string? municipios, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, CancellationToken ct)
        {
            var ids = (municipios ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => int.TryParse(t, out var id) ? id : 0)
                .Where(id => id > 0)
                .Distinct()
                .Take(MaxMunicipiosPorConsulta)
                .ToList();

            // "hasta" llega como fecha: se incluye el día completo
            DateTime? hastaFinDelDia = hasta?.TimeOfDay == TimeSpan.Zero ? hasta.Value.AddDays(1) : hasta;

            return Ok(await _alertaService.ListarVigentesAsync(ids, desde, hastaFinDelDia, ct));
        }

        // ═════════════ GESTIÓN (admins y comercios) ═════════════

        [HttpGet("gestion")]
        [Authorize(Roles = Roles.TodosLosAdmins)]
        public async Task<IActionResult> Gestion(CancellationToken ct) =>
            Ok(await _alertaService.ListarGestionAsync(UsuarioActual, RolActual, ct));

        [HttpGet("opciones")]
        [Authorize(Roles = Roles.TodosLosAdmins)]
        public async Task<IActionResult> Opciones(CancellationToken ct) =>
            Ok(await _alertaService.ObtenerOpcionesAsync(UsuarioActual, RolActual, ct));

        [HttpPost]
        [Authorize(Roles = Roles.TodosLosAdmins)]
        public async Task<IActionResult> Crear([FromBody] GuardarAlertaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (resultado, error, alerta) = await _alertaService.CrearAsync(UsuarioActual, RolActual, dto, ct);
            return resultado == ResultadoAlerta.Ok ? Ok(alerta) : ARespuesta(resultado, error);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.TodosLosAdmins)]
        public async Task<IActionResult> Editar(int id, [FromBody] GuardarAlertaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (resultado, error, alerta) = await _alertaService.EditarAsync(id, UsuarioActual, RolActual, dto, ct);
            return resultado == ResultadoAlerta.Ok ? Ok(alerta) : ARespuesta(resultado, error);
        }

        [HttpPost("{id:int}/finalizar")]
        [Authorize(Roles = Roles.TodosLosAdmins)]
        public async Task<IActionResult> Finalizar(int id, CancellationToken ct)
        {
            var (resultado, error) = await _alertaService.FinalizarAsync(id, UsuarioActual, RolActual, ct);
            return ARespuesta(resultado, error);
        }

        private IActionResult ARespuesta(ResultadoAlerta resultado, string? error) => resultado switch
        {
            ResultadoAlerta.Ok => NoContent(),
            ResultadoAlerta.NoEncontrada => NotFound(new { mensaje = error }),
            ResultadoAlerta.SinPermiso => StatusCode(StatusCodes.Status403Forbidden, new { mensaje = error }),
            _ => BadRequest(new { mensaje = error })
        };
    }
}
