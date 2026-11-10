using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TurismoConecta.api.Constants;
using TurismoConecta.api.Data;
using TurismoConecta.api.DTOs.Alertas;
using TurismoConecta.api.Models;
using TurismoConecta.api.Services.Interfaces;

namespace TurismoConecta.api.Services
{
    /// <summary>
    /// Alertas municipales (Módulo 8). Quién puede qué:
    ///   AdminGeneral        → cualquier municipio, cualquier tipo.
    ///   AdminMunicipio      → solo su municipio asignado (puede nombrar un negocio de ese municipio).
    ///   AdminEstablecimiento→ solo sus negocios aprobados, y solo cierres o cambios de horario.
    /// Al crear (o al cambiar fechas/severidad) se avisa en vivo a los afectados.
    /// </summary>
    public class AlertaService : IAlertaService
    {
        private const int MaxHistorial = 100;
        private const int MaxDiasDuracion = 180;
        private const int MaxDiasAnticipacion = 365;

        private static readonly CultureInfo Es = new("es-CO");

        private readonly AppDbContext _context;
        private readonly INotificacionService _notificaciones;
        private readonly ILogger<AlertaService> _logger;

        public AlertaService(AppDbContext context, INotificacionService notificaciones, ILogger<AlertaService> logger)
        {
            _context = context;
            _notificaciones = notificaciones;
            _logger = logger;
        }

        // ═════════════ PÚBLICO ═════════════

        public async Task<List<AlertaDto>> ListarVigentesAsync(IReadOnlyCollection<int> idsMunicipios, DateTime? desde, DateTime? hasta, CancellationToken ct = default)
        {
            if (idsMunicipios.Count == 0) return new();

            var ahora = DateTime.Now;
            var query = _context.AlertasMunicipales
                .AsNoTracking()
                .Where(a => idsMunicipios.Contains(a.IdMunicipio)
                            && a.Activa
                            && a.FechaFin > ahora
                            && a.IdMunicipioNavigation.Activo);

            // Solo las que se cruzan con el viaje: empiezan antes de que termine y terminan después de que empiece
            if (hasta.HasValue) query = query.Where(a => a.FechaInicio < hasta.Value);
            if (desde.HasValue) query = query.Where(a => a.FechaFin > desde.Value);

            var alertas = await Proyectar(query, incluirCreador: false)
                .ToListAsync(ct);

            return Ordenar(alertas, ahora);
        }

        // ═════════════ GESTIÓN ═════════════

        public async Task<List<AlertaDto>> ListarGestionAsync(int idUsuario, string rol, CancellationToken ct = default)
        {
            var query = _context.AlertasMunicipales.AsNoTracking();

            switch (rol)
            {
                case Roles.AdminGeneral:
                    break;

                case Roles.AdminMunicipio:
                    var idMunicipio = await MunicipioAsignadoAsync(idUsuario, ct);
                    if (idMunicipio is null) return new();
                    query = query.Where(a => a.IdMunicipio == idMunicipio);
                    break;

                case Roles.AdminEstablecimiento:
                    // Las que publicó él, y las que otros publicaron sobre sus negocios
                    query = query.Where(a => a.IdUsuarioCreador == idUsuario
                        || (a.IdNegocioNavigation != null && a.IdNegocioNavigation.IdUsuario == idUsuario));
                    break;

                default:
                    return new();
            }

            var alertas = await Proyectar(query.OrderByDescending(a => a.FechaCreacion).Take(MaxHistorial), incluirCreador: true)
                .ToListAsync(ct);

            // Mismas reglas que CargarConPermisoAsync: el comercio solo gestiona las que él publicó
            foreach (var a in alertas)
                a.PuedeGestionar = rol != Roles.AdminEstablecimiento || a.IdUsuarioCreador == idUsuario;

            return Ordenar(alertas, DateTime.Now);
        }

