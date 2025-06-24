using System;
using System.Collections.Generic;

namespace SistemaBodega.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Contrasena { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public string? TokenRecuperacion { get; set; }

    public virtual ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
}
