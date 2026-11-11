namespace TurismoConecta.web.Client.Models
{
    public class EstadoCorreoResponse
    {
        public string Email { get; set; } = string.Empty;
        public bool EmailConfirmado { get; set; }
    }

    public class ReenvioVerificacionResponse
    {
        public string? Mensaje { get; set; }
        public int SegundosEspera { get; set; }
    }
}
