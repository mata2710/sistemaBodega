using System;
using System.Collections.Generic;

namespace SistemaBodega.Models;

public partial class Bodega
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Ubicacion { get; set; } = null!;

    public string? Complejo { get; set; }

    public string? Estado { get; set; }

    public decimal? Precio { get; set; }

    public virtual ICollection<Alquilere> Alquileres { get; set; } = new List<Alquilere>();

    public virtual ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();

    public virtual ICollection<Mantenimiento> Mantenimientos { get; set; } = new List<Mantenimiento>();
}
