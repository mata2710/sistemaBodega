using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting; // IWebHostEnvironment
using SistemaBodega.Data;
using SistemaBodega.Models;

namespace SistemaBodega.Controllers
{
    public class AlquileresController : Controller
    {
        private readonly SistemaBodegaContext _context;
        private readonly IWebHostEnvironment _env;

        public AlquileresController(SistemaBodegaContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // =========================================================
        // GET: Alquileres (Index con paginación + búsqueda + filtros + orden)
        //  - Por defecto: solo activos
        //  - verInactivos = true: solo inactivos
        // =========================================================
        public async Task<IActionResult> Index(
     int page = 1,
     int pageSize = 10,
     string? q = null,
     bool verInactivos = false,
     bool? renovacion = null,
     string? sort = null,
     string dir = "desc")
        {
            // Normaliza page y pageSize
            if (page < 1) page = 1;
            var allowed = new[] { 5, 10, 25, 50, 100 };
            if (!allowed.Contains(pageSize)) pageSize = 10;

            // === Compatibilidad con la vista que envía "activos"
            // Si llega activos=false -> ver inactivos
            // Si llega activos=true  -> ver activos
            // Si no llega "activos", se respeta el valor de verInactivos (default: false -> activos)
            var activosValues = Request.Query["activos"];
            if (activosValues.Count > 0)
            {
                // Si alguno de los valores es "false", mostramos inactivos.
                // (robusto cuando el form envía checkbox + hidden con el mismo nombre)
                if (activosValues.Any(v => string.Equals(v, "false", StringComparison.OrdinalIgnoreCase)))
                    verInactivos = true;
                else
                    verInactivos = false;
            }

            // Construcción de la consulta (ignorando el filtro global para poder alternar)
            var query = _context.Alquileres
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(a => a.Bodega)
                .Include(a => a.Cliente)
                .AsQueryable();

            // Filtro principal por estado
            query = verInactivos ? query.Where(a => !a.Activo)
                                 : query.Where(a => a.Activo);

            // Búsqueda libre
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(a =>
                    (a.Cliente != null && (
                        a.Cliente.Nombre.Contains(q) ||
                        (a.Cliente.Identificacion != null && a.Cliente.Identificacion.Contains(q))
                    )) ||
                    (a.Bodega != null && (
                        a.Bodega.Nombre.Contains(q) ||
                        (a.Bodega.Ubicacion != null && a.Bodega.Ubicacion.Contains(q))
                    ))
                );
            }

            // Filtro de renovación
            if (renovacion.HasValue)
                query = query.Where(a => a.RenovacionAutomatica == renovacion.Value);

            // Ordenación
            bool asc = string.Equals(dir, "asc", StringComparison.OrdinalIgnoreCase);
            switch (sort)
            {
                case "cliente":
                    query = asc ? query.OrderBy(a => a.Cliente.Nombre)
                                : query.OrderByDescending(a => a.Cliente.Nombre);
                    break;
                case "bodega":
                    query = asc ? query.OrderBy(a => a.Bodega.Nombre)
                                : query.OrderByDescending(a => a.Bodega.Nombre);
                    break;
                case "inicio":
                    query = asc ? query.OrderBy(a => a.FechaInicio)
                                : query.OrderByDescending(a => a.FechaInicio);
                    break;
                case "fin":
                    query = asc ? query.OrderBy(a => a.FechaFin)
                                : query.OrderByDescending(a => a.FechaFin);
                    break;
                case "renov":
                    query = asc ? query.OrderBy(a => a.RenovacionAutomatica)
                                : query.OrderByDescending(a => a.RenovacionAutomatica);
                    break;
                case "estado":
                    query = asc ? query.OrderBy(a => a.Activo)
                                : query.OrderByDescending(a => a.Activo);
                    break;
                default:
                    query = query.OrderByDescending(a => a.FechaInicio);
                    break;
            }

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new PagedResult<Alquiler>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };

            // Estado para la vista
            ViewBag.q = q;
            ViewBag.VerInactivos = verInactivos; // <- úsalo en la vista si quieres
            ViewBag.RenovFilter = renovacion;
            ViewBag.Sort = sort;
            ViewBag.Dir = asc ? "asc" : "desc";

            return View(vm);
        }


        // (Opcional) Atajo para ir directo a inactivos desde un link si lo deseas
        [HttpGet]
        public Task<IActionResult> Inactivos(string? q, int page = 1, int pageSize = 10)
            => Index(page: page, pageSize: pageSize, q: q, verInactivos: true);

        // =========================================================
        // GET: Alquileres/Details/5 (permite ver inactivos también)
        // =========================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var alquiler = await _context.Alquileres
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(a => a.Bodega)
                .Include(a => a.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (alquiler == null) return NotFound();
            return View(alquiler);
        }

