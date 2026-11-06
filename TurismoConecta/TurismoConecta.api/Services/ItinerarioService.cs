// Services/ItinerarioService.cs
using Microsoft.EntityFrameworkCore;
using TurismoConecta.api.Data;
using TurismoConecta.api.DTOs.Itinerarios;
using TurismoConecta.api.Models;
using TurismoConecta.api.Services.Interfaces;

namespace TurismoConecta.api.Services
{
    public class ItinerarioService : IItinerarioService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ItinerarioService> _logger;
        private readonly IRecomendacionService _recomendaciones;

        public ItinerarioService(AppDbContext db, ILogger<ItinerarioService> logger, IRecomendacionService recomendaciones)
        {
            _db = db;
            _logger = logger;
            _recomendaciones = recomendaciones;
        }

        /// <summary>
        /// Km y minutos estimados desde la parada anterior (mismo cálculo que el resumen de Crear itinerario).
        /// Si la estimación falla, el itinerario se guarda igual, solo sin tramos.
        /// </summary>
        private async Task AsignarTramosAsync(ICollection<ItinerarioDetalle> paradas)
        {
            var enOrden = paradas.OrderBy(p => p.DiaNumero).ThenBy(p => p.Orden).ToList();

            try
            {
                var ruta = await _recomendaciones.EstimarRutaAsync(enOrden.Select(p => p.IdMunicipio).ToList(), optimizar: false);

                // Un tramo por parada y en el mismo orden (los municipios ya se validaron como activos)
                foreach (var (parada, tramo) in enOrden.Zip(ruta.Tramos))
                {
                    if (parada.IdMunicipio != tramo.IdMunicipio) continue;
                    parada.DistanciaKm = tramo.KmDesdeAnterior;
                    parada.TiempoEstimadoMin = tramo.MinutosDesdeAnterior;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudieron estimar los tramos del itinerario");
            }
        }

        /// <summary>Itinerarios guardados antes de calcular tramos: se completan al leerlos (sin tocar la BD).</summary>
        private async Task<ItinerarioResponseDto> CompletarTramosAsync(ItinerarioResponseDto dto)
        {
            if (dto.Paradas.Count < 2 || dto.Paradas.Skip(1).All(p => p.DistanciaKm is not null)) return dto;

            try
            {
                var ruta = await _recomendaciones.EstimarRutaAsync(dto.Paradas.Select(p => p.IdMunicipio).ToList(), optimizar: false);
                foreach (var (parada, tramo) in dto.Paradas.Zip(ruta.Tramos))
                {
                    if (parada.IdMunicipio != tramo.IdMunicipio || parada.DistanciaKm is not null) continue;
                    parada.DistanciaKm = tramo.KmDesdeAnterior;
                    parada.TiempoEstimadoMin = tramo.MinutosDesdeAnterior;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudieron completar los tramos del itinerario {IdItinerario}", dto.IdItinerario);
            }

            return dto;
        }

        public async Task<List<ItinerarioListadoDto>> ListarPorUsuarioAsync(int idUsuario)
        {
            return await _db.Itinerarios
                .Where(i => i.IdUsuario == idUsuario)
                .OrderByDescending(i => i.FechaCreacion)
                .Select(i => new ItinerarioListadoDto
                {
                    IdItinerario = i.IdItinerario,
                    Nombre = i.Nombre,
                    FechaInicio = i.FechaInicio,
                    FechaFin = i.FechaFin,
                    CantidadParadas = i.ItinerarioDetalles.Count
                })
                .ToListAsync();
        }

        public async Task<ItinerarioResponseDto?> ObtenerDetalleAsync(int idItinerario, int idUsuario)
        {
            var itinerario = await _db.Itinerarios
                .Include(i => i.ItinerarioDetalles)
                    .ThenInclude(d => d.IdMunicipioNavigation)
                .FirstOrDefaultAsync(i => i.IdItinerario == idItinerario && i.IdUsuario == idUsuario);

            return itinerario is null ? null : await CompletarTramosAsync(MapearAResponse(itinerario));
        }

        public async Task<ItinerarioResponseDto?> ObtenerPorCodigoCompartirAsync(Guid codigo)
        {
            var itinerario = await _db.Itinerarios
                .Include(i => i.ItinerarioDetalles)
                    .ThenInclude(d => d.IdMunicipioNavigation)
                .FirstOrDefaultAsync(i => i.CodigoCompartir == codigo && i.Compartido);

            return itinerario is null ? null : await CompletarTramosAsync(MapearAResponse(itinerario));
        }

        public async Task<(bool exito, string? error, ItinerarioResponseDto? itinerario)> CrearAsync(int idUsuario, ItinerarioCreateDto dto)
        {
            try
            {
                var idsMunicipios = dto.Paradas.Select(p => p.IdMunicipio).Distinct().ToList();
                var municipiosExistentes = await _db.Municipios
                    .Where(m => idsMunicipios.Contains(m.IdMunicipio) && m.Activo)
                    .Select(m => m.IdMunicipio)
                    .ToListAsync();

                if (municipiosExistentes.Count != idsMunicipios.Count)
                    return (false, "Uno o más municipios seleccionados no existen o no están activos.", null);

                var nuevoItinerario = new Itinerario
                {
                    IdUsuario = idUsuario,
                    Nombre = dto.Nombre,
                    FechaInicio = dto.FechaInicio,
                    FechaFin = dto.FechaFin,
                    Observaciones = dto.Observaciones,
                    Compartido = false,
                    CodigoCompartir = Guid.NewGuid(),
                    FechaCreacion = DateTime.UtcNow
                };

                foreach (var parada in dto.Paradas)
                {
                    nuevoItinerario.ItinerarioDetalles.Add(new ItinerarioDetalle
                    {
                        IdMunicipio = parada.IdMunicipio,
                        DiaNumero = parada.DiaNumero,
                        Orden = parada.Orden,
                        FechaVisita = parada.FechaVisita
                    });
                }

                await AsignarTramosAsync(nuevoItinerario.ItinerarioDetalles);

                _db.Itinerarios.Add(nuevoItinerario);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Itinerario {IdItinerario} creado por usuario {IdUsuario} con {Cantidad} paradas",
                    nuevoItinerario.IdItinerario, idUsuario, dto.Paradas.Count);

                var completo = await _db.Itinerarios
                    .AsNoTracking()
                    .Include(i => i.ItinerarioDetalles)
                        .ThenInclude(d => d.IdMunicipioNavigation)
                    .FirstAsync(i => i.IdItinerario == nuevoItinerario.IdItinerario);

                return (true, null, MapearAResponse(completo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear itinerario para usuario {IdUsuario}", idUsuario);
                return (false, $"Error al guardar el itinerario: {ex.Message}", null);
            }
        }

        public async Task<(bool exito, string? error)> EliminarAsync(int idItinerario, int idUsuario)
        {
            var itinerario = await _db.Itinerarios
                .FirstOrDefaultAsync(i => i.IdItinerario == idItinerario);

            if (itinerario is null)
                return (false, "El itinerario no existe.");

            if (itinerario.IdUsuario != idUsuario)
                return (false, "No tienes permiso para eliminar este itinerario.");

            _db.Itinerarios.Remove(itinerario);
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool exito, string? error, ItinerarioResponseDto? itinerario)> ActualizarAsync(int idItinerario, int idUsuario, ItinerarioUpdateDto dto)
        {
            // 1. Buscamos el itinerario y verificamos que exista y sea del usuario
            var itinerario = await _db.Itinerarios
                .Include(i => i.ItinerarioDetalles)
                .FirstOrDefaultAsync(i => i.IdItinerario == idItinerario);

            if (itinerario is null)
                return (false, "El itinerario no existe.", null);

            if (itinerario.IdUsuario != idUsuario)
                return (false, "No tienes permiso para editar este itinerario.", null);

            // 2. Validamos que todos los municipios de las nuevas paradas existan y estén activos
            var idsMunicipios = dto.Paradas.Select(p => p.IdMunicipio).Distinct().ToList();
            var municipiosExistentes = await _db.Municipios
                .Where(m => idsMunicipios.Contains(m.IdMunicipio) && m.Activo)
                .Select(m => m.IdMunicipio)
                .ToListAsync();

            if (municipiosExistentes.Count != idsMunicipios.Count)
                return (false, "Uno o más municipios no existen o no están activos.", null);

            // 3. Actualizamos los campos del itinerario
            itinerario.Nombre = dto.Nombre;
            itinerario.FechaInicio = dto.FechaInicio;
            itinerario.FechaFin = dto.FechaFin;
            itinerario.Observaciones = dto.Observaciones;

            // 4. Borramos las paradas antiguas y las reemplazamos con las nuevas
            //    Esta estrategia se llama "delete-and-reinsert" — es la más simple
            //    cuando el usuario puede reordenar libremente las paradas
            _db.ItinerarioDetalles.RemoveRange(itinerario.ItinerarioDetalles);

            foreach (var parada in dto.Paradas)
            {
                itinerario.ItinerarioDetalles.Add(new ItinerarioDetalle
                {
                    IdMunicipio = parada.IdMunicipio,
                    DiaNumero = parada.DiaNumero,
                    Orden = parada.Orden,
                    FechaVisita = parada.FechaVisita
                });
            }

            await AsignarTramosAsync(itinerario.ItinerarioDetalles.Where(d => d.IdItinerarioDetalle == 0).ToList());

            await _db.SaveChangesAsync();

            // 5. Recargamos con navegaciones para poder mapear el DTO de respuesta
            var completo = await _db.Itinerarios
                .AsNoTracking()
                .Include(i => i.ItinerarioDetalles)
                    .ThenInclude(d => d.IdMunicipioNavigation)
                .FirstAsync(i => i.IdItinerario == idItinerario);

            return (true, null, MapearAResponse(completo));
        }

        public async Task<(bool exito, string? error, bool compartido, Guid codigoCompartir)> ToggleCompartirAsync(int idItinerario, int idUsuario)
        {
            var itinerario = await _db.Itinerarios
                .FirstOrDefaultAsync(i => i.IdItinerario == idItinerario);

            if (itinerario is null)
                return (false, "El itinerario no existe.", false, Guid.Empty);

            if (itinerario.IdUsuario != idUsuario)
                return (false, "No tienes permiso para compartir este itinerario.", false, Guid.Empty);

            // Invierte el estado: si estaba compartido lo priva, si estaba privado lo comparte
            itinerario.Compartido = !itinerario.Compartido;
            await _db.SaveChangesAsync();

            return (true, null, itinerario.Compartido, itinerario.CodigoCompartir);
        }




        private static ItinerarioResponseDto MapearAResponse(Itinerario i)
        {
            return new ItinerarioResponseDto
            {
                IdItinerario = i.IdItinerario,
                Nombre = i.Nombre,
                FechaInicio = i.FechaInicio,
                FechaFin = i.FechaFin,
                Compartido = i.Compartido,
                CodigoCompartir = i.CodigoCompartir,
                Observaciones = i.Observaciones,
                FechaCreacion = i.FechaCreacion,
                Paradas = i.ItinerarioDetalles
                    .OrderBy(d => d.DiaNumero).ThenBy(d => d.Orden)
                    .Select(d => new ParadaResponseDto
                    {
                        IdItinerarioDetalle = d.IdItinerarioDetalle,
                        IdMunicipio = d.IdMunicipio,
                        NombreMunicipio = d.IdMunicipioNavigation?.Nombre ?? "Municipio",
                        ImagenUrl = d.IdMunicipioNavigation?.ImagenUrl,
                        DiaNumero = d.DiaNumero,
                        Orden = d.Orden,
                        FechaVisita = d.FechaVisita,
                        DistanciaKm = d.DistanciaKm,
                        TiempoEstimadoMin = d.TiempoEstimadoMin,
                        Latitud = (double?)d.IdMunicipioNavigation?.Latitud,
                        Longitud = (double?)d.IdMunicipioNavigation?.Longitud
                    }).ToList()
            };
        }
    }
}