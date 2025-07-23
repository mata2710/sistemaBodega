using System;
using System.Collections.Generic;

namespace SistemaBodega.Models;

public partial class Alquilere
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public int BodegaId { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public bool RenovacionAutomatica { get; set; }

    public bool Activo { get; set; }

    public virtual Bodega Bodega { get; set; } = null!;

    public virtual Cliente Cliente { get; set; } = null!;
}
