using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using SistemaBodega.Data;
using SistemaBodega.Models;
using SistemaBodega.Helpers; // ✅ Para usar EmailProvidersSettings
using Microsoft.Extensions.Options; // ✅ Para inyectar configuración SMTP
using System.Net.Mail;
using System.Net;

namespace SistemaBodega.Controllers
{
    public class CuentaController : Controller
    {
        private readonly SistemaBodegaContext _context;
        private readonly EmailProvidersSettings _emailSettings; // ✅ Configuración SMTP inyectada

        public CuentaController(SistemaBodegaContext context, IOptions<EmailProvidersSettings> emailSettings)
        {
            _context = context;
            _emailSettings = emailSettings.Value; // ✅ Leer configuración desde appsettings.json
        }

        private bool UsuarioAutenticado()
        {
            return HttpContext.Session.GetInt32("UsuarioId") != null;
        }

        public IActionResult Login() => View();

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

        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(string nombreCompleto, string correo, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
            {
                ViewBag.Error = "Todos los campos son obligatorios.";
                return View();
            }

            if (_context.Usuarios.Any(u => u.Correo == correo))
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

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

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
            return View("PerfilUsuario", usuario);
        }

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

        // ✅ Vista del formulario de recuperación
        public IActionResult Recuperar() => View();

        // ✅ Enviar enlace de recuperación por correo SMTP
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

            // ✅ Construir el enlace
            var url = Url.Action("Restablecer", "Cuenta", new { token }, Request.Scheme);
            var asunto = "Recuperación de contraseña";
            var cuerpo = $"<p>Haz clic en el siguiente enlace para restablecer tu contraseña:</p><p><a href='{url}'>{url}</a></p>";

            // ✅ Enviar correo con configuración SMTP
            var proveedor = _emailSettings.DefaultProvider;
            var config = _emailSettings.Providers[proveedor];

            try
            {
                using (var smtpClient = new SmtpClient(config.Host, config.Port))
                {
                    smtpClient.Credentials = new NetworkCredential(config.User, config.Password);
                    smtpClient.EnableSsl = config.EnableSsl;

                    var mensaje = new MailMessage
                    {
                        From = new MailAddress(config.User, "Sistema Bodega"),
                        Subject = asunto,
                        Body = cuerpo,
                        IsBodyHtml = true
                    };

                    mensaje.To.Add(correo);
                    smtpClient.Send(mensaje);
                }

                ViewBag.Mensaje = "Se ha enviado el enlace de recuperación a su correo.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al enviar el correo: " + ex.Message;
            }

            return View();
        }

        // ✅ Vista para ingresar nueva contraseña
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

        // ✅ Procesar nueva contraseña
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

        public IActionResult AccesoDenegado() => View();
    }
}

