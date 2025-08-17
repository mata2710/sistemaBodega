using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;          // 👈 para IWebHostEnvironment
using SistemaBodega.Data;
using SistemaBodega.Models;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.IO;                             // 👈 para Path/FileStream

namespace SistemaBodega.Controllers
{
    public class AdminController : Controller
    {
        private readonly SistemaBodegaContext _context;
        private readonly IWebHostEnvironment _env;   // 👈 agregado

        public AdminController(SistemaBodegaContext context, IWebHostEnvironment env) // 👈 agregado
        {
            _context = context;
            _env = env;                                // 👈 agregado
        }

        private bool EsAdministrador()
        {
            return HttpContext.Session?.GetString("Rol") == "Administrador";
        }

        // GET: /Admin/Usuarios
        // Filtros con model binding: q (texto), roles (checkbox múltiple), page, pageSize
        [HttpGet]
        public async Task<IActionResult> Usuarios(string? q, string[]? roles, int page = 1, int pageSize = 10)
        {
            if (!EsAdministrador())
                return RedirectToAction("AccesoDenegado", "Cuenta");

            // Normaliza paginación
            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            // Base de consulta
            var qry = _context.Usuarios.AsNoTracking().AsQueryable();

            // Filtro texto (nombre/correo)
            if (!string.IsNullOrWhiteSpace(q))
            {
                var ql = q.Trim().ToLower();
                qry = qry.Where(u =>
                    (u.NombreCompleto ?? "").ToLower().Contains(ql) ||
                    (u.Correo ?? "").ToLower().Contains(ql));
            }

            // Normalizar roles recibidos a las 2 únicas opciones
            string? NormalizeRole(string r)
            {
                var s = (r ?? "").Trim().ToLowerInvariant();
                if (s == "admin" || s == "administrador" || s == "administrator") return "Administrador";
                if (s == "empleado" || s == "employee") return "Empleado";
                return null;
            }

            var rolesNorm = (roles ?? Array.Empty<string>())
                .Select(NormalizeRole)
                .Where(x => x != null)
                .Cast<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (rolesNorm.Length > 0)
            {
                qry = qry.Where(u => rolesNorm.Contains(u.Rol!));
            }

            // Orden por defecto
            qry = qry.OrderBy(u => u.NombreCompleto).ThenBy(u => u.Id);

            // Conteo y paginación
            var totalItems = await qry.CountAsync();
            var items = await qry
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // (Opcional) ViewBags por si quieres usarlos
            ViewBag.q = q ?? string.Empty;
            ViewBag.rolesSeleccionados = rolesNorm;
            ViewBag.rolesDisponibles = new[] { "Administrador", "Empleado" };

            // Entrega el modelo paginado esperado por la vista
            var model = new PagedResult<Usuario>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };

            return View(model);
        }

        // GET: /Admin/EditarUsuario/5
        [HttpGet]
        public async Task<IActionResult> EditarUsuario(int id)
        {
            if (!EsAdministrador())
                return RedirectToAction("AccesoDenegado", "Cuenta");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        // POST: /Admin/EditarUsuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(int id, string nombreCompleto, string correo, string rol, IFormFile? Foto) // 👈 Foto agregado
        {
            if (!EsAdministrador())
                return RedirectToAction("AccesoDenegado", "Cuenta");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(nombreCompleto) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(rol))
            {
                TempData["Mensaje"] = "Todos los campos son obligatorios.";
                return RedirectToAction("EditarUsuario", new { id });
            }

            usuario.NombreCompleto = nombreCompleto.Trim();
            usuario.Correo = correo.Trim();
            usuario.Rol = rol.Trim();

            // ===== Guardado de imagen (opcional) =====
            if (Foto != null && Foto.Length > 0)
            {
                if (!Foto.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Mensaje"] = "El archivo debe ser una imagen.";
                    return RedirectToAction("EditarUsuario", new { id });
                }

                // (Opcional) limitar a 2 MB
                const long MAX_BYTES = 2 * 1024 * 1024;
                if (Foto.Length > MAX_BYTES)
                {
                    TempData["Mensaje"] = "La imagen no debe superar 2 MB.";
                    return RedirectToAction("EditarUsuario", new { id });
                }

                var carpetaRel = "img/usuarios";
                var carpetaAbs = Path.Combine(_env.WebRootPath, carpetaRel);
                if (!Directory.Exists(carpetaAbs))
                    Directory.CreateDirectory(carpetaAbs);

                var nombreArchivo = $"{Guid.NewGuid():N}{Path.GetExtension(Foto.FileName)}";
                var rutaAbs = Path.Combine(carpetaAbs, nombreArchivo);

                using (var stream = new FileStream(rutaAbs, FileMode.Create))
                {
                    await Foto.CopyToAsync(stream);
                }

                // Borrar foto anterior si existía
                if (!string.IsNullOrWhiteSpace(usuario.FotoFilePath))
                {
                    var anteriorAbs = Path.Combine(_env.WebRootPath, usuario.FotoFilePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(anteriorAbs))
                    {
                        try { System.IO.File.Delete(anteriorAbs); } catch { /* ignore */ }
                    }
                }

                // Guardar ruta relativa en BD
                usuario.FotoFilePath = Path.Combine(carpetaRel, nombreArchivo).Replace("\\", "/");
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Usuario actualizado correctamente.";
            return RedirectToAction("Usuarios");
        }

        // GET: /Admin/DeleteUsuario/5
        // Muestra la vista de confirmación "Delete.cshtml"
        [HttpGet]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            if (!EsAdministrador())
                return RedirectToAction("AccesoDenegado", "Cuenta");

            var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null)
                return NotFound();

            return View("Delete", usuario); // tu vista se llama "Delete"
        }

        // POST: /Admin/EliminarUsuario/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            if (!EsAdministrador())
                return RedirectToAction("AccesoDenegado", "Cuenta");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null)
                return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Usuario eliminado correctamente.";
            return RedirectToAction("Usuarios");
        }
    }
}



