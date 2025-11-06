using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ezel_Market.Data;
using Ezel_Market.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ezel_Market.Controllers
{
    public class InventarioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventarioController(ApplicationDbContext context)
        {
            _context = context;
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

        // POST: Inventario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreProducto,CategoriasId,Cantidad,PrecioCompra,PrecioVenta,FechaIngreso")] Inventario inventario)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inventario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
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

        // ✅ POST: Inventario/Edit/5 (CORREGIDO)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreProducto,CategoriasId,Cantidad,PrecioCompra,PrecioVenta,FechaIngreso")] Inventario inventario)
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
