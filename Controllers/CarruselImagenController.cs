using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SistemaBodega.Data;
using SistemaBodega.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaBodega.Controllers
{
    public class CarruselImagenController : Controller
    {
        private readonly SistemaBodegaContext _context;
        private readonly IWebHostEnvironment _env;

        public CarruselImagenController(SistemaBodegaContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: CarruselImagen
        // q: filtro por título (opcional), page/pageSize: paginación
        [HttpGet]
        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
        {
            // Normalizar paginación
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            // Base de consulta
            var qry = _context.CarruselImagenes
                .AsNoTracking()
                .AsQueryable();

            // Filtro por título
            if (!string.IsNullOrWhiteSpace(q))
            {
                var ql = q.Trim().ToLower();
                qry = qry.Where(i => (i.Titulo ?? "").ToLower().Contains(ql));
            }

            // Orden: más recientes primero (por Id)
            qry = qry.OrderByDescending(i => i.Id);

            // Conteo y página
            var totalItems = await qry.CountAsync();
            var items = await qry
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Modelo paginado (lo que tu vista espera)
            var model = new PagedResult<CarruselImagen>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };

            // Para mantener el valor del buscador en la vista
            ViewBag.q = q ?? string.Empty;

            return View(model);
        }

        // GET: CarruselImagen/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: CarruselImagen/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarruselImagen model, IFormFile imagen)
        {
            if (imagen == null || imagen.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Debe seleccionar una imagen.");
                return View(model);
            }

            if (!IsImageContentType(imagen.ContentType))
            {
                ModelState.AddModelError(string.Empty, "Solo se permiten archivos de imagen (jpeg, png, webp, gif).");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Titulo))
            {
                model.Titulo = "Imagen sin título";
            }

            var relativePath = await SaveImageAsync(imagen);
            model.RutaImagen = relativePath;

            _context.CarruselImagenes.Add(model);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Imagen creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: CarruselImagen/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var imagen = await _context.CarruselImagenes.FindAsync(id);
            if (imagen == null) return NotFound();
            return View(imagen);
        }

        // POST: CarruselImagen/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarruselImagen model, IFormFile? imagen)
        {
            var original = await _context.CarruselImagenes.FindAsync(id);
            if (original == null) return NotFound();

            // Actualizar título (con valor por defecto si viene vacío)
            original.Titulo = string.IsNullOrWhiteSpace(model.Titulo)
                ? "Imagen sin título"
                : model.Titulo.Trim();

            // Si se sube una nueva imagen, reemplazar la anterior
            if (imagen != null && imagen.Length > 0)
            {
                if (!IsImageContentType(imagen.ContentType))
                {
                    ModelState.AddModelError(string.Empty, "Solo se permiten archivos de imagen (jpeg, png, webp, gif).");
                    return View(original);
                }

                // Borrar el archivo anterior si existe
                if (!string.IsNullOrWhiteSpace(original.RutaImagen))
                {
                    var oldAbs = Path.Combine(_env.WebRootPath, original.RutaImagen.Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldAbs))
                    {
                        try { System.IO.File.Delete(oldAbs); } catch { /* ignore */ }
                    }
                }

                // Guardar la nueva imagen
                var newRelative = await SaveImageAsync(imagen);
                original.RutaImagen = newRelative;
            }

            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Imagen actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: CarruselImagen/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var imagen = await _context.CarruselImagenes
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);
            if (imagen == null) return NotFound();
            return View(imagen); // Vista Delete.cshtml
        }

        // POST: CarruselImagen/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var imagen = await _context.CarruselImagenes.FindAsync(id);
            if (imagen == null) return NotFound();

            // Eliminar archivo físico si existe
            if (!string.IsNullOrWhiteSpace(imagen.RutaImagen))
            {
                var absPath = Path.Combine(_env.WebRootPath, imagen.RutaImagen.Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(absPath))
                {
                    try { System.IO.File.Delete(absPath); } catch { /* ignore */ }
                }
            }

            _context.CarruselImagenes.Remove(imagen);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Imagen eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ===== Helpers =====

        private static bool IsImageContentType(string? contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType)) return false;
            contentType = contentType.ToLowerInvariant();
            return contentType.StartsWith("image/") &&
                   (contentType.Contains("jpeg") ||
                    contentType.Contains("jpg") ||
                    contentType.Contains("png") ||
                    contentType.Contains("webp") ||
                    contentType.Contains("gif"));
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            // Carpeta relativa
            const string carpetaDestino = "img/carrusel";
            var ext = Path.GetExtension(file.FileName);
            var nombreArchivo = $"{Guid.NewGuid():N}{ext}";

            // Rutas
            var destinoAbs = Path.Combine(_env.WebRootPath, carpetaDestino);
            if (!Directory.Exists(destinoAbs))
                Directory.CreateDirectory(destinoAbs);

            var rutaAbs = Path.Combine(destinoAbs, nombreArchivo);

            using (var stream = new FileStream(rutaAbs, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Ruta relativa que se guarda en BD
            return Path.Combine(carpetaDestino, nombreArchivo).Replace("\\", "/");
        }
    }
}
