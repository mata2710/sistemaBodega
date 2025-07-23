using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaBodega.Models
{
    public class Mantenimiento
    {
        public int Id { get; set; }

        [Display(Name = "Fecha de Mantenimiento")]
        public DateTime FechaMantenimiento { get; set; }

        [Display(Name = "Tipo de Mantenimiento")]
        public string TipoMantenimiento { get; set; }

        public decimal Costo { get; set; }

        [Display(Name = "Empresa Responsable")]
        public string EmpresaResponsable { get; set; }

        public int IdBodega { get; set; }

        public Bodega Bodega { get; set; }

        [Display(Name = "Comentarios de la Administración")]
        public string? ComentariosAdministracion { get; set; }  // <- ahora es opcional
    }
}
