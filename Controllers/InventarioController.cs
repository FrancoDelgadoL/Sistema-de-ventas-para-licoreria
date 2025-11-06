using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ezel_Market.Data;
using Ezel_Market.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace Ezel_Market.Controllers
{
    public class InventarioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InventarioController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Inventario
        public async Task<IActionResult> Index()
        {
            var lista = await _context.Inventarios
                .Include(i => i.Categoria)
                .ToListAsync();

            return View(lista);
        }

        // GET: Inventario/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var inventario = await _context.Inventarios
                .Include(i => i.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inventario == null)
                return NotFound();

            return View(inventario);
        }

        // GET: Inventario/Create
        public IActionResult Create()
        {
            ViewBag.Categorias = new SelectList(_context.Categoria, "Id", "Nombre");
            return View();
        }

        // POST: Inventario/Create (con imagen)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreProducto,CategoriasId,Cantidad,PrecioCompra,PrecioVenta,FechaIngreso")] Inventario inventario, IFormFile ImagenArchivo)
        {
            if (ModelState.IsValid)
            {
                // Carpeta donde se guardarán las imágenes
                string carpetaImagenes = Path.Combine(_webHostEnvironment.WebRootPath, "imagenes");

                // Si no existe, se crea
                if (!Directory.Exists(carpetaImagenes))
                {
                    Directory.CreateDirectory(carpetaImagenes);
                }

                // Si el usuario subió una imagen
                if (ImagenArchivo != null && ImagenArchivo.Length > 0)
                {
                    string nombreArchivo = Path.GetFileName(ImagenArchivo.FileName);
                    string rutaArchivo = Path.Combine(carpetaImagenes, nombreArchivo);

                    using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                    {
                        await ImagenArchivo.CopyToAsync(stream);
                    }

                    // Guardar nombre del archivo en el modelo (asegúrate que exista la propiedad Imagen en el modelo)
                    inventario.Imagen = "/imagenes/" + nombreArchivo;
                }

                _context.Add(inventario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categorias = new SelectList(_context.Categoria, "Id", "Nombre", inventario.CategoriasId);
            return View(inventario);
        }

        // GET: Inventario/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var inventario = await _context.Inventarios.FindAsync(id);
            if (inventario == null)
                return NotFound();

            ViewBag.Categorias = new SelectList(_context.Categoria, "Id", "Nombre", inventario.CategoriasId);
            return View(inventario);
        }

        // POST: Inventario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreProducto,CategoriasId,Cantidad,PrecioCompra,PrecioVenta,FechaIngreso,Imagen")] Inventario inventario, IFormFile ImagenArchivo)
        {
            if (id != inventario.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingInventario = await _context.Inventarios.FindAsync(id);
                    if (existingInventario == null)
                        return NotFound();

                    existingInventario.NombreProducto = inventario.NombreProducto;
                    existingInventario.CategoriasId = inventario.CategoriasId;
                    existingInventario.Cantidad = inventario.Cantidad;
                    existingInventario.PrecioCompra = inventario.PrecioCompra;
                    existingInventario.PrecioVenta = inventario.PrecioVenta;
                    existingInventario.FechaIngreso = inventario.FechaIngreso;

                    // Si se sube una nueva imagen, reemplazar la anterior
                    if (ImagenArchivo != null && ImagenArchivo.Length > 0)
                    {
                        string carpetaImagenes = Path.Combine(_webHostEnvironment.WebRootPath, "imagenes");
                        if (!Directory.Exists(carpetaImagenes))
                        {
                            Directory.CreateDirectory(carpetaImagenes);
                        }

                        string nombreArchivo = Path.GetFileName(ImagenArchivo.FileName);
                        string rutaArchivo = Path.Combine(carpetaImagenes, nombreArchivo);

                        using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                        {
                            await ImagenArchivo.CopyToAsync(stream);
                        }

                        existingInventario.Imagen = "/imagenes/" + nombreArchivo;
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Error al guardar cambios: " + (ex.InnerException?.Message ?? ex.Message));
                }
            }

            ViewBag.Categorias = new SelectList(_context.Categoria, "Id", "Nombre", inventario.CategoriasId);
            return View(inventario);
        }

        // GET: Inventario/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var inventario = await _context.Inventarios
                .Include(i => i.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inventario == null)
                return NotFound();

            return View(inventario);
        }

        // POST: Inventario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventario = await _context.Inventarios.FindAsync(id);
            if (inventario != null)
            {
                _context.Inventarios.Remove(inventario);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool InventarioExists(int id)
        {
            return _context.Inventarios.Any(e => e.Id == id);
        }
    }
}
