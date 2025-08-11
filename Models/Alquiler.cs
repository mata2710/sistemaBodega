using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace SistemaBodega.Models
{
    [Table("Alquileres")]
    public class Alquiler
    {
        public int Id { get; set; }

        // ===== Relaciones obligatorias =====
        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una bodega.")]
        [Display(Name = "Bodega")]
        public int BodegaId { get; set; }

        // ===== Fechas =====
        [Required, DataType(DataType.Date)]
        [Column(TypeName = "date")]
        [Display(Name = "Fecha de inicio")]
        public DateTime FechaInicio { get; set; }

        [Required, DataType(DataType.Date)]
        [Column(TypeName = "date")]
        [Display(Name = "Fecha de fin")]
        public DateTime FechaFin { get; set; }

        // ===== Económicos (opcionales) =====
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Área (m²)")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "El área no puede ser negativa.")]
        public decimal? AreaM2 { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Precio por m²")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "El precio por m² no puede ser negativo.")]
        public decimal? PrecioPorM2 { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Precio alquiler")]
        public decimal? PrecioAlquiler { get; set; }   // = AreaM2 * PrecioPorM2 (se calcula en el controlador)

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Aumento anual (%)")]
        [Range(0, 100, ErrorMessage = "El aumento anual debe estar entre 0 y 100.")]
        public decimal? AumentoAnualPorcentaje { get; set; }

        // ===== Otros =====
        [DataType(DataType.MultilineText)]
        public string? Observaciones { get; set; }

        [Display(Name = "Renovación automática")]
        public bool RenovacionAutomatica { get; set; }

        public bool Activo { get; set; }

        // ===== Archivo del contrato =====
        [StringLength(300)]
        [Display(Name = "Ruta del contrato")]
        public string? ContratoFilePath { get; set; }   // se guarda la ruta relativa, p.ej. /contratos/xxxx.pdf

        [NotMapped]
        [Display(Name = "Contrato (PDF/imagen)")]
        public IFormFile? ContratoArchivo { get; set; } // para recibir el archivo en el POST (no se mapea a BD)

        // ===== Navegaciones =====
        [ForeignKey(nameof(BodegaId))]
        public Bodega? Bodega { get; set; }

        [ForeignKey(nameof(ClienteId))]
        public Cliente? Cliente { get; set; }
    }
}
