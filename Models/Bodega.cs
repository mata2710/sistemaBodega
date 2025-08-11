using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaBodega.Models
{
    public partial class Bodega
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; } = null!;

        [Required, StringLength(150)]
        public string Ubicacion { get; set; } = null!;

        [StringLength(100)]
        public string? Complejo { get; set; }

        [StringLength(50)]
        public string? Estado { get; set; } // "Disponible", "Ocupada" o "Reservada"

        // Calculada por SQL (columna COMPUTED PERSISTED). EF la leerá; no se postea ni se escribe.
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Precio { get; private set; }

        // Bases del cálculo
        [Column(TypeName = "decimal(10,2)")]
        public decimal? AreaM2 { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PrecioAlquilerPorM2 { get; set; }

        // Relaciones
        public virtual ICollection<Alquiler> Alquileres { get; set; } = new List<Alquiler>();
        public virtual ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
        public virtual ICollection<Mantenimiento> Mantenimientos { get; set; } = new List<Mantenimiento>();
    }
}
