using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaBodega.Data;
using SistemaBodega.Models;
using SistemaBodega.Filters;

namespace SistemaBodega.Controllers
{
    public class MantenimientosController : Controller
    {
        private readonly SistemaBodegaContext _context;

        public MantenimientosController(SistemaBodegaContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET: Mantenimientos (filtros + paginación)
        // /Mantenimientos?bodegaId=&tipo=&empresa=&desde=&hasta=&q=&page=1&pageSize=25
        // =========================================================
        public async Task<IActionResult> Index(
            int? bodegaId,
            string? tipo,
            string? empresa,
            DateTime? desde,
            DateTime? hasta,
            string? q,
            int page = 1,
            int pageSize = 25)
        {
            if (page < 1) page = 1;
            if (pageSize < 5) pageSize = 25;

            var query = _context.Mantenimientos
                .AsNoTracking()
                .Include(m => m.Bodega)
                .AsQueryable();

            if (bodegaId.HasValue && bodegaId.Value > 0)
                query = query.Where(m => m.IdBodega == bodegaId.Value);

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                var t = tipo.Trim();
                query = query.Where(m => m.TipoMantenimiento != null && EF.Functions.Like(m.TipoMantenimiento, $"%{t}%"));
            }

            if (!string.IsNullOrWhiteSpace(empresa))
            {
                var e = empresa.Trim();
                query = query.Where(m => m.EmpresaResponsable != null && EF.Functions.Like(m.EmpresaResponsable, $"%{e}%"));
            }

            if (desde.HasValue)
                query = query.Where(m => m.FechaMantenimiento >= desde.Value.Date);

            if (hasta.HasValue)
                query = query.Where(m => m.FechaMantenimiento <= hasta.Value.Date.AddDays(1).AddTicks(-1));

            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim();
                query = query.Where(m =>
                    (m.ComentariosAdministracion != null && EF.Functions.Like(m.ComentariosAdministracion, $"%{s}%")) ||
                    (m.TipoMantenimiento != null && EF.Functions.Like(m.TipoMantenimiento, $"%{s}%")) ||
                    (m.EmpresaResponsable != null && EF.Functions.Like(m.EmpresaResponsable, $"%{s}%")) ||
                    (m.Bodega != null && (
                        EF.Functions.Like(m.Bodega.Nombre, $"%{s}%") ||
                        (m.Bodega.Ubicacion != null && EF.Functions.Like(m.Bodega.Ubicacion, $"%{s}%"))
                    ))
                );
            }

            // Para filtros de selects en la vista
            ViewBag.BodegasFiltro = await _context.Bodegas
                .AsNoTracking()
                .OrderBy(b => b.Nombre)
                .Select(b => new { b.Id, Nombre = b.Nombre + " - " + b.Ubicacion })
                .ToListAsync();

            // Mantener valores en la UI
            ViewBag.FiltroBodegaId = bodegaId;
            ViewBag.FiltroTipo = tipo;
            ViewBag.FiltroEmpresa = empresa;
            ViewBag.FiltroDesde = desde?.ToString("yyyy-MM-dd");
            ViewBag.FiltroHasta = hasta?.ToString("yyyy-MM-dd");
            ViewBag.FiltroQ = q;

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(m => m.FechaMantenimiento)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new PagedResult<Mantenimiento>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };

            return View(vm);
        }

        // =========================================================
        // GET: Mantenimientos/Details/5
        // =========================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var mantenimiento = await _context.Mantenimientos
                .AsNoTracking()
                .Include(m => m.Bodega)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mantenimiento == null) return NotFound();

            return View(mantenimiento);
        }

        // =========================================================
        // GET: Mantenimientos/Create
        // =========================================================
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Create()
        {
            await CargarBodegasAsync();
            return View();
        }

        // =========================================================
        // POST: Mantenimientos/Create
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Create([Bind("Id,FechaMantenimiento,TipoMantenimiento,Costo,EmpresaResponsable,IdBodega,ComentariosAdministracion")] Mantenimiento mantenimiento)
        {
            // Validaciones explícitas y claras
            if (mantenimiento.IdBodega <= 0)
                ModelState.AddModelError(nameof(Mantenimiento.IdBodega), "Debe seleccionar una bodega.");

            if (mantenimiento.Costo < 0)
                ModelState.AddModelError(nameof(Mantenimiento.Costo), "El costo no puede ser negativo.");

            if (!string.IsNullOrWhiteSpace(mantenimiento.TipoMantenimiento) && mantenimiento.TipoMantenimiento.Length > 100)
                ModelState.AddModelError(nameof(Mantenimiento.TipoMantenimiento), "Máximo 100 caracteres.");

            if (mantenimiento.IdBodega > 0 &&
                !await _context.Bodegas.AnyAsync(b => b.Id == mantenimiento.IdBodega))
            {
                ModelState.AddModelError(nameof(Mantenimiento.IdBodega), "La bodega seleccionada no existe.");
            }

            if (!ModelState.IsValid)
            {
                await CargarBodegasAsync(mantenimiento.IdBodega);
                return View(mantenimiento);
            }

            _context.Add(mantenimiento);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // GET: Mantenimientos/Edit/5
        // =========================================================
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var mantenimiento = await _context.Mantenimientos.FindAsync(id);
            if (mantenimiento == null) return NotFound();

            await CargarBodegasAsync(mantenimiento.IdBodega);
            return View(mantenimiento);
        }

        // =========================================================
        // POST: Mantenimientos/Edit/5
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FechaMantenimiento,TipoMantenimiento,Costo,EmpresaResponsable,IdBodega,ComentariosAdministracion")] Mantenimiento mantenimiento)
        {
            if (id != mantenimiento.Id) return NotFound();

            if (mantenimiento.IdBodega <= 0)
                ModelState.AddModelError(nameof(Mantenimiento.IdBodega), "Debe seleccionar una bodega.");

            if (mantenimiento.Costo < 0)
                ModelState.AddModelError(nameof(Mantenimiento.Costo), "El costo no puede ser negativo.");

            if (mantenimiento.IdBodega > 0 &&
                !await _context.Bodegas.AnyAsync(b => b.Id == mantenimiento.IdBodega))
            {
                ModelState.AddModelError(nameof(Mantenimiento.IdBodega), "La bodega seleccionada no existe.");
            }

            if (!ModelState.IsValid)
            {
                await CargarBodegasAsync(mantenimiento.IdBodega);
                return View(mantenimiento);
            }

            try
            {
                _context.Update(mantenimiento);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MantenimientoExists(mantenimiento.Id)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // GET: Mantenimientos/Delete/5
        // =========================================================
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var mantenimiento = await _context.Mantenimientos
                .AsNoTracking()
                .Include(m => m.Bodega)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mantenimiento == null) return NotFound();

            return View(mantenimiento);
        }

        // =========================================================
        // POST: Mantenimientos/Delete/5
        // =========================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mantenimiento = await _context.Mantenimientos.FindAsync(id);
            if (mantenimiento != null)
            {
                _context.Mantenimientos.Remove(mantenimiento);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ===================== Helpers =====================
        private async Task CargarBodegasAsync(int? seleccionada = null)
        {
            var bodegas = await _context.Bodegas
                .AsNoTracking()
                .OrderBy(b => b.Nombre)
                .Select(b => new { b.Id, Nombre = b.Nombre + " - " + b.Ubicacion })
                .ToListAsync();

            ViewBag.IdBodega = new SelectList(bodegas, "Id", "Nombre", seleccionada);
        }

        private bool MantenimientoExists(int id) =>
            _context.Mantenimientos.Any(e => e.Id == id);
    }
}
