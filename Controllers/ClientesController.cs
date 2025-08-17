using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaBodega.Data;
using SistemaBodega.Models;
using SistemaBodega.Filters; // filtro personalizado
using System.Collections.Generic;

namespace SistemaBodega.Controllers
{
    public class ClientesController : Controller
    {
        private readonly SistemaBodegaContext _context;

        public ClientesController(SistemaBodegaContext context)
        {
            _context = context;
        }

        // GET: Clientes (búsqueda unificada + paginación)
        // ?q=...&page=1&pageSize=10
        // Compatibilidad: también acepta nombre/cedula/representante
        [HttpGet]
        public async Task<IActionResult> Index(
            string? q,
            string? nombre,
            string? cedula,
            string? representante,
            int page = 1,
            int pageSize = 10)
        {
            var allowedPageSizes = new HashSet<int> { 5, 10, 25, 50, 100 };
            if (!allowedPageSizes.Contains(pageSize)) pageSize = 10;
            if (page < 1) page = 1;

            var query = _context.Clientes.AsNoTracking().AsQueryable();

            // Búsqueda unificada (q)
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(c =>
                    EF.Functions.Like(c.Nombre, $"%{term}%") ||
                    (c.Identificacion != null && EF.Functions.Like(c.Identificacion, $"%{term}%")) ||
                    (c.Email != null && EF.Functions.Like(c.Email, $"%{term}%")) ||
                    (c.RepresentanteLegal != null && EF.Functions.Like(c.RepresentanteLegal, $"%{term}%")) ||
                    (c.Telefono != null && EF.Functions.Like(c.Telefono, $"%{term}%")) ||
                    (c.TelefonoSecundario != null && EF.Functions.Like(c.TelefonoSecundario, $"%{term}%"))
                );
            }

            // Compat: filtros específicos
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var n = nombre.Trim();
                query = query.Where(c => EF.Functions.Like(c.Nombre, $"%{n}%"));
            }

            if (!string.IsNullOrWhiteSpace(cedula))
            {
                var cdl = cedula.Trim();
                query = query.Where(c => c.Identificacion != null && EF.Functions.Like(c.Identificacion, $"%{cdl}%"));
            }

            if (!string.IsNullOrWhiteSpace(representante))
            {
                var rep = representante.Trim();
                query = query.Where(c => c.RepresentanteLegal != null && EF.Functions.Like(c.RepresentanteLegal, $"%{rep}%"));
            }

            // Totales y paginación
            var total = await query.CountAsync();
            var totalPages = total == 0 ? 1 : (int)System.Math.Ceiling((decimal)total / pageSize);
            if (page > totalPages) page = totalPages;

            var items = await query
                .OrderBy(c => c.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new PagedResult<Cliente>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };

            // Para la vista
            ViewBag.q = q ?? string.Empty;
            ViewBag.FiltroNombre = nombre;
            ViewBag.FiltroCedula = cedula;
            ViewBag.FiltroRepresentante = representante;

            return View(vm);
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
