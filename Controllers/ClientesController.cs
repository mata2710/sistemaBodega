using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaBodega.Data;
using SistemaBodega.Models;
using SistemaBodega.Filters; // ✅ filtro personalizado

namespace SistemaBodega.Controllers
{
    public class ClientesController : Controller
    {
        private readonly SistemaBodegaContext _context;

        public ClientesController(SistemaBodegaContext context)
        {
            _context = context;
        }

        // GET: Clientes
        // Filtros: ?nombre=...&cedula=...&representante=...
        public async Task<IActionResult> Index(string? nombre, string? cedula, string? representante)
        {
            var q = _context.Clientes.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var n = nombre.Trim();
                q = q.Where(c => EF.Functions.Like(c.Nombre, $"%{n}%"));
            }

            if (!string.IsNullOrWhiteSpace(cedula))
            {
                var cdl = cedula.Trim();
                q = q.Where(c => EF.Functions.Like(c.Identificacion, $"%{cdl}%"));
            }

            if (!string.IsNullOrWhiteSpace(representante))
            {
                var rep = representante.Trim();
                q = q.Where(c => c.RepresentanteLegal != null && EF.Functions.Like(c.RepresentanteLegal, $"%{rep}%"));
            }

            ViewBag.FiltroNombre = nombre;
            ViewBag.FiltroCedula = cedula;
            ViewBag.FiltroRepresentante = representante;

            var clientes = await q.OrderBy(c => c.Nombre).ToListAsync();
            return View(clientes);
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // GET: Clientes/Create
        [AuthorizeRol("Administrador")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Create([Bind("Nombre,Identificacion,Telefono,TelefonoSecundario,Email,RepresentanteLegal,Actividad")] Cliente cliente)
        {
            if (!ModelState.IsValid)
                return View(cliente);

            _context.Add(cliente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Clientes/Edit/5
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Identificacion,Telefono,TelefonoSecundario,Email,RepresentanteLegal,Actividad")] Cliente cliente)
        {
            if (id != cliente.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(cliente);

            try
            {
                _context.Update(cliente);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClienteExists(cliente.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Clientes/Delete/5
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }
    }
}

