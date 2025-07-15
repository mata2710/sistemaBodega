using System.ComponentModel.DataAnnotations;

namespace SistemaBodega.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La identificación es obligatoria")]
        public string Identificacion { get; set; } = string.Empty ;


       

        public string? Telefono { get; set; }

        public string? Email { get; set; }
    }
}


