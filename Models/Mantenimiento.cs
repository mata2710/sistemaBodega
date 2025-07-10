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
        public DateTime FechaMantenimiento { get; set; }

        [Display(Name = "Tipo de Mantenimiento")]
        public string TipoMantenimiento { get; set; } = null!;

        public decimal Costo { get; set; }

        [Display(Name = "Empresa Responsable")]
        public string EmpresaResponsable { get; set; } = null!;

        public int IdBodega { get; set; }

        [ForeignKey("IdBodega")]
        public virtual Bodega? Bodega { get; set; }
    }
}

