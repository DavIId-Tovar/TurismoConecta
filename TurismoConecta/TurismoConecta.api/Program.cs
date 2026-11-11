using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Threading.Tasks;
using TurismoConecta.api.Data;
using TurismoConecta.api.Hubs;
using TurismoConecta.api.Services;
using TurismoConecta.api.Services.Etiquetas;
using TurismoConecta.api.Services.Interfaces;
using TurismoConecta.api.Services.Recomendaciones;
using TurismoConecta.api.Services.Rutas;


var builder = WebApplication.CreateBuilder(args);



//  ASÍ DEBE QUEDAR:
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)
    ));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVerificacionCorreoService, VerificacionCorreoService>();
builder.Services.AddScoped<TurismoConecta.api.Services.Exportacion.IExportarItinerarioService,
                           TurismoConecta.api.Services.Exportacion.ExportarItinerarioService>();

// QuestPDF (PDF de itinerarios): licencia Community, gratuita para este proyecto
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<ISitioTuristicoService, SitioTuristicoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IMunicipioService, MunicipioService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IMunicipioEtiquetaService, MunicipioEtiquetaService>();
builder.Services.AddScoped<IItinerarioService, ItinerarioService>();
builder.Services.AddScoped<IEtiquetaService, EtiquetaService>();
builder.Services.AddScoped<INegocioService, NegocioService>();
builder.Services.AddScoped<IReseñaService, ReseñaService>();
builder.Services.AddScoped<IRecomendacionService, RecomendacionService>();
// Recomendaciones con ML.NET: el modelo vive en memoria (singleton) y se entrena en segundo plano.
// con las interacciones de los usuarios (favoritos, reseñas, itinerarios) se va alimentando y mejorando.
// Rutas reales por carretera (OpenRouteService). Sin clave, el motor usa la estimación en línea recta
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<RutaCarreteraService>(cliente =>
{
    cliente.BaseAddress = new Uri(builder.Configuration["OpenRouteService:BaseUrl"] ?? "https://api.openrouteservice.org/");
    cliente.Timeout = TimeSpan.FromSeconds(builder.Configuration.GetValue("OpenRouteService:SegundosEspera", 10));
});
builder.Services.AddSingleton<ModeloColaborativo>();
builder.Services.AddScoped<EntrenadorColaborativo>();
builder.Services.AddHostedService<EntrenamientoRecomendacionesService>();
builder.Services.AddScoped<IFavoritoService, FavoritoService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddScoped<IAlertaService, AlertaService>();
builder.Services.AddScoped<IVisibilidadService, VisibilidadService>();



var origenesPermitidos = builder.Configuration
    .GetSection("Cors:OrigenesPermitidos")
    .Get<string[]>() ?? []; 


builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
    {
        policy
            .WithOrigins(origenesPermitidos)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            // El nombre del CSV del reporte municipal viaja en esta cabecera
            .WithExposedHeaders("Content-Disposition"); 

  
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();
app.MapScalarApiReference(); // esto nos permite hacer pruebas desde la interfaz de scalar -- tener instalado el Nuget
                             // scalar.AspNetCore; 


               



app.UseHttpsRedirection();
app.UseCors("BlazorPolicy");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificacionesHub>("/hubs/notificaciones");
app.UseStaticFiles();

app.Run();
