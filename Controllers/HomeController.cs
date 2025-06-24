using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace SistemaBodega.Controllers
{
    public class HomeController : Controller
    {
        // Función auxiliar para validar sesión
        private bool UsuarioAutenticado()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("Usuario"));
        }

        public IActionResult Index()
        {
            if (!UsuarioAutenticado()) return RedirectToAction("Login", "Cuenta");
            return View();
        }

        public IActionResult Clientes()
        {
            if (!UsuarioAutenticado()) return RedirectToAction("Login", "Cuenta");
            return View();
        }

        public IActionResult CrearCliente()
        {
            if (!UsuarioAutenticado()) return RedirectToAction("Login", "Cuenta");
            return View();
        }

        public IActionResult Alquileres()
        {
            if (!UsuarioAutenticado()) return RedirectToAction("Login", "Cuenta");
            return View();
        }

        public IActionResult Bodegas()
        {
            if (!UsuarioAutenticado()) return RedirectToAction("Login", "Cuenta");
            return View();
        }

        public IActionResult RenovarAlquiler()
        {
            if (!UsuarioAutenticado()) return RedirectToAction("Login", "Cuenta");
            return View();
        }

        public IActionResult PrecioBodegas()
        {
            if (!UsuarioAutenticado()) return RedirectToAction("Login", "Cuenta");
            return View();
        }

        public IActionResult PerfilUsuario()
        {
            if (!UsuarioAutenticado()) return RedirectToAction("Login", "Cuenta");
            return View();
        }

        public IActionResult Configuracion()
        {
            if (!UsuarioAutenticado()) return RedirectToAction("Login", "Cuenta");
            return View();
        }
    }
}

