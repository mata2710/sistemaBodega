using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaBodega.Data;
using SistemaBodega.Models;
using SistemaBodega.Filters; // filtro personalizado
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SistemaBodega.Controllers
{
    public class ClientesController : Controller
    {
        private readonly SistemaBodegaContext _context;

        public ClientesController(SistemaBodegaContext context)
        {
            _context = context;
        }

        // ============================
        // LISTA (ACTIVOS)
        // ============================
        // ?q=...&page=1&pageSize=10  (compat: nombre/cedula/representante)
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

            // Solo ACTIVOS
            var query = _context.Clientes
                .AsNoTracking()
                .Where(c => c.IsActive)
                .AsQueryable();

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

        // ============================
        // LISTA (INACTIVOS)
        // ============================
        [HttpGet]
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Inactivos(
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

            // Ignoramos filtros globales si los agregas en el DbContext (opcional),
            // pero además filtramos !IsActive explícitamente.
            var query = _context.Clientes
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => !c.IsActive)
                .AsQueryable();

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

            ViewBag.q = q ?? string.Empty;
            ViewBag.FiltroNombre = nombre;
            ViewBag.FiltroCedula = cedula;
            ViewBag.FiltroRepresentante = representante;

            return View(vm);
        }

        // ============================
        // DETALLES (solo activos por defecto)
        // ============================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

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

            // Activo por defecto
            cliente.IsActive = true;

            _context.Add(cliente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Clientes/Edit/5  (permitimos abrir inactivos también)
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Identificacion,Telefono,TelefonoSecundario,Email,RepresentanteLegal,Actividad,IsActive")] Cliente cliente)
        {
            if (id != cliente.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(cliente);

            // Solo actualizamos campos permitidos; si no quieres que editen IsActive desde el form, quítalo del Bind y no lo toques aquí.
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

            // Redirige según estado
            if (cliente.IsActive)
                return RedirectToAction(nameof(Index));
            else
                return RedirectToAction(nameof(Inactivos));
        }

        // ============================
        // DESACTIVAR (Soft-delete)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Desactivar(int id)
        {
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
            if (cliente == null)
            {
                TempData["Mensaje"] = "Cliente no encontrado o ya inactivo.";
                return RedirectToAction(nameof(Index));
            }

            // Actor desde Session (como hicimos en AdminController)
            var actor = HttpContext.Session.GetString("UsuarioNombre")
                       ?? HttpContext.Session.GetString("UsuarioCorreo")
                       ?? User?.Identity?.Name
                       ?? "Desconocido";

            cliente.IsActive = false;
            // Estas 2 líneas se aplican solo si agregas estas columnas en BD/modelo:
            cliente.DeactivatedAt = System.DateTime.UtcNow;
            cliente.DeactivatedBy = actor;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Cliente desactivado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ============================
        // ACTIVAR
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRol("Administrador")]
        public async Task<IActionResult> Activar(int id)
        {
            var cliente = await _context.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                TempData["Mensaje"] = "Cliente no encontrado.";
                return RedirectToAction(nameof(Inactivos));
            }

            cliente.IsActive = true;
            // limpiar campos opcionales
            cliente.DeactivatedAt = null;
            cliente.DeactivatedBy = null;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Cliente reactivado correctamente.";
            return RedirectToAction(nameof(Inactivos));
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }

        // ============ ✂️ Eliminado flujo de borrado físico ============
        // GET: Clientes/Delete/5  → ya no se usa
        // POST: Clientes/Delete/5 → ya no se usa
    }
}