        public async Task<OpcionesAlertaDto> ObtenerOpcionesAsync(int idUsuario, string rol, CancellationToken ct = default)
        {
            var opciones = new OpcionesAlertaDto
            {
                Severidades = SeveridadesAlerta.Todas.ToList(),
                Tipos = TiposAlerta.Todos.ToList()
            };

            var negociosAprobados = _context.Negocios.AsNoTracking()
                .Where(n => n.Estado == EstadosNegocio.Aprobado && n.IdMunicipioNavigation.Activo);

            switch (rol)
            {
                case Roles.AdminGeneral:
                    opciones.Alcance = "General";
                    opciones.Municipios = await _context.Municipios.AsNoTracking()
                        .Where(m => m.Activo)
                        .OrderBy(m => m.Nombre)
                        .Select(m => new OpcionDto { Id = m.IdMunicipio, Nombre = m.Nombre })
                        .ToListAsync(ct);
                    break;

                case Roles.AdminMunicipio:
                    opciones.Alcance = "Municipio";
                    var idMunicipio = await MunicipioAsignadoAsync(idUsuario, ct);
                    if (idMunicipio is null) break;
                    opciones.Municipios = await _context.Municipios.AsNoTracking()
                        .Where(m => m.IdMunicipio == idMunicipio)
                        .Select(m => new OpcionDto { Id = m.IdMunicipio, Nombre = m.Nombre })
                        .ToListAsync(ct);
                    negociosAprobados = negociosAprobados.Where(n => n.IdMunicipio == idMunicipio);
                    break;

                case Roles.AdminEstablecimiento:
                    opciones.Alcance = "Comercio";
                    opciones.Tipos = TiposAlerta.DeComercio.ToList();
                    negociosAprobados = negociosAprobados.Where(n => n.IdUsuario == idUsuario);
                    break;

                default:
                    return opciones;
            }

            opciones.Negocios = await negociosAprobados
                .OrderBy(n => n.Nombre)
                .Select(n => new OpcionNegocioDto { Id = n.IdNegocio, Nombre = n.Nombre, IdMunicipio = n.IdMunicipio })
                .ToListAsync(ct);

            // El comercio ve sus municipios a partir de sus negocios
            if (rol == Roles.AdminEstablecimiento)
            {
                var ids = opciones.Negocios.Select(n => n.IdMunicipio).Distinct().ToList();
                opciones.Municipios = await _context.Municipios.AsNoTracking()
                    .Where(m => ids.Contains(m.IdMunicipio))
                    .Select(m => new OpcionDto { Id = m.IdMunicipio, Nombre = m.Nombre })
                    .ToListAsync(ct);
            }

            return opciones;
        }

        public async Task<(ResultadoAlerta resultado, string? error, AlertaDto? alerta)> CrearAsync(int idUsuario, string rol, GuardarAlertaDto dto, CancellationToken ct = default)
        {
            var ahora = DateTime.Now;

            var errorFechas = ValidarContenido(dto, ahora, esNueva: true);
            if (errorFechas is not null) return (ResultadoAlerta.Invalida, errorFechas, null);

            var (resultado, error, idMunicipio) = await ResolverLugarAsync(idUsuario, rol, dto, ct);
            if (resultado != ResultadoAlerta.Ok) return (resultado, error, null);

            var alerta = new AlertaMunicipal
            {
                IdMunicipio = idMunicipio!.Value,
                IdNegocio = dto.IdNegocio,
                Tipo = dto.Tipo,
                Severidad = dto.Severidad,
                Titulo = dto.Titulo.Trim(),
                Descripcion = dto.Descripcion.Trim(),
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                Activa = true,
                IdUsuarioCreador = idUsuario,
                FechaCreacion = ahora
            };

            _context.AlertasMunicipales.Add(alerta);
            await _context.SaveChangesAsync(ct);

            var creada = await ObtenerDtoAsync(alerta.IdAlertaMunicipal, ct);
            if (creada is not null) creada.PuedeGestionar = true;
            if (creada is not null) await NotificarAfectadosAsync(creada, idUsuario, esActualizacion: false, ct);

            return (ResultadoAlerta.Ok, null, creada);
        }

