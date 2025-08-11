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

        // GET: Bodegas (con filtros)
        public async Task<IActionResult> Index(string nombre, string ubicacion, string complejo, string estado)
        {
            // Mantener valores en la vista
            ViewBag.FiltroNombre = nombre;
            ViewBag.FiltroUbicacion = ubicacion;
            ViewBag.FiltroComplejo = complejo;
            ViewBag.FiltroEstado = estado;

            var q = _context.Bodegas.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
                q = q.Where(b => b.Nombre.Contains(nombre));

            if (!string.IsNullOrWhiteSpace(ubicacion))
                q = q.Where(b => b.Ubicacion.Contains(ubicacion));

            if (!string.IsNullOrWhiteSpace(complejo))
                q = q.Where(b => b.Complejo != null && b.Complejo.Contains(complejo));

            if (!string.IsNullOrWhiteSpace(estado))
                q = q.Where(b => b.Estado == estado); // collation de SQL Server suele ser case-insensitive

            q = q.OrderBy(b => b.Nombre);

            var lista = await q.ToListAsync();
            return View(lista);
        }

        // GET: Bodegas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var bodega = await _context.Bodegas
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(m => m.Id == id);
            if (bodega == null) return NotFound();

            return View(bodega);
        }

        // GET: Bodegas/Create
        [AuthorizeRol("Administrador")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Bodegas/Create
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

        // GET: Bodegas/Edit/5
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var bodega = await _context.Bodegas.FindAsync(id);
            if (bodega == null) return NotFound();

            return View(bodega);
        }

        // POST: Bodegas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Ubicacion,Complejo,Estado,AreaM2,PrecioAlquilerPorM2")] Bodega bodega)
        {
            if (id != bodega.Id) return NotFound();

            // La columna Precio es calculada en DB
            ModelState.Remove("Precio");

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

                // Asegurar que EF NO intente escribir la calculada
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

        // GET: Bodegas/Delete/5
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

        // POST: Bodegas/Delete/5
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
