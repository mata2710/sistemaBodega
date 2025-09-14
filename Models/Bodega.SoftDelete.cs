using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaBodega.Models
{
    public partial class Bodega
    {
        public bool IsActive { get; set; } = true;
        public DateTime? DeactivatedAt { get; set; }

        [StringLength(100)]
        public string? DeactivatedBy { get; set; }
    }
}
