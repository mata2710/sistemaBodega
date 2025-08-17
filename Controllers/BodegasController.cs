using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaBodega.Data;
using SistemaBodega.Models;
using SistemaBodega.Filters; // Filtro personalizado

namespace SistemaBodega.Controllers
{
    public class BodegasController : Controller
    {
        private readonly SistemaBodegaContext _context;

        public BodegasController(SistemaBodegaContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET: Bodegas (filtros + paginación + sort + quick pills)
        // ?nombre=&ubicacion=&complejo=&estado=&sort=precio&dir=desc&page=1&pageSize=10
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? nombre,
            string? ubicacion,
            string? complejo,
            string? estado,
            string? sort = "nombre",
            string? dir = "asc",
            int page = 1,
            int pageSize = 10)
        {
            var allowed = new HashSet<int> { 5, 10, 25, 50, 100 };
            if (!allowed.Contains(pageSize)) pageSize = 10;
            if (page < 1) page = 1;

            var q = _context.Bodegas.AsNoTracking().AsQueryable();

            // --- Filtros ---
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var n = nombre.Trim();
                q = q.Where(b => EF.Functions.Like(b.Nombre, $"%{n}%"));
            }
            if (!string.IsNullOrWhiteSpace(ubicacion))
            {
                var u = ubicacion.Trim();
                q = q.Where(b => b.Ubicacion != null && EF.Functions.Like(b.Ubicacion, $"%{u}%"));
            }
            if (!string.IsNullOrWhiteSpace(complejo))
            {
                var c = complejo.Trim();
                q = q.Where(b => b.Complejo != null && EF.Functions.Like(b.Complejo, $"%{c}%"));
            }
            if (!string.IsNullOrWhiteSpace(estado))
            {
                var e = estado.Trim();
                q = q.Where(b => b.Estado != null && EF.Functions.Like(b.Estado, $"%{e}%"));
            }

            // --- Ordenamiento (REMOVIDO "ubicacion") ---
            sort ??= "nombre";
            sort = sort.ToLowerInvariant();
            var desc = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);

            q = (sort, desc) switch
            {
                ("area", false) => q.OrderBy(b => b.AreaM2 ?? 0).ThenBy(b => b.Nombre),
                ("area", true) => q.OrderByDescending(b => b.AreaM2 ?? 0).ThenBy(b => b.Nombre),

                ("pm2", false) => q.OrderBy(b => b.PrecioAlquilerPorM2 ?? 0).ThenBy(b => b.Nombre),
                ("pm2", true) => q.OrderByDescending(b => b.PrecioAlquilerPorM2 ?? 0).ThenBy(b => b.Nombre),

                ("precio", false) => q.OrderBy(b => b.Precio ?? 0).ThenBy(b => b.Nombre),
                ("precio", true) => q.OrderByDescending(b => b.Precio ?? 0).ThenBy(b => b.Nombre),

                ("complejo", false) => q.OrderBy(b => b.Complejo).ThenBy(b => b.Nombre),
                ("complejo", true) => q.OrderByDescending(b => b.Complejo).ThenBy(b => b.Nombre),

                ("estado", false) => q.OrderBy(b => b.Estado).ThenBy(b => b.Nombre),
                ("estado", true) => q.OrderByDescending(b => b.Estado).ThenBy(b => b.Nombre),

                // default: nombre
                (_, false) => q.OrderBy(b => b.Nombre),
                (_, true) => q.OrderByDescending(b => b.Nombre),
            };

            // --- Paginación ---
            var total = await q.CountAsync();
            var totalPages = total == 0 ? 1 : (int)Math.Ceiling((decimal)total / pageSize);
            if (page > totalPages) page = totalPages;

            var items = await q.Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

            // ViewBags para la vista
            ViewBag.FiltroNombre = nombre;
            ViewBag.FiltroUbicacion = ubicacion;
            ViewBag.FiltroComplejo = complejo;
            ViewBag.FiltroEstado = estado;
            ViewBag.Sort = sort;
            ViewBag.Dir = desc ? "desc" : "asc";

            var vm = new PagedResult<Bodega>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };
            return View(vm);
        }

        // =========================================================
        // GET: Bodegas/Details/5
        // =========================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var bodega = await _context.Bodegas
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (bodega == null) return NotFound();

            return View(bodega);
        }

        // =========================================================
        // GET: Bodegas/Create
        // =========================================================
        [AuthorizeRol("Administrador")]
        public IActionResult Create()
        {
            return View();
        }

        // =========================================================
        // POST: Bodegas/Create
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Ubicacion,Complejo,Estado,AreaM2,PrecioAlquilerPorM2")] Bodega bodega)
        {
            // La columna Precio es calculada en DB
            ModelState.Remove("Precio");

            // Validaciones simples
            if (bodega.AreaM2 < 0) ModelState.AddModelError(nameof(Bodega.AreaM2), "El área no puede ser negativa.");
            if (bodega.PrecioAlquilerPorM2 < 0) ModelState.AddModelError(nameof(Bodega.PrecioAlquilerPorM2), "El precio por m² no puede ser negativo.");

            if (!ModelState.IsValid) return View(bodega);

            _context.Add(bodega);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // GET: Bodegas/Edit/5
        // =========================================================
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var bodega = await _context.Bodegas.FindAsync(id);
            if (bodega == null) return NotFound();

            return View(bodega);
        }

        // =========================================================
        // POST: Bodegas/Edit/5
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Ubicacion,Complejo,Estado,AreaM2,PrecioAlquilerPorM2")] Bodega bodega)
        {
            if (id != bodega.Id) return NotFound();

            // La columna Precio es calculada en DB
            ModelState.Remove("Precio");

            // Validaciones simples
            if (bodega.AreaM2 < 0) ModelState.AddModelError(nameof(Bodega.AreaM2), "El área no puede ser negativa.");
            if (bodega.PrecioAlquilerPorM2 < 0) ModelState.AddModelError(nameof(Bodega.PrecioAlquilerPorM2), "El precio por m² no puede ser negativo.");

            if (!ModelState.IsValid) return View(bodega);

            try
            {
                _context.Attach(bodega);
                _context.Entry(bodega).Property(x => x.Nombre).IsModified = true;
                _context.Entry(bodega).Property(x => x.Ubicacion).IsModified = true;
                _context.Entry(bodega).Property(x => x.Complejo).IsModified = true;
                _context.Entry(bodega).Property(x => x.Estado).IsModified = true;
                _context.Entry(bodega).Property(x => x.AreaM2).IsModified = true;
                _context.Entry(bodega).Property(x => x.PrecioAlquilerPorM2).IsModified = true;

                // Evitar escribir la calculada
                _context.Entry(bodega).Property(x => x.Precio).IsModified = false;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BodegaExists(bodega.Id)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // GET: Bodegas/Delete/5
        // =========================================================
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var bodega = await _context.Bodegas
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (bodega == null) return NotFound();

            return View(bodega);
        }

        // =========================================================
        // POST: Bodegas/Delete/5
        // =========================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bodega = await _context.Bodegas.FindAsync(id);
            if (bodega != null)
            {
                _context.Bodegas.Remove(bodega);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BodegaExists(int id) => _context.Bodegas.Any(e => e.Id == id);
    }
}


