using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaBodega.Models
{
    public class Mantenimiento
    {
        public int Id { get; set; }

        [Display(Name = "Fecha de Mantenimiento")]
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        [Required(ErrorMessage = "La fecha de mantenimiento es obligatoria.")]
        public DateTime FechaMantenimiento { get; set; }

        [Display(Name = "Tipo de Mantenimiento")]
        [Required(ErrorMessage = "El tipo de mantenimiento es obligatorio.")]
        [StringLength(50, ErrorMessage = "Máximo {1} caracteres.")]
        public string TipoMantenimiento { get; set; } = string.Empty;

        [Display(Name = "Costo")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 999999999.99, ErrorMessage = "El costo no puede ser negativo.")]
        public decimal Costo { get; set; }

        [Display(Name = "Empresa Responsable")]
        [Required(ErrorMessage = "La empresa responsable es obligatoria.")]
        [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
        public string EmpresaResponsable { get; set; } = string.Empty;

        [Display(Name = "Bodega")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una bodega.")]
        public int IdBodega { get; set; }

        [ForeignKey(nameof(IdBodega))]
        public Bodega? Bodega { get; set; }

        [Display(Name = "Comentarios de la Administración")]
        [DataType(DataType.MultilineText)]
        public string? ComentariosAdministracion { get; set; }

        // ===== Soft-delete =====
        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Fecha de Desactivación")]
        public DateTime? DeactivatedAt { get; set; }

        [Display(Name = "Desactivado por")]
        [StringLength(150)]
        public string? DeactivatedBy { get; set; }
    }
}
