namespace TurismoConecta.api.Services.Recomendaciones
{
    /// <summary>
    /// Tarea en segundo plano: entrena el modelo colaborativo al arrancar el API y luego cada N horas.
    /// Así ninguna petición del usuario espera a que el modelo se entrene.
    /// </summary>
    public class EntrenamientoRecomendacionesService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<EntrenamientoRecomendacionesService> _logger;
        private readonly TimeSpan _intervalo;

        public EntrenamientoRecomendacionesService(
            IServiceScopeFactory scopes,
            IConfiguration configuracion,
            ILogger<EntrenamientoRecomendacionesService> logger)
        {
            _scopes = scopes;
            _logger = logger;
            _intervalo = TimeSpan.FromHours(Math.Max(0.1, configuracion.GetValue("Recomendaciones:HorasEntreEntrenamientos", 6.0)));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Se espera un poco para no competir con el arranque del API
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

                using var temporizador = new PeriodicTimer(_intervalo);
                do
                {
                    await EntrenarAsync(stoppingToken);
                }
                while (await temporizador.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException)
            {
                // El API se está apagando
            }
        }

        private async Task EntrenarAsync(CancellationToken ct)
        {
            try
            {
                // El DbContext es "scoped": cada entrenamiento crea el suyo
                using var scope = _scopes.CreateScope();
                var entrenador = scope.ServiceProvider.GetRequiredService<EntrenadorColaborativo>();
                var estado = await entrenador.EntrenarAsync(ct);

                _logger.LogInformation("[Recomendaciones] {Mensaje}", estado.Mensaje);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Si falla, el motor sigue funcionando sin la parte colaborativa
                _logger.LogError(ex, "[Recomendaciones] Falló el entrenamiento del modelo colaborativo");
            }
        }
    }
}
