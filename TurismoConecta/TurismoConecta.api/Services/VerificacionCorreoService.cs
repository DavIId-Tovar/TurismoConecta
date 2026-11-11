using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TurismoConecta.api.Data;
using TurismoConecta.api.DTOs.Auth;
using TurismoConecta.api.Models;
using TurismoConecta.api.Services.Interfaces;

namespace TurismoConecta.api.Services;

/// <summary>
/// Verificación de correo (Módulo 1 del documento técnico).
///
/// El enlace NO se guarda en la base de datos: es un token firmado con HMAC
/// que lleva el id del usuario y la fecha de vencimiento, y cuya firma incluye
/// el correo. Si alguien cambia un número del enlace, la firma deja de cuadrar;
/// si el usuario cambia de correo, los enlaces viejos dejan de servir.
///
///   token = {idUsuario}.{venceUnix}.{firma}
///   firma = HMAC-SHA256("verificar-correo|{id}|{correo}|{vence}")
/// </summary>
public class VerificacionCorreoService : IVerificacionCorreoService
{
    private const string Proposito = "verificar-correo";

    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;
    private readonly ILogger<VerificacionCorreoService> _logger;

    public VerificacionCorreoService(AppDbContext db, IEmailService email, IMemoryCache cache,
        IConfiguration config, ILogger<VerificacionCorreoService> logger)
    {
        _db = db;
        _email = email;
        _cache = cache;
        _config = config;
        _logger = logger;
    }

    private int HorasValidez => _config.GetValue("VerificacionCorreo:HorasValidez", 24);
    private int SegundosEntreEnvios => _config.GetValue("VerificacionCorreo:SegundosEntreEnvios", 60);

    // ───────────────────────── Enviar ─────────────────────────

    public async Task<bool> EnviarAsync(Usuario usuario)
    {
        try
        {
            var vence = DateTimeOffset.UtcNow.AddHours(HorasValidez).ToUnixTimeSeconds();
            var token = $"{usuario.IdUsuario}.{vence}.{Firmar(usuario.IdUsuario, usuario.Email, vence)}";

            var frontendUrl = _config["App:FrontendUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("Falta 'App:FrontendUrl' en appsettings.json");
            var enlace = $"{frontendUrl}/verificar-correo?token={Uri.EscapeDataString(token)}";

            await _email.EnviarCorreoAsync(usuario.Email, "Confirma tu correo - TurismoConecta",
                CuerpoCorreo(usuario.Nombre, enlace, HorasValidez));

            _cache.Set(ClaveEspera(usuario.IdUsuario), DateTime.UtcNow, TimeSpan.FromSeconds(SegundosEntreEnvios));
            return true;
        }
        catch (Exception ex)
        {
            // El registro no debe fallar porque el SMTP esté caído: el usuario puede pedir otro enlace.
            _logger.LogError(ex, "No se pudo enviar el correo de verificación al usuario {IdUsuario}", usuario.IdUsuario);
            return false;
        }
    }

    // ───────────────────────── Verificar ─────────────────────────

    public async Task<(ResultadoVerificacion resultado, string? email)> VerificarAsync(string token)
    {
        var partes = (token ?? string.Empty).Trim().Split('.');
        if (partes.Length != 3
            || !int.TryParse(partes[0], NumberStyles.None, CultureInfo.InvariantCulture, out var idUsuario)
            || !long.TryParse(partes[1], NumberStyles.None, CultureInfo.InvariantCulture, out var vence))
            return (ResultadoVerificacion.Invalido, null);

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
        if (usuario is null || !usuario.Activo) return (ResultadoVerificacion.Invalido, null);

        // La firma se revisa ANTES que el vencimiento: así un enlace alterado nunca dice "expirado"
        var esperada = Encoding.ASCII.GetBytes(Firmar(idUsuario, usuario.Email, vence));
        var recibida = Encoding.ASCII.GetBytes(partes[2]);
        if (!CryptographicOperations.FixedTimeEquals(esperada, recibida))
            return (ResultadoVerificacion.Invalido, null);

        if (usuario.EmailConfirmado) return (ResultadoVerificacion.YaVerificado, usuario.Email);

        if (DateTimeOffset.FromUnixTimeSeconds(vence) < DateTimeOffset.UtcNow)
            return (ResultadoVerificacion.Expirado, usuario.Email);

        usuario.EmailConfirmado = true;
        await _db.SaveChangesAsync();
        return (ResultadoVerificacion.Ok, usuario.Email);
    }

    // ───────────────────────── Reenviar / estado ─────────────────────────

    public async Task<(ResultadoVerificacion resultado, int segundosEspera)> ReenviarAsync(int idUsuario)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario && u.Activo);
        if (usuario is null) return (ResultadoVerificacion.Invalido, 0);
        if (usuario.EmailConfirmado) return (ResultadoVerificacion.YaVerificado, 0);

