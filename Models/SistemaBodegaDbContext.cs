using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SistemaBodega.Models;

public partial class SistemaBodegaDbContext : DbContext
{
    public SistemaBodegaDbContext()
    {
    }

    public SistemaBodegaDbContext(DbContextOptions<SistemaBodegaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alquilere> Alquileres { get; set; }

    public virtual DbSet<Bodega> Bodegas { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Contrato> Contratos { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    public virtual DbSet<Mantenimiento> Mantenimientos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DJ\\SQLEXPRESS;Database=SistemaBodegaDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alquilere>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Alquiler__3214EC077E7AFFD8");

            entity.Property(e => e.Activo).HasDefaultValue(true);

            entity.HasOne(d => d.Bodega).WithMany(p => p.Alquileres)
                .HasForeignKey(d => d.BodegaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Alquilere__Bodeg__31EC6D26");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Alquileres)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Alquilere__Clien__32E0915F");
        });

        modelBuilder.Entity<Bodega>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bodega__3214EC07A85C1708");

            entity.ToTable("Bodega");

            entity.Property(e => e.Complejo).HasMaxLength(100);
            entity.Property(e => e.Estado).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Ubicacion).HasMaxLength(150);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Clientes__3214EC077FBCD9D4");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Identificacion).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
        });

        modelBuilder.Entity<Contrato>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Contrato__3214EC07BD6BAEA3");

            entity.Property(e => e.NombreCliente).HasMaxLength(100);
            entity.Property(e => e.TotalApagar)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("TotalAPagar");

            entity.HasOne(d => d.IdBodegaNavigation).WithMany(p => p.Contratos)
                .HasForeignKey(d => d.IdBodega)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Contratos__IdBod__33D4B598");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Contratos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Contratos__IdUsu__35BCFE0A");
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Facturas__3214EC074F1A8A67");

            entity.Property(e => e.Estado).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.PrecioTotal).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdContratoNavigation).WithMany(p => p.Facturas)
                .HasForeignKey(d => d.IdContrato)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Facturas__IdCont__37A5467C");
        });

        modelBuilder.Entity<Mantenimiento>(entity =>
        {
            entity.ToTable("Mantenimientos");

            entity.Property(e => e.TipoMantenimiento).HasMaxLength(50);
            entity.Property(e => e.EmpresaResponsable).HasMaxLength(100);
            entity.Property(e => e.Costo).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ComentariosAdministracion).HasMaxLength(1000); // opcional si querés limitarlo

            entity.HasOne(d => d.Bodega)
                .WithMany(p => p.Mantenimientos)
                .HasForeignKey(d => d.IdBodega)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Mantenimientos_Bodegas"); // ← Podés dejar esto si lo deseás nombrado así
        });


        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario__3214EC077A0A4FC3");

            entity.ToTable("Usuario");

            entity.HasIndex(e => e.Correo, "UQ__Usuario__60695A1929042438").IsUnique();

            entity.Property(e => e.Contrasena).HasMaxLength(100);
            entity.Property(e => e.Correo).HasMaxLength(100);
            entity.Property(e => e.NombreCompleto).HasMaxLength(100);
            entity.Property(e => e.Rol).HasMaxLength(50);
            entity.Property(e => e.TokenRecuperacion).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
