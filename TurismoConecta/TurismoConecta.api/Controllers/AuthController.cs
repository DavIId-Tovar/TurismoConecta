using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurismoConecta.api.DTOs.Auth;
using TurismoConecta.api.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace TurismoConecta.api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IVerificacionCorreoService _verificacion;

    public AuthController(IAuthService authService, IVerificacionCorreoService verificacion)
    {
        _authService = authService;
        _verificacion = verificacion;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        try
        {
            var response = await _authService.RegistrarAsync(dto);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        try
        {
            var response = await _authService.LoginAsync(dto);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { mensaje = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto dto)
    {
        await _authService.ForgotPasswordAsync(dto);
        return Ok("Si el correo existe en nuestro sistema, recibirás un enlace de recuperación.");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto dto)
    {
        try
        {
            await _authService.ResetPasswordAsync(dto);
            return Ok("Contraseña actualizada correctamente.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ═════════════ VERIFICACIÓN DE CORREO ═════════════

    // POST api/auth/verificar-correo  { token }  → lo llama la página /verificar-correo (sin sesión)
    [HttpPost("verificar-correo")]
    public async Task<IActionResult> VerificarCorreo([FromBody] VerificarCorreoRequestDto dto)
    {
        var (resultado, email) = await _verificacion.VerificarAsync(dto.Token);
        return resultado switch
        {
            ResultadoVerificacion.Ok => Ok(new { mensaje = "¡Listo! Tu correo quedó confirmado.", email }),
            ResultadoVerificacion.YaVerificado => Ok(new { mensaje = "Tu correo ya estaba confirmado.", email }),
            ResultadoVerificacion.Expirado => BadRequest(new { mensaje = "El enlace venció. Pide uno nuevo desde tu perfil.", codigo = "expirado" }),
            _ => BadRequest(new { mensaje = "El enlace no es válido. Revisa que lo hayas copiado completo o pide uno nuevo.", codigo = "invalido" })
        };
    }

    // POST api/auth/reenviar-verificacion  → nuevo enlace para el usuario en sesión
    [HttpPost("reenviar-verificacion")]
    [Authorize]
    public async Task<IActionResult> ReenviarVerificacion()
    {
        var (resultado, segundos) = await _verificacion.ReenviarAsync(UsuarioActual);
        return resultado switch
        {
            ResultadoVerificacion.Ok => Ok(new { mensaje = "Te enviamos un enlace nuevo. Revisa tu bandeja de entrada (y la de spam).", segundosEspera = segundos }),
            ResultadoVerificacion.YaVerificado => Ok(new { mensaje = "Tu correo ya está confirmado.", segundosEspera = 0 }),
            ResultadoVerificacion.Espera => StatusCode(StatusCodes.Status429TooManyRequests,
                new { mensaje = $"Ya te enviamos un enlace. Espera {segundos} s para pedir otro.", segundosEspera = segundos }),
            ResultadoVerificacion.ErrorEnvio => StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { mensaje = "No pudimos enviar el correo en este momento. Intenta más tarde." }),
            _ => NotFound(new { mensaje = "Usuario no encontrado." })
        };
    }

    // GET api/auth/estado-correo  → { email, emailConfirmado }
    [HttpGet("estado-correo")]
    [Authorize]
    public async Task<IActionResult> EstadoCorreo()
    {
        var estado = await _verificacion.ObtenerEstadoAsync(UsuarioActual);
        return estado is null ? NotFound() : Ok(estado);
    }

    private int UsuarioActual => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
