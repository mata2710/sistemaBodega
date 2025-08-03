using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SistemaBodega.Data;
using SistemaBodega.Models;
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

            // Cargar las imágenes del carrusel desde la base de datos
            var imagenesCarrusel = _context.CarruselImagenes.ToList();
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
