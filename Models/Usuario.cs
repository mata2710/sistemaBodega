using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaBodega.Models
{
    public partial class Usuario
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string NombreCompleto { get; set; } = null!;

        [Required, StringLength(100), EmailAddress]
        public string Correo { get; set; } = null!;

        [Required, StringLength(100)]
        public string Contrasena { get; set; } = null!;

        [Required, StringLength(50)]
        public string Rol { get; set; } = null!;

        [StringLength(100)]
        public string? TokenRecuperacion { get; set; }

        [StringLength(260)]
        public string? FotoFilePath { get; set; }

        [Display(Name = "Número de cédula")]
        [StringLength(50)]
        public string? Cedula { get; set; }

        [Display(Name = "Teléfono")]
        [StringLength(30)]
        public string? Telefono { get; set; }

        public virtual ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
    }
}
