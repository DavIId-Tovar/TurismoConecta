using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurismoConecta.api.DTOs.Recomendaciones;
using TurismoConecta.api.Services.Interfaces;

namespace TurismoConecta.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecomendacionesController : ControllerBase
    {
        private readonly IRecomendacionService _recomendacionService;
        public RecomendacionesController(IRecomendacionService recomendacionService) => _recomendacionService = recomendacionService;

        private int UsuarioActual => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        /// <summary>Recomendaciones personales: mezcla gustos, favoritos, reseñas e itinerarios del usuario.</summary>
        [HttpGet("para-mi")]
        [Authorize]
        [ProducesResponseType(typeof(List<RecomendacionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ParaMi([FromQuery] int cantidad = 6, CancellationToken ct = default) =>
            Ok(await _recomendacionService.ParaUsuarioAsync(UsuarioActual, cantidad, ct));

        /// <summary>Recomendaciones sin sesión, a partir de los gustos marcados en pantalla ("1,4").</summary>
        [HttpGet("para-ti")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<RecomendacionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ParaTi([FromQuery] string? etiquetas, [FromQuery] int cantidad = 6, CancellationToken ct = default) =>
            Ok(await _recomendacionService.PorGustosAsync(LeerIds(etiquetas), cantidad, ct));

        /// <summary>"Si te gustó X…": municipios parecidos por etiquetas y bien calificados.</summary>
        [HttpGet("similares/{idMunicipio:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<RecomendacionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Similares(int idMunicipio, [FromQuery] int cantidad = 4, CancellationToken ct = default) =>
            Ok(await _recomendacionService.SimilaresAsync(idMunicipio, cantidad, ct));

        /// <summary>Los gustos guardados del usuario en sesión.</summary>
        [HttpGet("preferencias")]
        [Authorize]
        [ProducesResponseType(typeof(PreferenciasDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> MisPreferencias(CancellationToken ct = default) =>
            Ok(new PreferenciasDto { IdsEtiquetas = await _recomendacionService.ObtenerPreferenciasAsync(UsuarioActual, ct) });

        /// <summary>Reemplaza los gustos del usuario por la lista enviada (máximo 10).</summary>
        [HttpPut("preferencias")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GuardarPreferencias([FromBody] PreferenciasDto dto, CancellationToken ct = default)
        {
            var (exito, error) = await _recomendacionService.GuardarPreferenciasAsync(UsuarioActual, dto.IdsEtiquetas, ct);
            return exito ? NoContent() : BadRequest(new { mensaje = error });
        }

        // "3,7" → [3, 7]. Lo que no sea un número se ignora en vez de reventar con un 500
        private static List<int> LeerIds(string? valores) =>
            (valores ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.TryParse(s, out var id) ? id : 0)
                .Where(id => id > 0)
                .Distinct()
                .ToList();
    }
}
