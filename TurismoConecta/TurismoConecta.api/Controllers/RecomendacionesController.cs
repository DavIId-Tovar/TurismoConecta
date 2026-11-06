using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurismoConecta.api.Constants;
using TurismoConecta.api.DTOs.Recomendaciones;
using TurismoConecta.api.Services.Interfaces;
using TurismoConecta.api.Services.Recomendaciones;

namespace TurismoConecta.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecomendacionesController : ControllerBase
    {
        private readonly IRecomendacionService _recomendacionService;
        public RecomendacionesController(IRecomendacionService recomendacionService) => _recomendacionService = recomendacionService;

        private int UsuarioActual => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // En los endpoints públicos: el usuario si mandó su token, null si navega sin sesión
        private int? UsuarioOpcional =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

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

        /// <summary>"Si te gustó X…": municipios parecidos por etiquetas y bien calificados (con sesión, sin los ocultados).</summary>
        [HttpGet("similares/{idMunicipio:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<RecomendacionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Similares(int idMunicipio, [FromQuery] int cantidad = 4, CancellationToken ct = default) =>
            Ok(await _recomendacionService.SimilaresAsync(idMunicipio, UsuarioOpcional, cantidad, ct));

        /// <summary>
        /// "Queda cerca": municipios a poca distancia del indicado (para armar el itinerario).
        /// excluir = los que ya están en la ruta ("3,7"). Con sesión, también pesan los gustos del usuario.
        /// </summary>
        [HttpGet("cerca/{idMunicipio:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<RecomendacionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Cerca(int idMunicipio, [FromQuery] string? excluir, [FromQuery] int cantidad = 4, CancellationToken ct = default) =>
            Ok(await _recomendacionService.CercaAsync(idMunicipio, UsuarioOpcional, LeerIds(excluir), cantidad, ct));

        /// <summary>
        /// "Sorpréndeme": propone una ruta de una parada por día con los mejores destinos para el usuario.
        /// Con sesión usa su perfil completo; sin sesión, los gustos marcados ("1,4"). variante = "Otra sorpresa".
        /// No guarda nada: el cliente llena el formulario de Crear itinerario.
        /// </summary>
        [HttpGet("sorprendeme")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PropuestaItinerarioDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Sorprendeme(
            [FromQuery] int dias = 3, [FromQuery] int variante = 0, [FromQuery] string? etiquetas = null, CancellationToken ct = default) =>
            Ok(await _recomendacionService.SorprendemeAsync(UsuarioOpcional, LeerIds(etiquetas), dias, variante, ct));

        /// <summary>
        /// Km y tiempo estimados de una ruta ("4,1,2"): resumen de Crear itinerario.
        /// optimizar=true la devuelve ordenada por cercanía (la primera parada se queda como punto de partida).
        /// </summary>
        [HttpGet("ruta")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RutaEstimadaDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> EstimarRuta([FromQuery] string? ids, [FromQuery] bool optimizar = false, CancellationToken ct = default) =>
            Ok(await _recomendacionService.EstimarRutaAsync(LeerIds(ids).Take(30).ToList(), optimizar, ct));

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

        /// <summary>"No me interesa": el municipio deja de recomendarse y sus etiquetas pierden peso.</summary>
        [HttpPost("descartar/{idMunicipio:int}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Descartar(int idMunicipio, CancellationToken ct = default)
        {
            var (exito, error) = await _recomendacionService.DescartarAsync(UsuarioActual, idMunicipio, ct);
            return exito ? NoContent() : NotFound(new { mensaje = error });
        }

        /// <summary>Deshace un "No me interesa" (botón Deshacer o "Volver a mostrar" en el Perfil).</summary>
        [HttpDelete("descartar/{idMunicipio:int}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Restaurar(int idMunicipio, CancellationToken ct = default)
        {
            await _recomendacionService.RestaurarAsync(UsuarioActual, idMunicipio, ct);
            return NoContent();
        }

        /// <summary>Los municipios que el usuario ocultó, del más reciente al más antiguo.</summary>
        [HttpGet("descartados")]
        [Authorize]
        [ProducesResponseType(typeof(List<MunicipioDescartadoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Descartados(CancellationToken ct = default) =>
            Ok(await _recomendacionService.ListarDescartadosAsync(UsuarioActual, ct));

        /// <summary>Estado del modelo colaborativo de ML.NET (panel del admin general).</summary>
        [HttpGet("modelo")]
        [Authorize(Roles = Roles.AdminGeneral)]
        [ProducesResponseType(typeof(EstadoModeloDto), StatusCodes.Status200OK)]
        public IActionResult EstadoModelo([FromServices] ModeloColaborativo modelo) => Ok(modelo.Estado);

        /// <summary>Entrena el modelo ahora mismo, sin esperar al entrenamiento automático.</summary>
        [HttpPost("modelo/entrenar")]
        [Authorize(Roles = Roles.AdminGeneral)]
        [ProducesResponseType(typeof(EstadoModeloDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> EntrenarModelo([FromServices] EntrenadorColaborativo entrenador, CancellationToken ct = default) =>
            Ok(await entrenador.EntrenarAsync(ct));

        /// <summary>Uso del motor: cuántos tienen gustos, cuántos descartes, municipios sin coordenadas.</summary>
        [HttpGet("metricas")]
        [Authorize(Roles = Roles.AdminGeneral)]
        [ProducesResponseType(typeof(MetricasRecomendacionesDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Metricas(CancellationToken ct = default) =>
            Ok(await _recomendacionService.MetricasAsync(ct));

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
