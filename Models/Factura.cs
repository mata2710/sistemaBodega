using System;
using System.Collections.Generic;

namespace SistemaBodega.Models;

public partial class Factura
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int IdContrato { get; set; }

    public string? Estado { get; set; }

    public decimal? PrecioTotal { get; set; }

    public virtual Contrato IdContratoNavigation { get; set; } = null!;
}
