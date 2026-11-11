using System.ComponentModel.DataAnnotations;

namespace TurismoConecta.api.DTOs.Auth;

public enum ResultadoVerificacion
{
    Ok,
    YaVerificado,
    Invalido,       // enlace alterado, de otro correo o de un usuario que no existe
    Expirado,
    Espera,         // pidió otro enlace hace muy poco
    ErrorEnvio      // el SMTP falló
}

public class VerificarCorreoRequestDto
{
    [Required, MaxLength(300)]
    public string Token { get; set; } = string.Empty;
}

public class EstadoCorreoDto
{
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmado { get; set; }
}
