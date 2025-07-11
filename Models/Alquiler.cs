using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaBodega.Models
{
    public class Alquiler
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una bodega.")]
        public int BodegaId { get; set; }
        public Bodega Bodega { get; set; }

        [Required(ErrorMessage = "Debe indicar la fecha de inicio.")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "Debe indicar la fecha de fin.")]
        public DateTime FechaFin { get; set; }

        public bool RenovacionAutomatica { get; set; }
        public bool Activo { get; set; }
    }
}