        // =========================================================
        // GET: Alquileres/Create
        // =========================================================
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("Rol") != "Administrador")
                return RedirectToAction("Index", "Home");

            await CargarCombosAsync();
            return View();
        }

        // =========================================================
        // API: Datos de Bodega para autocompletar
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> GetBodegaData(int id)
        {
            var b = await _context.Bodegas
                                  .AsNoTracking()
                                  .Where(x => x.Id == id)
                                  .Select(x => new
                                  {
                                      areaM2 = x.AreaM2 ?? 0m,
                                      precioPorM2 = x.PrecioAlquilerPorM2 ?? 0m,
                                      precioCalculado = (x.AreaM2 ?? 0m) * (x.PrecioAlquilerPorM2 ?? 0m)
                                  })
                                  .FirstOrDefaultAsync();

            if (b == null) return NotFound();
            return Json(b);
        }

        // =========================================================
        // POST: Alquileres/Create
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClienteId,BodegaId,FechaInicio,FechaFin,AreaM2,PrecioPorM2,PrecioAlquiler,AumentoAnualPorcentaje,Observaciones,RenovacionAutomatica,Activo,ContratoArchivo")] Alquiler model)
        {
            if (HttpContext.Session.GetString("Rol") != "Administrador")
                return RedirectToAction("Index", "Home");

            // Validaciones mínimas
            if (model.FechaFin < model.FechaInicio)
                ModelState.AddModelError(nameof(model.FechaFin), "La fecha fin no puede ser menor que la fecha inicio.");

            if (!await _context.Clientes.AnyAsync(c => c.Id == model.ClienteId))
                ModelState.AddModelError(nameof(model.ClienteId), "Cliente no válido.");

            if (!await _context.Bodegas.AnyAsync(b => b.Id == model.BodegaId))
                ModelState.AddModelError(nameof(model.BodegaId), "Bodega no válida.");

            if (model.AreaM2 < 0) ModelState.AddModelError(nameof(model.AreaM2), "El área no puede ser negativa.");
            if (model.PrecioPorM2 < 0) ModelState.AddModelError(nameof(model.PrecioPorM2), "El precio por m² no puede ser negativo.");

            if (model.AumentoAnualPorcentaje.HasValue && (model.AumentoAnualPorcentaje.Value < 0 || model.AumentoAnualPorcentaje.Value > 100))
                ModelState.AddModelError(nameof(model.AumentoAnualPorcentaje), "El aumento debe estar entre 0% y 100%.");

            // Cálculo: PrecioAlquiler
            var calculoBase = (model.AreaM2 ?? 0m) * (model.PrecioPorM2 ?? 0m);
            if (model.AumentoAnualPorcentaje.HasValue && model.AumentoAnualPorcentaje.Value > 0m)
            {
                var factor = 1m + (model.AumentoAnualPorcentaje.Value / 100m);
                calculoBase = Math.Round(calculoBase * factor, 2, MidpointRounding.AwayFromZero);
            }
            model.PrecioAlquiler = Math.Round(calculoBase, 2, MidpointRounding.AwayFromZero);

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(model.ClienteId, model.BodegaId);
                return View(model);
            }

            // Archivo de contrato -> guardar ruta
            await GuardarContratoSiCorresponde(model);

            _context.Alquileres.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Alquiler creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // GET: Alquileres/Edit/5 (permite abrir inactivos)
        // =========================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("Rol") != "Administrador")
                return RedirectToAction("Index", "Home");

            if (id == null) return NotFound();

            var alquiler = await _context.Alquileres
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Id == id);
            if (alquiler == null) return NotFound();

            await CargarCombosAsync(alquiler.ClienteId, alquiler.BodegaId);
            return View(alquiler);
        }

        // =========================================================
        // POST: Alquileres/Edit/5
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,BodegaId,FechaInicio,FechaFin,AreaM2,PrecioPorM2,PrecioAlquiler,AumentoAnualPorcentaje,Observaciones,RenovacionAutomatica,Activo,ContratoArchivo,ContratoFilePath")] Alquiler model)
        {
            if (HttpContext.Session.GetString("Rol") != "Administrador")
                return RedirectToAction("Index", "Home");

            if (id != model.Id) return NotFound();

            // Validaciones mínimas
            if (model.FechaFin < model.FechaInicio)
                ModelState.AddModelError(nameof(model.FechaFin), "La fecha fin no puede ser menor que la fecha inicio.");

            if (!await _context.Clientes.AnyAsync(c => c.Id == model.ClienteId))
                ModelState.AddModelError(nameof(model.ClienteId), "Cliente no válido.");

            if (!await _context.Bodegas.AnyAsync(b => b.Id == model.BodegaId))
                ModelState.AddModelError(nameof(model.BodegaId), "Bodega no válida.");

            if (model.AreaM2 < 0) ModelState.AddModelError(nameof(model.AreaM2), "El área no puede ser negativa.");
            if (model.PrecioPorM2 < 0) ModelState.AddModelError(nameof(model.PrecioPorM2), "El precio por m² no puede ser negativo.");

            if (model.AumentoAnualPorcentaje.HasValue && (model.AumentoAnualPorcentaje.Value < 0 || model.AumentoAnualPorcentaje.Value > 100))
                ModelState.AddModelError(nameof(model.AumentoAnualPorcentaje), "El aumento debe estar entre 0% y 100%.");

            // Recalcular precio
            var calculoBase = (model.AreaM2 ?? 0m) * (model.PrecioPorM2 ?? 0m);
            if (model.AumentoAnualPorcentaje.HasValue && model.AumentoAnualPorcentaje.Value > 0m)
            {
                var factor = 1m + (model.AumentoAnualPorcentaje.Value / 100m);
                calculoBase = Math.Round(calculoBase * factor, 2, MidpointRounding.AwayFromZero);
            }
            model.PrecioAlquiler = Math.Round(calculoBase, 2, MidpointRounding.AwayFromZero);

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(model.ClienteId, model.BodegaId);
                return View(model);
            }

            // Cargar original para conservar ruta si no se sube nuevo archivo
            var original = await _context.Alquileres
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Id == id);
            if (original == null) return NotFound();

            // Guardar archivo nuevo si corresponde (y borrar el anterior)
            await GuardarContratoSiCorresponde(model, original.ContratoFilePath);

            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Alquiler actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AlquilerExists(model.Id)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // POST: Alquileres/Inactivar/5  (NO hay reactivar, NO hay eliminar)
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inactivar(int id)
        {
            if (HttpContext.Session.GetString("Rol") != "Administrador")
                return RedirectToAction("Index", "Home");

            var alquiler = await _context.Alquileres
                .FirstOrDefaultAsync(a => a.Id == id && a.Activo);
            if (alquiler == null)
            {
                TempData["Error"] = "No se encontró el alquiler o ya está inactivo.";
                return RedirectToAction(nameof(Index));
            }

            alquiler.Activo = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Alquiler desactivado correctamente.";
            return RedirectToAction(nameof(Index)); // por defecto lista de activos
        }

        // ===================== Helpers =====================
        private async Task CargarCombosAsync(int? clienteId = null, int? bodegaId = null)
        {
            var clientes = await _context.Clientes
                .AsNoTracking()
                .Select(c => new { c.Id, Nombre = c.Nombre + " (" + c.Identificacion + ")" })
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            var bodegas = await _context.Bodegas
                .AsNoTracking()
                .Select(b => new { b.Id, Nombre = b.Nombre + " - " + b.Ubicacion })
                .OrderBy(b => b.Nombre)
                .ToListAsync();

            ViewData["ClienteId"] = new SelectList(clientes, "Id", "Nombre", clienteId);
            ViewData["BodegaId"] = new SelectList(bodegas, "Id", "Nombre", bodegaId);
        }

        private bool AlquilerExists(int id) =>
            _context.Alquileres.IgnoreQueryFilters().Any(e => e.Id == id);

        private async Task GuardarContratoSiCorresponde(Alquiler model, string? anteriorPath = null)
        {
            if (model.ContratoArchivo == null || model.ContratoArchivo.Length == 0)
            {
                if (!string.IsNullOrWhiteSpace(anteriorPath))
                    model.ContratoFilePath = anteriorPath;
                return;
            }

            var ext = Path.GetExtension(model.ContratoArchivo.FileName).ToLowerInvariant();
            var permitidas = new[] { ".pdf", ".png", ".jpg", ".jpeg" };
            if (!permitidas.Contains(ext))
            {
                ModelState.AddModelError(nameof(Alquiler.ContratoArchivo), "Formato no permitido. Solo PDF, PNG, JPG.");
                return;
            }

            var folder = Path.Combine(_env.WebRootPath, "contratos");
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            using (var stream = System.IO.File.Create(fullPath))
            {
                await model.ContratoArchivo.CopyToAsync(stream);
            }

            if (!string.IsNullOrWhiteSpace(anteriorPath))
            {
                var oldFull = Path.Combine(_env.WebRootPath, anteriorPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldFull))
                    System.IO.File.Delete(oldFull);
            }

            model.ContratoFilePath = $"/contratos/{fileName}";
        }
    }
}


