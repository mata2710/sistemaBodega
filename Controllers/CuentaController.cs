using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaBodega.Data;
using SistemaBodega.Models;
using Microsoft.AspNetCore.Http;

namespace SistemaBodega.Controllers
{
    public class CuentaController : Controller
    {
        private readonly SistemaBodegaContext _context;

        public CuentaController(SistemaBodegaContext context)
        {
            _context = context;
        }

        private bool UsuarioAutenticado()
        {
            return HttpContext.Session.GetInt32("UsuarioId") != null;
        }

        // Vista de Login
        public IActionResult Login()
        {
            return View();
        }

        // Procesar Login
        [HttpPost]
        public IActionResult Login(string correo, string contrasena)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Correo == correo && u.Contrasena == contrasena);

            if (usuario != null)
            {
                HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                HttpContext.Session.SetString("Usuario", usuario.NombreCompleto ?? "");
                HttpContext.Session.SetString("Rol", usuario.Rol ?? "Empleado");
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

        // Procesar Registro
        [HttpPost]
        public IActionResult Register(string nombreCompleto, string correo, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
            {
                ViewBag.Error = "Todos los campos son obligatorios.";
                return View();
            }

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
                Contrasena = contrasena,
                Rol = "Empleado"
            };

            _context.Usuarios.Add(nuevoUsuario);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Vista de Perfil de Usuario
        public IActionResult PerfilUsuario()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var rol = HttpContext.Session.GetString("Rol");

            if (usuarioId == null || string.IsNullOrEmpty(rol))
                return RedirectToAction("Login");

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario == null)
                return RedirectToAction("Login");

            ViewBag.Rol = rol;
            return View("PerfilUsuario", usuario); // 👈 MUY IMPORTANTE
        }


        // Procesar actualización de perfil
        [HttpPost]
        public IActionResult ActualizarPerfil(int id, string nombreCompleto, string correo)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario == null)
                return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(nombreCompleto) || string.IsNullOrWhiteSpace(correo))
            {
                TempData["Error"] = "Nombre y correo no pueden estar vacíos.";
                return RedirectToAction("PerfilUsuario");
            }

            usuario.NombreCompleto = nombreCompleto;
            usuario.Correo = correo;

            _context.SaveChanges();

            TempData["Mensaje"] = "Perfil actualizado correctamente.";
            return RedirectToAction("PerfilUsuario");
        }

        // Vista de recuperación
        public IActionResult Recuperar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Recuperar(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                ViewBag.Error = "Debe ingresar un correo.";
                return View();
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo == correo);
            if (usuario == null)
            {
                ViewBag.Error = "El correo ingresado no está registrado.";
                return View();
            }

            string token = Guid.NewGuid().ToString();
            usuario.TokenRecuperacion = token;
            _context.SaveChanges();

            var url = Url.Action("Restablecer", "Cuenta", new { token }, Request.Scheme);
            ViewBag.Mensaje = $"Se ha enviado un enlace de recuperación: <a href='{url}'>{url}</a>";

            return View();
        }

        public IActionResult Restablecer(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login");

            var usuario = _context.Usuarios.FirstOrDefault(u => u.TokenRecuperacion == token);
            if (usuario == null)
                return RedirectToAction("Login");

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public IActionResult Restablecer(string token, string nuevaContrasena)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(nuevaContrasena))
                return RedirectToAction("Login");

            var usuario = _context.Usuarios.FirstOrDefault(u => u.TokenRecuperacion == token);
            if (usuario == null)
                return RedirectToAction("Login");

            usuario.Contrasena = nuevaContrasena;
            usuario.TokenRecuperacion = null;
            _context.SaveChanges();

            TempData["Mensaje"] = "Contraseña actualizada correctamente.";
            return RedirectToAction("Login");
        }

        // Vista de acceso denegado
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}


