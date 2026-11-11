namespace TurismoConecta.web.Client.Models
{
    public class PerfilResponse
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