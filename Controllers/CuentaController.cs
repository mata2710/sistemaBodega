using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaBodega.Data;
using SistemaBodega.Models;

namespace SistemaBodega.Controllers
{
    public class CuentaController : Controller
    {
        private readonly SistemaBodegaContext _context;

        public CuentaController(SistemaBodegaContext context)
        {
            _context = context;
        }

        // Función auxiliar para verificar sesión activa
        private bool UsuarioAutenticado()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("Usuario"));
        }

        // Vista de Login
        public IActionResult Login()
        {
            return View();
        }

        // Procesar formulario de Login
        [HttpPost]
        public IActionResult Login(string correo, string contrasena)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Correo == correo && u.Contrasena == contrasena);

            if (usuario != null)
            {
                HttpContext.Session.SetString("Usuario", usuario.NombreCompleto);
                HttpContext.Session.SetString("Rol", usuario.Rol);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Credenciales incorrectas.";
            return View();
        }

        // Vista de Registro
        public IActionResult Register()
        {
            return View();
        }

        // Procesar formulario de Registro
        [HttpPost]
        public IActionResult Register(string nombreCompleto, string correo, string contrasena, string rol)
        {
            var existeUsuario = _context.Usuarios.Any(u => u.Correo == correo);
            if (existeUsuario)
            {
                ViewBag.Error = "El correo ya está registrado.";
                return View();
            }

            var nuevoUsuario = new Usuario
            {
                NombreCompleto = nombreCompleto,
                Correo = correo,
                Contrasena = contrasena, // ⚠️ En producción, usar hash seguro
                Rol = rol
            };

            _context.Usuarios.Add(nuevoUsuario);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        // Logout
        public IActionResult Logout()
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login");

            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Vista PerfilUsuario
        public IActionResult PerfilUsuario()
        {
            var nombre = HttpContext.Session.GetString("Usuario");
            if (string.IsNullOrEmpty(nombre))
            {
                return RedirectToAction("Login");
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreCompleto == nombre);
            if (usuario == null)
            {
                return RedirectToAction("Login");
            }

            return View(usuario);
        }

        // Vista Recuperar contraseña
        public IActionResult Recuperar()
        {
            return View();
        }

        // Procesar recuperación (enviar enlace)
        [HttpPost]
        public IActionResult Recuperar(string correo)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo == correo);
            if (usuario == null)
            {
                ViewBag.Error = "El correo ingresado no está registrado.";
                return View();
            }

            // Generar token y guardar
            string token = Guid.NewGuid().ToString();
            usuario.TokenRecuperacion = token;
            _context.SaveChanges();

            // Enlace de recuperación
            var url = Url.Action("Restablecer", "Cuenta", new { token }, Request.Scheme);
            ViewBag.Mensaje = $"Se ha enviado un enlace de recuperación: <a href='{url}'>{url}</a>";

            // Aquí podrías enviar el correo real
            return View();
        }

        // Vista para restablecer contraseña
        public IActionResult Restablecer(string token)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.TokenRecuperacion == token);
            if (usuario == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Token = token;
            return View();
        }

        // Procesar nueva contraseña
        [HttpPost]
        public IActionResult Restablecer(string token, string nuevaContrasena)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.TokenRecuperacion == token);
            if (usuario == null)
            {
                return RedirectToAction("Login");
            }

            usuario.Contrasena = nuevaContrasena;
            usuario.TokenRecuperacion = null;
            _context.SaveChanges();

            TempData["Mensaje"] = "Contraseña actualizada correctamente.";
            return RedirectToAction("Login");
        }
    }
}



