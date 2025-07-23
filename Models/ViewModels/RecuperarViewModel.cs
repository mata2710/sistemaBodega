// Ruta sugerida: Models/ViewModels/RecuperarViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace SistemaBodega.Models.ViewModels
{
    public class RecuperarViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Correo { get; set; }
    }
}