        public async Task<(ResultadoAlerta resultado, string? error, AlertaDto? alerta)> EditarAsync(int idAlerta, int idUsuario, string rol, GuardarAlertaDto dto, CancellationToken ct = default)
        {
            var (alerta, resultadoPermiso, errorPermiso) = await CargarConPermisoAsync(idAlerta, idUsuario, rol, ct);
            if (alerta is null) return (resultadoPermiso, errorPermiso, null);

            if (!alerta.Activa)
                return (ResultadoAlerta.Invalida, "Esta alerta ya fue finalizada; crea una nueva.", null);

            var ahora = DateTime.Now;
            var errorContenido = ValidarContenido(dto, ahora, esNueva: false);
            if (errorContenido is not null) return (ResultadoAlerta.Invalida, errorContenido, null);

            // El municipio no cambia al editar: se conserva el de la alerta
            dto.IdMunicipio = alerta.IdMunicipio;
            var (resultado, error, _) = await ResolverLugarAsync(idUsuario, rol, dto, ct);
            if (resultado != ResultadoAlerta.Ok) return (resultado, error, null);

            // Solo se vuelve a avisar si cambia algo que afecte el viaje
            var cambioImportante = alerta.FechaInicio != dto.FechaInicio
                || alerta.FechaFin != dto.FechaFin
                || alerta.Severidad != dto.Severidad
                || alerta.Tipo != dto.Tipo;

            alerta.IdNegocio = dto.IdNegocio;
            alerta.Tipo = dto.Tipo;
            alerta.Severidad = dto.Severidad;
            alerta.Titulo = dto.Titulo.Trim();
            alerta.Descripcion = dto.Descripcion.Trim();
            alerta.FechaInicio = dto.FechaInicio;
            alerta.FechaFin = dto.FechaFin;

            await _context.SaveChangesAsync(ct);

            var editada = await ObtenerDtoAsync(alerta.IdAlertaMunicipal, ct);
            if (editada is not null) editada.PuedeGestionar = true;
            if (editada is not null && cambioImportante)
                await NotificarAfectadosAsync(editada, idUsuario, esActualizacion: true, ct);

            return (ResultadoAlerta.Ok, null, editada);
        }

        public async Task<(ResultadoAlerta resultado, string? error)> FinalizarAsync(int idAlerta, int idUsuario, string rol, CancellationToken ct = default)
        {
            var (alerta, resultado, error) = await CargarConPermisoAsync(idAlerta, idUsuario, rol, ct);
            if (alerta is null) return (resultado, error);

            if (alerta.Activa)
            {
                alerta.Activa = false;
                // Si aún no vencía, se corta ahora para que el historial muestre cuándo terminó realmente
                var ahora = DateTime.Now;
                if (alerta.FechaFin > ahora) alerta.FechaFin = ahora > alerta.FechaInicio ? ahora : alerta.FechaInicio.AddMinutes(1);
                await _context.SaveChangesAsync(ct);
            }

            return (ResultadoAlerta.Ok, null);
        }

        // ═════════════ NOTIFICACIONES ═════════════

