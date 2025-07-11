using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaBodega.Data;
using SistemaBodega.Models;

namespace SistemaBodega.Controllers
{
    public class AlquileresController : Controller
    {
        private readonly SistemaBodegaContext _context;

        public AlquileresController(SistemaBodegaContext context)
        {
            _context = context;
        }

        // GET: Alquileres
        public async Task<IActionResult> Index()
        {
            var sistemaBodegaContext = _context.Alquileres
                .Include(a => a.Bodega)
                .Include(a => a.Cliente);
            return View(await sistemaBodegaContext.ToListAsync());
        }

        // GET: Alquileres/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var alquiler = await _context.Alquileres
                .Include(a => a.Bodega)
                .Include(a => a.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (alquiler == null) return NotFound();
            return View(alquiler);
        }

        // GET: Alquileres/Create
        public IActionResult Create()
        {
            ViewData["BodegaId"] = new SelectList(_context.Bodegas, "Id", "Nombre");
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre");
            return View();
        }

        // POST: Alquileres/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClienteId,BodegaId,FechaInicio,FechaFin,RenovacionAutomatica,Activo")] Alquiler alquiler)
        {
            if (ModelState.IsValid)
            {
                _context.Add(alquiler);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BodegaId"] = new SelectList(_context.Bodegas, "Id", "Nombre", alquiler.BodegaId);
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", alquiler.ClienteId);
            return View(alquiler);
        }

        // GET: Alquileres/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var alquiler = await _context.Alquileres.FindAsync(id);
            if (alquiler == null) return NotFound();

            ViewData["BodegaId"] = new SelectList(_context.Bodegas, "Id", "Nombre", alquiler.BodegaId);
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", alquiler.ClienteId);
            return View(alquiler);
        }

        // POST: Alquileres/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,BodegaId,FechaInicio,FechaFin,RenovacionAutomatica,Activo")] Alquiler alquiler)
        {
            if (id != alquiler.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alquiler);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlquilerExists(alquiler.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BodegaId"] = new SelectList(_context.Bodegas, "Id", "Nombre", alquiler.BodegaId);
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", alquiler.ClienteId);
            return View(alquiler);
        }

        // GET: Alquileres/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var alquiler = await _context.Alquileres
                .Include(a => a.Bodega)
                .Include(a => a.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (alquiler == null) return NotFound();

            return View(alquiler);
        }

        // POST: Alquileres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alquiler = await _context.Alquileres.FindAsync(id);
            if (alquiler != null)
                _context.Alquileres.Remove(alquiler);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlquilerExists(int id)
        {
            return _context.Alquileres.Any(e => e.Id == id);
        }
    }
}

