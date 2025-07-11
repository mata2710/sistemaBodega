using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaBodega.Models;

public partial class Bodega
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la bodega es obligatorio")]
    public string Nombre { get; set; }

    public string Ubicacion { get; set; } = null!;

    public string? Complejo { get; set; }

    public string? Estado { get; set; }

    public decimal? Precio { get; set; }

    public virtual ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
    public virtual ICollection<Mantenimiento> Mantenimientos { get; set; } = new List<Mantenimiento>();

}
