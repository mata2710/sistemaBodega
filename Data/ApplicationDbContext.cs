using Microsoft.EntityFrameworkCore;

namespace SistemaBodega.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Aquí defines las tablas (DbSet) según tus modelos
        // Ejemplo:
        // public DbSet<Cliente> Clientes { get; set; }
    }
}

