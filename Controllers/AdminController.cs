using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaBodega.Data;
using SistemaBodega.Models;

namespace SistemaBodega.Controllers
{
    public class AdminController : Controller
    {
        private readonly SistemaBodegaContext _context;

        public AdminController(SistemaBodegaContext context)
        {
            _context = context;
        }

        private bool EsAdministrador()
        {
            return HttpContext.Session.GetString("Rol") == "Administrador";
        }

        // Vista principal de usuarios
        public IActionResult Usuarios()
        {
            if (!EsAdministrador())
                return RedirectToAction("AccesoDenegado", "Cuenta");

            var usuarios = _context.Usuarios.ToList();
            return View(usuarios);
        }

        // Vista para editar usuario
        public IActionResult EditarUsuario(int id)
        {
            if (!EsAdministrador())
                return RedirectToAction("AccesoDenegado", "Cuenta");

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        // Procesar la edición del usuario
        [HttpPost]
        public IActionResult EditarUsuario(int id, string nombreCompleto, string correo, string rol)
        {
            if (!EsAdministrador())
                return RedirectToAction("AccesoDenegado", "Cuenta");

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario == null)
                return NotFound();

            usuario.NombreCompleto = nombreCompleto;
            usuario.Correo = correo;
            usuario.Rol = rol;

            _context.SaveChanges();

            TempData["Mensaje"] = "Usuario actualizado correctamente.";
            return RedirectToAction("Usuarios");
        }

        // Eliminar usuario
        [HttpPost]
        public IActionResult EliminarUsuario(int id)
        {
            if (!EsAdministrador())
                return RedirectToAction("AccesoDenegado", "Cuenta");

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario == null)
                return NotFound();

            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();

            TempData["Mensaje"] = "Usuario eliminado correctamente.";
            return RedirectToAction("Usuarios");
        }
    }
}
