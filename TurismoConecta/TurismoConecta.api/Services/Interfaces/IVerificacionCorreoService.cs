using TurismoConecta.api.DTOs.Auth;
using TurismoConecta.api.Models;

namespace TurismoConecta.api.Services.Interfaces;

public interface IVerificacionCorreoService
{
    /// <summary>Envía el correo con el enlace de verificación. Nunca lanza: si el SMTP falla, lo registra y devuelve false.</summary>
    Task<bool> EnviarAsync(Usuario usuario);

    /// <summary>Valida el enlace y marca el correo como confirmado.</summary>
    Task<(ResultadoVerificacion resultado, string? email)> VerificarAsync(string token);

    /// <summary>Reenvía el enlace al usuario en sesión (con espera mínima entre envíos).</summary>
    Task<(ResultadoVerificacion resultado, int segundosEspera)> ReenviarAsync(int idUsuario);

    Task<EstadoCorreoDto?> ObtenerEstadoAsync(int idUsuario);
}
