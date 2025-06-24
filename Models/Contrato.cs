using System;
using System.Collections.Generic;

namespace SistemaBodega.Models;

public partial class Contrato
{
    public int Id { get; set; }

    public string NombreCliente { get; set; } = null!;

    public int IdUsuario { get; set; }

    public int IdBodega { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public decimal TotalApagar { get; set; }

    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    public virtual Bodega IdBodegaNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
