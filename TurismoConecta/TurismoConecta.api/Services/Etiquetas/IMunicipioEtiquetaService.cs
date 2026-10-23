using TurismoConecta.api.Data;
using TurismoConecta.api.DTOs;
using TurismoConecta.api.Models;
using Microsoft.EntityFrameworkCore;
using TurismoConecta.api.DTOs.Etiquetas;

namespace TurismoConecta.api.Services.Etiquetas;

public interface IMunicipioEtiquetaService
{
    Task<MunicipioEtiquetasDto?> ObtenerPorMunicipioAsync(int idMunicipio);
    Task<(bool exito, string? error)> AsignarAsync(int idMunicipio, int idUsuarioSolicitante, AsignarEtiquetasDto dto);
}

public class MunicipioEtiquetaService : IMunicipioEtiquetaService
{
    private readonly AppDbContext _db;
    public MunicipioEtiquetaService(AppDbContext db) => _db = db;

    public async Task<MunicipioEtiquetasDto?> ObtenerPorMunicipioAsync(int idMunicipio)
    {
        var municipio = await _db.Municipios.FindAsync(idMunicipio);
        if (municipio is null) return null;

        // Include() le dice a EF Core: "cuando traigas MunicipioEtiqueta, tráete también
        // la Etiqueta relacionada en la misma consulta" (evita hacer una consulta por cada fila,
        // problema clásico llamado "N+1 queries").
        var etiquetas = await _db.MunicipioEtiqueta
            .Where(me => me.IdMunicipio == idMunicipio)
            .Include(me => me.IdEtiquetaNavigation)
            .Select(me => new EtiquetaDto
            {
                IdEtiqueta = me.IdEtiquetaNavigation.IdEtiqueta,
                Nombre = me.IdEtiquetaNavigation.Nombre,
                Descripcion = me.IdEtiquetaNavigation.Descripcion,
                Activo = me.IdEtiquetaNavigation.Activo
            })
            .ToListAsync();

        return new MunicipioEtiquetasDto
        {
            IdMunicipio = municipio.IdMunicipio,
            NombreMunicipio = municipio.Nombre,
            Etiquetas = etiquetas
        };
    }

    public async Task<(bool exito, string? error)> AsignarAsync(int idMunicipio, int idUsuarioSolicitante, AsignarEtiquetasDto dto)
    {
        //  Validar ownership: el usuario que hace la petición debe tener asignado ESTE municipio
        var usuario = await _db.Usuarios.FindAsync(idUsuarioSolicitante);
        if (usuario is null || usuario.MunicipioAsignadoId != idMunicipio)
            return (false, "No tienes permiso para editar las etiquetas de este municipio.");

        // Si llega [4, 4, 6] se toma como [4, 6]: el mismo enlace nunca se inserta dos veces
        var idsUnicos = dto.IdsEtiquetas.Distinct().ToList();

        //  Regla de negocio del backlog: mínimo 1 etiqueta
        if (idsUnicos.Count == 0)
            return (false, "El municipio debe tener al menos 1 etiqueta.");

        var existentes = await _db.Etiqueta
            .Where(e => idsUnicos.Contains(e.IdEtiqueta) && e.Activo)
            .Select(e => e.IdEtiqueta)
            .ToListAsync();

        if (existentes.Count != idsUnicos.Count)
            return (false, "Una o más etiquetas seleccionadas no existen o están inactivas.");

        var actuales = _db.MunicipioEtiqueta.Where(me => me.IdMunicipio == idMunicipio);
        _db.MunicipioEtiqueta.RemoveRange(actuales);

        
        var nuevas = idsUnicos.Select(idEtiqueta => new MunicipioEtiqueta
        {
            IdMunicipio = idMunicipio,
            IdEtiqueta = idEtiqueta
        });
        _db.MunicipioEtiqueta.AddRange(nuevas);

        await _db.SaveChangesAsync();
        return (true, null);
    }
}
