using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaBodega.Models
{
    [Table("Clientes")]
    public partial class Cliente
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = null!;

        [Required, MaxLength(50)]
        public string Identificacion { get; set; } = null!;

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [MaxLength(20)]
        public string? TelefonoSecundario { get; set; }   // nuevo (opcional)

        [MaxLength(100), EmailAddress]
        public string? Email { get; set; }

        [MaxLength(150)]
        public string? RepresentanteLegal { get; set; }   // nuevo (opcional)

        [MaxLength(100)]
        public string? Actividad { get; set; }            // nuevo (opcional)

        public virtual ICollection<Alquiler> Alquileres { get; set; } = new List<Alquiler>();
    }
}