        if (_cache.TryGetValue(ClaveEspera(idUsuario), out DateTime ultimoEnvio))
        {
            var faltan = SegundosEntreEnvios - (int)(DateTime.UtcNow - ultimoEnvio).TotalSeconds;
            if (faltan > 0) return (ResultadoVerificacion.Espera, faltan);
        }

        return await EnviarAsync(usuario)
            ? (ResultadoVerificacion.Ok, SegundosEntreEnvios)
            : (ResultadoVerificacion.ErrorEnvio, 0);
    }

    public async Task<EstadoCorreoDto?> ObtenerEstadoAsync(int idUsuario) =>
        await _db.Usuarios.AsNoTracking()
            .Where(u => u.IdUsuario == idUsuario)
            .Select(u => new EstadoCorreoDto { Email = u.Email, EmailConfirmado = u.EmailConfirmado })
            .FirstOrDefaultAsync();

    // ───────────────────────── Apoyo ─────────────────────────

    private static string ClaveEspera(int idUsuario) => $"verificacion-correo:{idUsuario}";

    private string Firmar(int idUsuario, string email, long vence)
    {
        // Clave propia derivada de la del JWT: un token de sesión nunca sirve como enlace y viceversa
        var claveJwt = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key no está configurada.");
        var clave = SHA256.HashData(Encoding.UTF8.GetBytes($"{Proposito}:{claveJwt}"));

        var datos = Encoding.UTF8.GetBytes(
            $"{Proposito}|{idUsuario}|{email.Trim().ToLowerInvariant()}|{vence}");
        var firma = HMACSHA256.HashData(clave, datos);

        return Convert.ToBase64String(firma).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string CuerpoCorreo(string nombre, string enlace, int horas)
    {
        var nombreSeguro = WebUtility.HtmlEncode(nombre);
        var enlaceSeguro = WebUtility.HtmlEncode(enlace);
        return $@"
<div style=""font-family:Segoe UI,Arial,sans-serif;max-width:520px;margin:0 auto;color:#1f2937"">
  <h2 style=""color:#1A6FA8;margin-bottom:8px"">¡Hola, {nombreSeguro}!</h2>
  <p>Gracias por unirte a <strong>TurismoConecta</strong>. Confirma que este correo es tuyo para poder publicar reseñas y recibir avisos de tus destinos.</p>
  <p style=""text-align:center;margin:28px 0"">
    <a href=""{enlaceSeguro}"" style=""background:#1A6FA8;color:#ffffff;padding:12px 24px;border-radius:8px;text-decoration:none;font-weight:600"">Confirmar mi correo</a>
  </p>
  <p style=""font-size:13px;color:#6b7280"">El enlace vence en {horas} horas. Si no creaste esta cuenta, ignora este mensaje.</p>
  <p style=""font-size:12px;color:#9ca3af;word-break:break-all"">Si el botón no funciona, copia este enlace en tu navegador:<br>{enlaceSeguro}</p>
</div>";
    }
}
