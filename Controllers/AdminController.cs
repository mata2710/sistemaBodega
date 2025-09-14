using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using SistemaBodega.Data;
using SistemaBodega.Models;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SistemaBodega.Controllers
{
    public class AdminController : Controller
    {
        private readonly SistemaBodegaContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminController(SistemaBodegaContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private bool EsAdministrador()
            => HttpContext.Session?.GetString("Rol") == "Administrador";

        // ============================
        // Helpers
        // ============================
        private static string HashPasswordSha256(string plain)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plain));
            return Convert.ToHexString(bytes); // HEX en mayúsculas
        }
        // Alternativa: BCrypt
        // private static string HashPassword(string plain) => BCrypt.Net.BCrypt.HashPassword(plain);

        private async Task<string?> GuardarFotoAsync(IFormFile foto, string? anteriorRel)
        {
            if (foto == null || foto.Length == 0) return anteriorRel;

            if (!foto.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("El archivo debe ser una imagen.");

            const long MAX_BYTES = 2 * 1024 * 1024; // 2 MB
            if (foto.Length > MAX_BYTES)
                throw new InvalidOperationException("La imagen no debe superar 2 MB.");

            var carpetaRel = "img/usuarios";
            var carpetaAbs = Path.Combine(_env.WebRootPath, carpetaRel);
            Directory.CreateDirectory(carpetaAbs);

            var nombreArchivo = $"{Guid.NewGuid():N}{Path.GetExtension(foto.FileName)}";
            var rutaAbs = Path.Combine(carpetaAbs, nombreArchivo);

            using (var stream = new FileStream(rutaAbs, FileMode.Create))
                await foto.CopyToAsync(stream);

            // Borrar foto anterior si existía
            if (!string.IsNullOrWhiteSpace(anteriorRel))
            {
                var anteriorAbs = Path.Combine(_env.WebRootPath, anteriorRel.Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (System.IO.File.Exists(anteriorAbs))
                {
                    try { System.IO.File.Delete(anteriorAbs); } catch { /* ignore */ }
                }
            }

            return Path.Combine(carpetaRel, nombreArchivo).Replace("\\", "/");
        }

        private static string? NormalizeRole(string r)
        {
            var s = (r ?? "").Trim().ToLowerInvariant();
            if (s == "admin" || s == "administrador" || s == "administrator") return "Administrador";
            if (s == "empleado" || s == "employee") return "Empleado";
            return null;
        }

        // ============================
        // USUARIOS (ACTIVOS)
        // ============================
        [HttpGet]
        public async Task<IActionResult> Usuarios(string? q, string[]? roles, int page = 1, int pageSize = 10)
        {
            if (!EsAdministrador()) return RedirectToAction("AccesoDenegado", "Cuenta");

            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var qry = _context.Usuarios
                .AsNoTracking()
                .Where(u => u.IsActive)
                .AsQueryable();

            // Buscar por nombre/correo
            if (!string.IsNullOrWhiteSpace(q))
            {
                var ql = q.Trim().ToLower();
                qry = qry.Where(u =>
                    (u.NombreCompleto ?? "").ToLower().Contains(ql) ||
                    (u.Correo ?? "").ToLower().Contains(ql));
            }

            // Filtro por roles
            var rolesNorm = (roles ?? Array.Empty<string>())
                .Select(NormalizeRole)
                .Where(x => x != null)
                .Cast<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (rolesNorm.Length > 0)
                qry = qry.Where(u => rolesNorm.Contains(u.Rol!));

            qry = qry.OrderBy(u => u.NombreCompleto).ThenBy(u => u.Id);

            var totalItems = await qry.CountAsync();
            var items = await qry.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.q = q ?? string.Empty;
            ViewBag.rolesSeleccionados = rolesNorm;
            ViewBag.rolesDisponibles = new[] { "Administrador", "Empleado" };

            return View(new PagedResult<Usuario>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            });
        }

        // ============================
        // USUARIOS (INACTIVOS)
        // ============================
        [HttpGet]
        public async Task<IActionResult> UsuariosInactivos(string? q, string[]? roles, int page = 1, int pageSize = 10)
        {
            if (!EsAdministrador()) return RedirectToAction("AccesoDenegado", "Cuenta");

            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var qry = _context.Usuarios
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(u => !u.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var ql = q.Trim().ToLower();
                qry = qry.Where(u =>
                    (u.NombreCompleto ?? "").ToLower().Contains(ql) ||
                    (u.Correo ?? "").ToLower().Contains(ql));
            }

            var rolesNorm = (roles ?? Array.Empty<string>())
                .Select(NormalizeRole)
                .Where(x => x != null)
                .Cast<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (rolesNorm.Length > 0)
                qry = qry.Where(u => rolesNorm.Contains(u.Rol!));

            qry = qry.OrderBy(u => u.NombreCompleto).ThenBy(u => u.Id);

            var totalItems = await qry.CountAsync();
            var items = await qry.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.q = q ?? string.Empty;
            ViewBag.rolesSeleccionados = rolesNorm;
            ViewBag.rolesDisponibles = new[] { "Administrador", "Empleado" };

            return View(new PagedResult<Usuario>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            });
        }

        // ============================
        // CREAR USUARIO
        // ============================
        [HttpGet]
        public IActionResult CrearUsuario()
        {
            if (!EsAdministrador()) return RedirectToAction("AccesoDenegado", "Cuenta");
            // Modelo base con Rol por defecto (Empleado) y activo
            return View(new Usuario
            {
                Rol = "Empleado",
                // Asegúrate de que tu modelo incluye estas props:
                // IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuario(
            [Bind("NombreCompleto,Correo,Contrasena,Rol,Cedula,Telefono")] Usuario model,
            IFormFile? Foto)
        {
            if (!EsAdministrador()) return RedirectToAction("AccesoDenegado", "Cuenta");

            // Validaciones mínimas
            if (string.IsNullOrWhiteSpace(model.NombreCompleto))
                ModelState.AddModelError(nameof(model.NombreCompleto), "Requerido.");
            if (string.IsNullOrWhiteSpace(model.Correo))
                ModelState.AddModelError(nameof(model.Correo), "Requerido.");
            if (string.IsNullOrWhiteSpace(model.Contrasena))
                ModelState.AddModelError(nameof(model.Contrasena), "Requerida.");
            if (string.IsNullOrWhiteSpace(model.Rol))
                ModelState.AddModelError(nameof(model.Rol), "Requerido.");

            // Correo único (activos e inactivos)
            var correoExiste = await _context.Usuarios
                .IgnoreQueryFilters()
                .AnyAsync(u => u.Correo == model.Correo);
            if (correoExiste)
                ModelState.AddModelError(nameof(model.Correo), "Ya existe un usuario con este correo.");

            if (!ModelState.IsValid)
                return View(model);

            // Hash de contraseña
            model.Contrasena = HashPasswordSha256(model.Contrasena);
            // Si usas BCrypt: model.Contrasena = BCrypt.Net.BCrypt.HashPassword(model.Contrasena);

            // Estado por defecto
            model.IsActive = true;
            model.DeactivatedAt = null;
            model.DeactivatedBy = null;

            // Foto (opcional)
            if (Foto != null && Foto.Length > 0)
            {
                try
                {
                    model.FotoFilePath = await GuardarFotoAsync(Foto, null);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(model);
                }
            }

            _context.Usuarios.Add(model);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Usuario creado correctamente.";
            return RedirectToAction(nameof(Usuarios));
        }

        // ============================
        // EDITAR USUARIO (permite abrir inactivos)
        // ============================
        [HttpGet]
        public async Task<IActionResult> EditarUsuario(int id)
        {
            if (!EsAdministrador()) return RedirectToAction("AccesoDenegado", "Cuenta");

            var usuario = await _context.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null) return NotFound();

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(
            int id,
            string nombreCompleto,
            string correo,
            string rol,
            string? cedula,
            string? telefono,
            string? nuevaContrasena,
            string? confirmarContrasena,
            IFormFile? Foto)
        {
            if (!EsAdministrador()) return RedirectToAction("AccesoDenegado", "Cuenta");

            var usuario = await _context.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return NotFound();

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(nombreCompleto) ||
                string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(rol))
            {
                TempData["Mensaje"] = "Todos los campos obligatorios deben estar completos.";
                return RedirectToAction(nameof(EditarUsuario), new { id });
            }

            // Correo único si se cambia
            if (!string.Equals(usuario.Correo, correo, StringComparison.OrdinalIgnoreCase))
            {
                var emailTaken = await _context.Usuarios
                    .IgnoreQueryFilters()
                    .AnyAsync(u => u.Correo == correo && u.Id != id);
                if (emailTaken)
                {
                    TempData["Mensaje"] = "Ya existe un usuario con ese correo.";
                    return RedirectToAction(nameof(EditarUsuario), new { id });
                }
            }

            // Cambio de contraseña (opcional)
            if (!string.IsNullOrWhiteSpace(nuevaContrasena) || !string.IsNullOrWhiteSpace(confirmarContrasena))
            {
                if ((nuevaContrasena ?? "") != (confirmarContrasena ?? ""))
                {
                    TempData["Mensaje"] = "La nueva contraseña y su confirmación no coinciden.";
                    return RedirectToAction(nameof(EditarUsuario), new { id });
                }
                usuario.Contrasena = HashPasswordSha256(nuevaContrasena!);
                // Si usas BCrypt: usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(nuevaContrasena!);
            }

            // Actualizar campos
            usuario.NombreCompleto = nombreCompleto.Trim();
            usuario.Correo = correo.Trim();
            usuario.Rol = rol.Trim();
            usuario.Cedula = string.IsNullOrWhiteSpace(cedula) ? null : cedula.Trim();
            usuario.Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();

            // Foto (opcional)
            if (Foto != null && Foto.Length > 0)
            {
                try
                {
                    usuario.FotoFilePath = await GuardarFotoAsync(Foto, usuario.FotoFilePath);
                }
                catch (Exception ex)
                {
                    TempData["Mensaje"] = ex.Message;
                    return RedirectToAction(nameof(EditarUsuario), new { id });
                }
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Usuarios));
        }

        // ============================
        // DESACTIVAR (Soft-delete)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarUsuario(int id)
        {
            if (!EsAdministrador()) return RedirectToAction("AccesoDenegado", "Cuenta");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
            if (usuario == null) return NotFound();

            usuario.IsActive = false;
            usuario.DeactivatedAt = DateTime.UtcNow;
            usuario.DeactivatedBy = User?.Identity?.Name;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Usuario desactivado correctamente.";
            return RedirectToAction(nameof(Usuarios));
        }

        // ============================
        // ACTIVAR
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarUsuario(int id)
        {
            if (!EsAdministrador()) return RedirectToAction("AccesoDenegado", "Cuenta");

            var usuario = await _context.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return NotFound();

            usuario.IsActive = true;
            usuario.DeactivatedAt = null;
            usuario.DeactivatedBy = null;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Usuario reactivado correctamente.";
            return RedirectToAction(nameof(UsuariosInactivos));
        }
    }
}

