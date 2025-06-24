using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SistemaBodega.Models;

namespace SistemaBodega.Data;

public partial class SistemaBodegaContext : DbContext
{
    public SistemaBodegaContext()
    {
    }

    public SistemaBodegaContext(DbContextOptions<SistemaBodegaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bodega> Bodegas { get; set; }

    public virtual DbSet<Contrato> Contratos { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=DJ\\SQLEXPRESS;Database=SistemaBodegaDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bodega>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bodega__3214EC075A9BA732");

            entity.ToTable("Bodega");

            entity.Property(e => e.Complejo).HasMaxLength(100);
            entity.Property(e => e.Estado).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Ubicacion).HasMaxLength(150);
        });

        modelBuilder.Entity<Contrato>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Contrato__3214EC07A4F92959");

            entity.Property(e => e.NombreCliente).HasMaxLength(100);
            entity.Property(e => e.TotalApagar)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("TotalAPagar");

            entity.HasOne(d => d.IdBodegaNavigation).WithMany(p => p.Contratos)
                .HasForeignKey(d => d.IdBodega)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Contratos__IdBod__3C69FB99");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Contratos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Contratos__IdUsu__3B75D760");
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Facturas__3214EC0799C96AEF");

            entity.Property(e => e.Estado).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.PrecioTotal).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdContratoNavigation).WithMany(p => p.Facturas)
                .HasForeignKey(d => d.IdContrato)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Facturas__IdCont__3F466844");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario__3214EC07D5FC1902");

            entity.ToTable("Usuario");

            entity.HasIndex(e => e.Correo, "UQ__Usuario__60695A19E794BCD1").IsUnique();

            entity.Property(e => e.Contrasena).HasMaxLength(100);
            entity.Property(e => e.Correo).HasMaxLength(100);
            entity.Property(e => e.NombreCompleto).HasMaxLength(100);
            entity.Property(e => e.Rol).HasMaxLength(50);

            // Token para recuperación de contraseña
            entity.Property(e => e.TokenRecuperacion)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
