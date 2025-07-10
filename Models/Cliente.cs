namespace SistemaBodega.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = null!;

        public string Identificacion { get; set; } = null!;

        public string? Telefono { get; set; }

        public string? Email { get; set; }
    }
}
