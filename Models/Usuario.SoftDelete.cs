using System;

namespace SistemaBodega.Models
{
    public partial class Usuario
    {
        public bool IsActive { get; set; } = true;
        public DateTime? DeactivatedAt { get; set; }
        public string? DeactivatedBy { get; set; }
    }
}
