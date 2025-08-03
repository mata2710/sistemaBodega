using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using SistemaBodega.Data;
using SistemaBodega.Models;
using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

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
        public IActionResult Index()
        {
            var lista = _context.CarruselImagenes.ToList();
            return View(lista);
        }

        // GET: CarruselImagen/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CarruselImagen/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CarruselImagen model, IFormFile imagen)
        {
            if (imagen == null || imagen.Length == 0)
            {
                ModelState.AddModelError("", "Debe seleccionar una imagen.");
                return View(model);
            }

            // Validar que sea una imagen
            if (!imagen.ContentType.StartsWith("image/"))
            {
                ModelState.AddModelError("", "Solo se permiten archivos de imagen.");
                return View(model);
            }

            // Asignar título por defecto si está vacío o nulo
            if (string.IsNullOrWhiteSpace(model.Titulo))
            {
                model.Titulo = "Imagen sin título";
            }

            string carpetaDestino = "img/carrusel";
            string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
            string rutaCarpeta = Path.Combine(_env.WebRootPath, carpetaDestino);

            if (!Directory.Exists(rutaCarpeta))
                Directory.CreateDirectory(rutaCarpeta);

            string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                imagen.CopyTo(stream);
            }

            // Ruta que se guarda en la base de datos
            model.RutaImagen = Path.Combine(carpetaDestino, nombreArchivo).Replace("\\", "/");

            _context.CarruselImagenes.Add(model);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // GET: CarruselImagen/Edit/5
        public IActionResult Edit(int id)
        {
            var imagen = _context.CarruselImagenes.Find(id);
            if (imagen == null) return NotFound();
            return View(imagen);
        }

        // POST: CarruselImagen/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CarruselImagen model)
        {
            var original = _context.CarruselImagenes.Find(id);
            if (original == null) return NotFound();

            // Si el título viene vacío, ponerle uno por defecto
            original.Titulo = string.IsNullOrWhiteSpace(model.Titulo) ? "Imagen sin título" : model.Titulo;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // GET: CarruselImagen/Delete/5
        public IActionResult Delete(int id)
        {
            var imagen = _context.CarruselImagenes.Find(id);
            if (imagen == null) return NotFound();
            return View(imagen);
        }

        // POST: CarruselImagen/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var imagen = _context.CarruselImagenes.Find(id);
            if (imagen == null) return NotFound();

            // Eliminar archivo físico si existe
            var rutaCompleta = Path.Combine(_env.WebRootPath, imagen.RutaImagen);
            if (System.IO.File.Exists(rutaCompleta))
                System.IO.File.Delete(rutaCompleta);

            _context.CarruselImagenes.Remove(imagen);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}