        /// <summary>
        /// Avisa a: turistas con el municipio (o el negocio) en favoritos, turistas con un itinerario
        /// que pasa por ahí en las fechas de la alerta, y comercios aprobados del municipio.
        /// Nunca al autor. Un fallo aquí no deshace la alerta.
        /// </summary>
        private async Task NotificarAfectadosAsync(AlertaDto alerta, int idAutor, bool esActualizacion, CancellationToken ct)
        {
            try
            {
                var destinatarios = await BuscarAfectadosAsync(alerta, ct);
                destinatarios.Remove(idAutor);
                if (destinatarios.Count == 0) return;

                var mensaje = ArmarMensaje(alerta, esActualizacion);

                foreach (var idUsuario in destinatarios)
                    await _notificaciones.EnviarAsync(idUsuario, CodigosNotificacion.AlertaMunicipal, mensaje, ct);

                _logger.LogInformation("Alerta {IdAlerta} notificada a {Cantidad} usuarios", alerta.IdAlerta, destinatarios.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo notificar la alerta {IdAlerta}", alerta.IdAlerta);
            }
        }

        private async Task<HashSet<int>> BuscarAfectadosAsync(AlertaDto alerta, CancellationToken ct)
        {
            var idMunicipio = alerta.IdMunicipio;
            var idNegocio = alerta.IdNegocio;
            var ids = new HashSet<int>();

            // 1. Favoritos del municipio o del negocio
            ids.UnionWith(await _context.Favoritos.AsNoTracking()
                .Where(f => f.IdMunicipio == idMunicipio || (idNegocio != null && f.IdNegocio == idNegocio))
                .Select(f => f.IdUsuario)
                .ToListAsync(ct));

            // 2. Itinerarios con una parada en ese municipio durante la alerta
            var paradas = await _context.ItinerarioDetalles.AsNoTracking()
                .Where(d => d.IdMunicipio == idMunicipio)
                .Select(d => new
                {
                    d.IdItinerarioNavigation.IdUsuario,
                    d.FechaVisita,
                    d.DiaNumero,
                    Inicio = d.IdItinerarioNavigation.FechaInicio,
                    Fin = d.IdItinerarioNavigation.FechaFin
                })
                .ToListAsync(ct);

            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var inicioAlerta = DateOnly.FromDateTime(alerta.FechaInicio);
            var finAlerta = DateOnly.FromDateTime(alerta.FechaFin);

            foreach (var p in paradas)
            {
                // Fecha de la parada: la guardada, o la que sale del día del recorrido
                var fecha = p.FechaVisita ?? p.Inicio?.AddDays(Math.Max(p.DiaNumero, 1) - 1);

                bool afecta = fecha.HasValue
                    ? fecha.Value >= hoy && fecha.Value >= inicioAlerta && fecha.Value <= finAlerta
                    // Sin fecha por parada: basta con que el viaje se cruce con la alerta
                    : p.Inicio.HasValue && p.Fin.HasValue
                      && p.Fin.Value >= hoy && p.Inicio.Value <= finAlerta && p.Fin.Value >= inicioAlerta;

                if (afecta) ids.Add(p.IdUsuario);
            }

            // 3. Comercios aprobados del municipio (y el dueño del negocio nombrado)
            ids.UnionWith(await _context.Negocios.AsNoTracking()
                .Where(n => n.IdUsuario != null
                    && ((n.IdMunicipio == idMunicipio && n.Estado == EstadosNegocio.Aprobado) || n.IdNegocio == idNegocio))
                .Select(n => n.IdUsuario!.Value)
                .ToListAsync(ct));

            if (ids.Count == 0) return ids;

            // Solo cuentas activas
            var activos = await _context.Usuarios.AsNoTracking()
                .Where(u => ids.Contains(u.IdUsuario) && u.Activo)
                .Select(u => u.IdUsuario)
                .ToListAsync(ct);

            return activos.ToHashSet();
        }

        private static string ArmarMensaje(AlertaDto a, bool esActualizacion)
        {
            var prefijo = esActualizacion ? "Aviso actualizado" : "Aviso";
            var lugar = a.NombreNegocio is null ? a.NombreMunicipio : $"{a.NombreNegocio} ({a.NombreMunicipio})";
            return $"{prefijo} en {lugar} · {TiposAlerta.Nombre(a.Tipo)}: {a.Titulo} — {RangoFechas(a.FechaInicio, a.FechaFin)}";
        }

        private static string RangoFechas(DateTime inicio, DateTime fin) =>
            inicio.Date == fin.Date
                ? $"{inicio.ToString("d MMM", Es)}, {inicio:HH:mm}–{fin:HH:mm}"
                : $"{inicio.ToString("d MMM", Es)} al {fin.ToString("d MMM", Es)}";

        // ═════════════ AYUDANTES ═════════════

        /// <summary>Valida tipo, severidad y fechas (lo que no depende del rol).</summary>
        private static string? ValidarContenido(GuardarAlertaDto dto, DateTime ahora, bool esNueva)
        {
            if (!TiposAlerta.Todos.Contains(dto.Tipo)) return "El tipo de alerta no es válido.";
            if (!SeveridadesAlerta.Todas.Contains(dto.Severidad)) return "La severidad no es válida.";
            if (string.IsNullOrWhiteSpace(dto.Titulo) || string.IsNullOrWhiteSpace(dto.Descripcion))
                return "Escribe el título y la descripción.";

            if (dto.FechaInicio == default || dto.FechaFin == default) return "Indica cuándo empieza y cuándo termina.";
            if (dto.FechaFin <= dto.FechaInicio) return "La fecha de fin debe ser posterior a la de inicio.";
            if (dto.FechaFin <= ahora) return "La alerta terminaría en el pasado: revisa la fecha de fin.";
            if ((dto.FechaFin - dto.FechaInicio).TotalDays > MaxDiasDuracion)
                return $"Una alerta puede durar máximo {MaxDiasDuracion} días.";
            if (esNueva && dto.FechaInicio > ahora.AddDays(MaxDiasAnticipacion))
                return "Solo se pueden publicar alertas hasta con un año de anticipación.";

            return null;
        }

        /// <summary>
        /// Comprueba que el usuario pueda publicar sobre ese municipio/negocio y devuelve el municipio final.
        /// </summary>
        private async Task<(ResultadoAlerta resultado, string? error, int? idMunicipio)> ResolverLugarAsync(
            int idUsuario, string rol, GuardarAlertaDto dto, CancellationToken ct)
        {
            Negocio? negocio = null;
            if (dto.IdNegocio.HasValue)
            {
                negocio = await _context.Negocios.AsNoTracking()
                    .FirstOrDefaultAsync(n => n.IdNegocio == dto.IdNegocio.Value, ct);
                if (negocio is null) return (ResultadoAlerta.Invalida, "El establecimiento no existe.", null);
            }

            int? idMunicipio;

            switch (rol)
            {
                case Roles.AdminGeneral:
                    idMunicipio = dto.IdMunicipio ?? negocio?.IdMunicipio;
                    if (idMunicipio is null) return (ResultadoAlerta.Invalida, "Elige el municipio.", null);
                    break;

                case Roles.AdminMunicipio:
                    idMunicipio = await MunicipioAsignadoAsync(idUsuario, ct);
                    if (idMunicipio is null)
                        return (ResultadoAlerta.SinPermiso, "Tu cuenta no tiene un municipio asignado.", null);
                    if (dto.IdMunicipio.HasValue && dto.IdMunicipio != idMunicipio)
                        return (ResultadoAlerta.SinPermiso, "Solo puedes publicar alertas de tu municipio.", null);
                    break;

                case Roles.AdminEstablecimiento:
                    if (negocio is null)
                        return (ResultadoAlerta.Invalida, "Elige el establecimiento al que se refiere el aviso.", null);
                    if (negocio.IdUsuario != idUsuario)
                        return (ResultadoAlerta.SinPermiso, "Solo puedes publicar avisos de tus establecimientos.", null);
                    if (negocio.Estado != EstadosNegocio.Aprobado)
                        return (ResultadoAlerta.Invalida, "El establecimiento debe estar aprobado para publicar avisos.", null);
                    if (!TiposAlerta.DeComercio.Contains(dto.Tipo))
                        return (ResultadoAlerta.SinPermiso, "Un establecimiento solo puede avisar cierres o cambios de horario.", null);
                    idMunicipio = negocio.IdMunicipio;
                    break;

                default:
                    return (ResultadoAlerta.SinPermiso, "No tienes permiso para publicar alertas.", null);
            }

            if (negocio is not null && negocio.IdMunicipio != idMunicipio)
                return (ResultadoAlerta.Invalida, "El establecimiento no pertenece a ese municipio.", null);

            var municipioActivo = await _context.Municipios.AnyAsync(m => m.IdMunicipio == idMunicipio && m.Activo, ct);
            if (!municipioActivo) return (ResultadoAlerta.Invalida, "El municipio no existe o está inactivo.", null);

            return (ResultadoAlerta.Ok, null, idMunicipio);
        }

        /// <summary>Carga la alerta si el usuario puede editarla o finalizarla.</summary>
        private async Task<(AlertaMunicipal? alerta, ResultadoAlerta resultado, string? error)> CargarConPermisoAsync(
            int idAlerta, int idUsuario, string rol, CancellationToken ct)
        {
            var alerta = await _context.AlertasMunicipales
                .Include(a => a.IdNegocioNavigation)
                .FirstOrDefaultAsync(a => a.IdAlertaMunicipal == idAlerta, ct);

            if (alerta is null) return (null, ResultadoAlerta.NoEncontrada, "La alerta no existe.");

            var puede = rol switch
            {
                Roles.AdminGeneral => true,
                Roles.AdminMunicipio => await MunicipioAsignadoAsync(idUsuario, ct) == alerta.IdMunicipio,
                // El comercio solo toca las que él publicó
                Roles.AdminEstablecimiento => alerta.IdUsuarioCreador == idUsuario,
                _ => false
            };

            return puede
                ? (alerta, ResultadoAlerta.Ok, null)
                : (null, ResultadoAlerta.SinPermiso, "No tienes permiso sobre esta alerta.");
        }

        private Task<int?> MunicipioAsignadoAsync(int idUsuario, CancellationToken ct) =>
            _context.Usuarios
                .Where(u => u.IdUsuario == idUsuario)
                .Select(u => u.MunicipioAsignadoId)
                .FirstOrDefaultAsync(ct);

        private async Task<AlertaDto?> ObtenerDtoAsync(int idAlerta, CancellationToken ct)
        {
            var dto = await Proyectar(_context.AlertasMunicipales.AsNoTracking().Where(a => a.IdAlertaMunicipal == idAlerta), incluirCreador: true)
                .FirstOrDefaultAsync(ct);
            if (dto is not null) dto.Estado = CalcularEstado(dto, DateTime.Now);
            return dto;
        }

        private static IQueryable<AlertaDto> Proyectar(IQueryable<AlertaMunicipal> query, bool incluirCreador) =>
            query.Select(a => new AlertaDto
            {
                IdAlerta = a.IdAlertaMunicipal,
                IdMunicipio = a.IdMunicipio,
                NombreMunicipio = a.IdMunicipioNavigation.Nombre,
                IdNegocio = a.IdNegocio,
                NombreNegocio = a.IdNegocioNavigation != null ? a.IdNegocioNavigation.Nombre : null,
                Tipo = a.Tipo,
                Severidad = a.Severidad,
                Titulo = a.Titulo,
                Descripcion = a.Descripcion,
                FechaInicio = a.FechaInicio,
                FechaFin = a.FechaFin,
                Activa = a.Activa,
                FechaCreacion = a.FechaCreacion,
                IdUsuarioCreador = a.IdUsuarioCreador,
                NombreCreador = incluirCreador
                    ? a.IdUsuarioCreadorNavigation.Nombre + " " + a.IdUsuarioCreadorNavigation.Apellido
                    : null
            });

        private static string CalcularEstado(AlertaDto a, DateTime ahora) =>
            !a.Activa ? "Finalizada"
            : a.FechaFin <= ahora ? "Vencida"
            : a.FechaInicio > ahora ? "Proxima"
            : "EnCurso";

        private static int PesoSeveridad(string severidad) => severidad switch
        {
            SeveridadesAlerta.Importante => 0,
            SeveridadesAlerta.Precaucion => 1,
            _ => 2
        };

        /// <summary>En curso primero, luego próximas, luego historial; dentro de cada grupo, lo más grave primero.</summary>
        private static List<AlertaDto> Ordenar(List<AlertaDto> alertas, DateTime ahora)
        {
            foreach (var a in alertas) a.Estado = CalcularEstado(a, ahora);

            static int PesoEstado(string e) => e switch { "EnCurso" => 0, "Proxima" => 1, "Vencida" => 2, _ => 3 };

            return alertas
                .OrderBy(a => PesoEstado(a.Estado))
                .ThenBy(a => PesoSeveridad(a.Severidad))
                .ThenBy(a => a.Estado == "Proxima" ? a.FechaInicio : DateTime.MinValue)
                .ThenByDescending(a => a.FechaCreacion)
                .ToList();
        }
    }
}
