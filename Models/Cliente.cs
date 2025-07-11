using System.ComponentModel.DataAnnotations;

namespace SistemaBodega.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        // Puedes agregar otros campos si los tienes

        public string? Telefono { get; set; }

        public string? Email { get; set; }
    }
}


