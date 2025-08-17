using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SistemaBodega.Data;
using SistemaBodega.Models;
using SistemaBodega.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace SistemaBodega.Controllers
{
    public class HomeController : Controller
    {
        private readonly SistemaBodegaContext _context;

        public HomeController(SistemaBodegaContext context)
        {
            _context = context;
        }

        // Función auxiliar para validar sesión
        private bool UsuarioAutenticado()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("Usuario"));
        }

        public IActionResult Index()
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Cuenta");

            // 1) Carrusel (tu modelo existente)
            var imagenesCarrusel = _context.CarruselImagenes.ToList();

            // 2) Usuario (pill superior)
            ViewBag.UsuarioNombre = HttpContext.Session.GetString("Usuario") ?? "Usuario";
            ViewBag.UsuarioFoto = Url.Content("~/img/perfil.jpg");

            // 3) Accesos rápidos (opcional)
            ViewBag.AccesosRapidos = new List<(string Titulo, string IconoBi, string Url)>
            {
                ("Nuevo cliente",  "bi-person-plus",       Url.Action("CrearCliente","Home")),
                ("Nuevo alquiler", "bi-file-earmark-plus", Url.Action("Alquileres","Home")),
                ("Bodegas",        "bi-building",          Url.Action("Bodegas","Home")),
                ("Mantenimientos", "bi-wrench-adjustable", Url.Action("Bodegas","Home")), // ajusta si tienes controlador propio
                ("Contratos",      "bi-clipboard-check",   Url.Action("Alquileres","Home")),
                ("Configuración",  "bi-gear",              Url.Action("Configuracion","Home"))
            };

            // 4) Mantenimientos próximos (tipado)
            ViewBag.MantenimientosProximos = new List<MantenimientoItemVM>
            {
                new MantenimientoItemVM { Titulo="Inspección bodega A-12",   Dia="Vie", FechaCorta="18/08/2025" },
                new MantenimientoItemVM { Titulo="Cambio de cerradura B-04", Dia="Lun", FechaCorta="21/08/2025" },
                new MantenimientoItemVM { Titulo="Fumigación sector C",      Dia="Mié", FechaCorta="23/08/2025" }
            };

            // 5) Contratos por vencer (tipado)
            ViewBag.ContratosPorVencer = new List<ContratoPorVencerVM>
            {
                new ContratoPorVencerVM { Cliente="ACME S.A.",       Bodega="D-02", VenceEl="19/08/2025", Dias="6 días"  },
                new ContratoPorVencerVM { Cliente="Logística Norte", Bodega="B-11", VenceEl="24/08/2025", Dias="11 días" },
                new ContratoPorVencerVM { Cliente="Transportes XY",  Bodega="A-03", VenceEl="28/08/2025", Dias="15 días" }
            };

            // La vista sigue fuertemente tipada a List<CarruselImagen>
            return View(imagenesCarrusel);
        }

        public IActionResult Clientes()
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Cuenta");
            return View();
        }

        public IActionResult CrearCliente()
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Cuenta");
            return View();
        }

        public IActionResult Alquileres()
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Cuenta");
            return View();
        }

        public IActionResult Bodegas()
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Cuenta");
            return View();
        }

        public IActionResult RenovarAlquiler()
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Cuenta");
            return View();
        }

        public IActionResult PrecioBodegas()
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Cuenta");
            return View();
        }

        public IActionResult Configuracion()
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Cuenta");
            return View();
        }

        public IActionResult PerfilUsuario()
        {
            return RedirectToAction("PerfilUsuario", "Cuenta");
        }
    }
}

