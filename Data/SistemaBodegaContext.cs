using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SistemaBodega.Models;

namespace SistemaBodega.Data
{
    public partial class SistemaBodegaContext : DbContext
    {
        public SistemaBodegaContext(DbContextOptions<SistemaBodegaContext> options)
            : base(options) { }

        public virtual DbSet<Bodega> Bodegas { get; set; }
        public virtual DbSet<Contrato> Contratos { get; set; }
        public virtual DbSet<Factura> Facturas { get; set; }
        public virtual DbSet<Usuario> Usuarios { get; set; }
        public virtual DbSet<Cliente> Clientes { get; set; }
        public virtual DbSet<Mantenimiento> Mantenimientos { get; set; }
        public virtual DbSet<Alquiler> Alquileres { get; set; }
        public virtual DbSet<CarruselImagen> CarruselImagenes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ======================
            // BODEGA
            // ======================
            modelBuilder.Entity<Bodega>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("Bodega");

                entity.Property(e => e.Nombre).HasMaxLength(100);
                entity.Property(e => e.Ubicacion).HasMaxLength(150);
                entity.Property(e => e.Complejo).HasMaxLength(100);
                entity.Property(e => e.Estado).HasMaxLength(50);

                entity.Property(e => e.AreaM2).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.PrecioAlquilerPorM2).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.Precio)
                      .HasColumnType("decimal(10, 2)")
                      .HasComputedColumnSql(
                          "CONVERT([decimal](10,2), ISNULL(CAST([AreaM2] AS [decimal](18,4)),(0)) * ISNULL(CAST([PrecioAlquilerPorM2] AS [decimal](18,4)),(0)))",
                          stored: true
                      );
            });

            // ======================
            // CLIENTE
            // ======================
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Clientes");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Identificacion).HasMaxLength(50).IsRequired();

                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.TelefonoSecundario).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);

                entity.Property(e => e.RepresentanteLegal).HasMaxLength(150);
                entity.Property(e => e.Actividad).HasMaxLength(100);

                // Soft-delete por convención (partial)
            });

            // ======================
            // MANTENIMIENTOS
            // ======================
            modelBuilder.Entity<Mantenimiento>(entity =>
            {
                entity.ToTable("Mantenimientos");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FechaMantenimiento).HasColumnType("date");
                entity.Property(e => e.TipoMantenimiento).HasMaxLength(50);
                entity.Property(e => e.EmpresaResponsable).HasMaxLength(100);
                entity.Property(e => e.Costo).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.ComentariosAdministracion).HasColumnType("nvarchar(max)");

                entity.HasOne(d => d.Bodega)
                      .WithMany(p => p.Mantenimientos)
                      .HasForeignKey(d => d.IdBodega)
                      .OnDelete(DeleteBehavior.Cascade);

                // ===== CAMBIOS para soft-delete (opcionales si ya existen en tu modelo) =====
                entity.Property(e => e.IsActive)
                      .HasColumnType("bit")
                      .HasDefaultValue(true);

                entity.Property(e => e.DeactivatedAt)
                      .HasColumnType("datetime2")
                      .IsRequired(false);

                entity.Property(e => e.DeactivatedBy)
                      .HasMaxLength(150)
                      .IsUnicode(false)
                      .IsRequired(false);
            });

            // ======================
            // CONTRATOS
            // ======================
            modelBuilder.Entity<Contrato>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NombreCliente).HasMaxLength(100);
                entity.Property(e => e.TotalApagar)
                      .HasColumnType("decimal(10, 2)")
                      .HasColumnName("TotalAPagar");

                entity.HasOne(d => d.IdBodegaNavigation)
                      .WithMany(p => p.Contratos)
                      .HasForeignKey(d => d.IdBodega)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.IdUsuarioNavigation)
                      .WithMany(p => p.Contratos)
                      .HasForeignKey(d => d.IdUsuario)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // ======================
            // FACTURAS
            // ======================
            modelBuilder.Entity<Factura>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Estado).HasMaxLength(50);
                entity.Property(e => e.Nombre).HasMaxLength(100);
                entity.Property(e => e.PrecioTotal).HasColumnType("decimal(10, 2)");

                entity.HasOne(d => d.IdContratoNavigation)
                      .WithMany(p => p.Facturas)
                      .HasForeignKey(d => d.IdContrato)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // ======================
            // USUARIO
            // ======================
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("Usuario");

                entity.HasIndex(e => e.Correo).IsUnique();
                entity.Property(e => e.Contrasena).HasMaxLength(100);
                entity.Property(e => e.Correo).HasMaxLength(100);
                entity.Property(e => e.NombreCompleto).HasMaxLength(100);
                entity.Property(e => e.Rol).HasMaxLength(50);
                entity.Property(e => e.TokenRecuperacion).HasMaxLength(100).IsUnicode(false);
                // Soft-delete por convención (partial)
            });

            // ======================
            // ALQUILERES
            // ======================
            modelBuilder.Entity<Alquiler>(entity =>
            {
                entity.ToTable("Alquileres");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FechaInicio).HasColumnType("date");
                entity.Property(e => e.FechaFin).HasColumnType("date");

                entity.Property(e => e.AreaM2).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.PrecioPorM2).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.PrecioAlquiler).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.AumentoAnualPorcentaje).HasColumnType("decimal(5, 2)");
                entity.Property(e => e.Observaciones).HasColumnType("nvarchar(max)");

                entity.Property(e => e.ContratoFilePath).HasMaxLength(300);

                entity.HasOne(e => e.Bodega)
                      .WithMany(b => b.Alquileres)
                      .HasForeignKey(e => e.BodegaId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Cliente)
                      .WithMany(c => c.Alquileres)
                      .HasForeignKey(e => e.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ======================
            // GLOBAL QUERY FILTERS (Soft-delete)
            // ======================
            modelBuilder.Entity<Usuario>().HasQueryFilter(u => u.IsActive);
            modelBuilder.Entity<Cliente>().HasQueryFilter(c => c.IsActive);
            modelBuilder.Entity<Alquiler>().HasQueryFilter(a => a.Activo);
            modelBuilder.Entity<Bodega>().HasQueryFilter(b => b.IsActive);

            // 👇 NUEVO: solo mantenimientos activos por defecto
            modelBuilder.Entity<Mantenimiento>().HasQueryFilter(m => m.IsActive);

            // Si agregas IsActive a otras entidades, puedes sumar más filtros aquí

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
