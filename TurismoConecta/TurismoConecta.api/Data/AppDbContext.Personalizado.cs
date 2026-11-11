using Microsoft.EntityFrameworkCore;
using TurismoConecta.api.Models;

namespace TurismoConecta.api.Data;

/// <summary>
/// Lo propio del modelo, fuera del AppDbContext generado
/// (regla 3 de Database/README.md: lo propio va en partials):
///   - tablas nuevas (AlertaMunicipal, EventoVisibilidad),
///   - índices únicos creados por scripts (2026-11-10_002_limpieza_duplicados).
/// </summary>
public partial class AppDbContext
{
    public virtual DbSet<AlertaMunicipal> AlertasMunicipales { get; set; }

    public virtual DbSet<EventoVisibilidad> EventosVisibilidad { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Tabla del script 2026-11-10_003_evento_visibilidad (estadísticas y reportes)
        modelBuilder.Entity<EventoVisibilidad>(entity =>
        {
            entity.HasKey(e => e.IdEventoVisibilidad);

            entity.ToTable("EventoVisibilidad");

            entity.HasIndex(e => new { e.IdNegocio, e.Fecha }, "IX_EventoVisibilidad_Negocio_Fecha");
            entity.HasIndex(e => new { e.IdMunicipio, e.Fecha }, "IX_EventoVisibilidad_Municipio_Fecha");
            entity.HasIndex(e => new { e.IdVisitante, e.Fecha }, "IX_EventoVisibilidad_Visitante_Fecha");

            entity.Property(e => e.IdEventoVisibilidad).HasColumnName("idEventoVisibilidad");
            entity.Property(e => e.Tipo).HasMaxLength(20).HasColumnName("tipo");
            entity.Property(e => e.IdMunicipio).HasColumnName("idMunicipio");
            entity.Property(e => e.IdNegocio).HasColumnName("idNegocio");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.IdVisitante).HasColumnName("idVisitante");
            // Sin HasDefaultValueSql: el servicio siempre pone la fecha (así EF no hace un SELECT extra)
            entity.Property(e => e.Fecha).HasPrecision(0).HasColumnName("fecha");

            entity.HasOne(d => d.IdMunicipioNavigation).WithMany()
                .HasForeignKey(d => d.IdMunicipio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventoVisibilidad_Municipio");

            entity.HasOne(d => d.IdNegocioNavigation).WithMany()
                .HasForeignKey(d => d.IdNegocio)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_EventoVisibilidad_Negocio");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany()
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_EventoVisibilidad_Usuario");
        });

        // Índice único del script 2026-11-10_002 (el de MunicipioEtiqueta ya está en el
        // AppDbContext generado). EF necesita conocerlos para ordenar "borrar antes de insertar".
        modelBuilder.Entity<NegocioEtiqueta>()
            .HasIndex(e => new { e.IdNegocio, e.IdEtiqueta }, "UQ_NegocioEtiqueta_Negocio_Etiqueta")
            .IsUnique();

        modelBuilder.Entity<AlertaMunicipal>(entity =>
        {
            entity.HasKey(e => e.IdAlertaMunicipal);

            entity.ToTable("AlertaMunicipal");

            entity.HasIndex(e => new { e.IdMunicipio, e.FechaFin }, "IX_AlertaMunicipal_Municipio_FechaFin");

            entity.Property(e => e.IdAlertaMunicipal).HasColumnName("idAlertaMunicipal");
            entity.Property(e => e.IdMunicipio).HasColumnName("idMunicipio");
            entity.Property(e => e.IdNegocio).HasColumnName("idNegocio");
            entity.Property(e => e.Tipo).HasMaxLength(20).HasColumnName("tipo");
            entity.Property(e => e.Severidad).HasMaxLength(20).HasColumnName("severidad");
            entity.Property(e => e.Titulo).HasMaxLength(120).HasColumnName("titulo");
            entity.Property(e => e.Descripcion).HasMaxLength(1000).HasColumnName("descripcion");
            entity.Property(e => e.FechaInicio).HasColumnName("fechaInicio");
            entity.Property(e => e.FechaFin).HasColumnName("fechaFin");
            // Sin HasDefaultValue: el servicio siempre la crea con Activa = true
            entity.Property(e => e.Activa).HasColumnName("activa");
            entity.Property(e => e.IdUsuarioCreador).HasColumnName("idUsuarioCreador");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fechaCreacion");

            entity.HasOne(d => d.IdMunicipioNavigation).WithMany()
                .HasForeignKey(d => d.IdMunicipio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AlertaMunicipal_Municipio");

            entity.HasOne(d => d.IdNegocioNavigation).WithMany()
                .HasForeignKey(d => d.IdNegocio)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_AlertaMunicipal_Negocio");

            entity.HasOne(d => d.IdUsuarioCreadorNavigation).WithMany()
                .HasForeignKey(d => d.IdUsuarioCreador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AlertaMunicipal_Usuario");
        });
    }
}
