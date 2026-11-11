// DTOs/Usuarios/PerfilResponseDto.cs
namespace TurismoConecta.api.DTOs.Usuarios
{
    public class PerfilResponseDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string? FotoUrl { get; set; }
        public bool EmailConfirmado { get; set; }
    }
}