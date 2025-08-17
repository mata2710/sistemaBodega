namespace SistemaBodega.Models.ViewModels
{
    public class MantenimientoItemVM
    {
        public string Titulo { get; set; } = "";
        public string Dia { get; set; } = "";
        public string FechaCorta { get; set; } = "";
    }

    public class ContratoPorVencerVM
    {
        public string Cliente { get; set; } = "";
        public string Bodega { get; set; } = "";
        public string VenceEl { get; set; } = "";
        public string Dias { get; set; } = "";
    }
}
