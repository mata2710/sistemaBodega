using System.ComponentModel.DataAnnotations;

namespace SistemaBodega.Models
{
    public class CarruselImagen
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Título opcional")]
        public string? Titulo { get; set; }

        [Required]
        [Display(Name = "Ruta de la Imagen")]
        public string RutaImagen { get; set; }
    }
}
