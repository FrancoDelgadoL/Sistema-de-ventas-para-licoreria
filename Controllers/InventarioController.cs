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
using System.Security.Claims;

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
                .Include(i => i.CategoriaInventarios)
                    .ThenInclude(ci => ci.Categoria)
                .ToListAsync();

            return View(lista);
        }

        // GET: Inventario/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var inventario = await _context.Inventarios
                .Include(i => i.CategoriaInventarios)
                    .ThenInclude(ci => ci.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inventario == null)
                return NotFound();

            return View(inventario);
        }

        // GET: Inventario/Create
        public IActionResult Create()
        {
            ViewBag.Categorias = new SelectList(_context.Categorias, "Id", "Nombre");
            return View();
        }

        // POST: Inventario/Create (con imagen)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreProducto,Cantidad,PrecioCompra,PrecioVentaMinorista,PrecioVentaMayorista,FechaIngreso")] Inventario inventario, IFormFile ImagenArchivo, List<int> CategoriasSeleccionadas)  // ✅ AGREGADO CategoriasSeleccionadas
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

                    inventario.Imagen = "/imagenes/" + nombreArchivo;
                }

                _context.Add(inventario);
                await _context.SaveChangesAsync();

                // ✅ AGREGAR RELACIONES CON CATEGORÍAS
                if (CategoriasSeleccionadas != null && CategoriasSeleccionadas.Any())
                {
                    foreach (var categoriaId in CategoriasSeleccionadas)
                    {
                        var categoriaInventario = new CategoriaInventario
                        {
                            InventarioId = inventario.Id,
                            CategoriaId = categoriaId
                        };
                        _context.CategoriaInventarios.Add(categoriaInventario);
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categorias = new SelectList(_context.Categorias.ToList(), "Id", "Nombre");
            return View(inventario);
        }

        // GET: Inventario/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            // ✅ CORREGIDO: Agregar Include para las relaciones
            var inventario = await _context.Inventarios
                .Include(i => i.CategoriaInventarios)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventario == null)
                return NotFound();

            ViewBag.Categorias = new SelectList(_context.Categorias.ToList(), "Id", "Nombre");
            
            // ✅ PASAR CATEGORÍAS SELECCIONADAS A LA VISTA
            ViewBag.CategoriasSeleccionadas = inventario.CategoriaInventarios?.Select(ci => ci.CategoriaId).ToList() ?? new List<int>();
            
            return View(inventario);
        }

        // POST: Inventario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreProducto,Cantidad,PrecioCompra,PrecioVentaMinorista,PrecioVentaMayorista,FechaIngreso,Imagen")] Inventario inventario, IFormFile ImagenArchivo, List<int> CategoriasSeleccionadas)  // ✅ AGREGADO
        {
            if (id != inventario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // 1. OBTENEMOS EL ESTADO ANTIGUO (¡SIN RASTREO!)
                var inventarioAntiguo = await _context.Inventarios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (inventarioAntiguo == null)
                {
                    return NotFound();
                }

                try
                {
                    // 2. LÓGICA DE HISTORIAL
                    int cantidadAntigua = inventarioAntiguo.Cantidad;
                    int cantidadNueva = inventario.Cantidad;

                    if (cantidadAntigua != cantidadNueva)
                    {
                        string usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        var historial = new HistorialInventario
                        {
                            InventarioId = inventario.Id,
                            CantidadAnterior = cantidadAntigua,
                            CantidadNueva = cantidadNueva,
                            TipoMovimiento = "Edición Manual",
                            Fecha = DateTime.Now,
                            UsuarioId = usuarioId
                        };
                        _context.HistorialInventarios.Add(historial);
                    }

                    // 3. LÓGICA DE IMAGEN
                    if (ImagenArchivo != null && ImagenArchivo.Length > 0)
                    {
                        string carpetaImagenes = Path.Combine(_webHostEnvironment.WebRootPath, "imagenes");
                        if (!Directory.Exists(carpetaImagenes)) Directory.CreateDirectory(carpetaImagenes);
                        string nombreArchivo = Path.GetFileName(ImagenArchivo.FileName);
                        string rutaArchivo = Path.Combine(carpetaImagenes, nombreArchivo);
                        using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                        {
                            await ImagenArchivo.CopyToAsync(stream);
                        }
                        inventario.Imagen = "/imagenes/" + nombreArchivo;
                    }
                    else
                    {
                        inventario.Imagen = inventarioAntiguo.Imagen;
                    }

                    // ✅ 4. ACTUALIZAR RELACIONES CON CATEGORÍAS
                    var relacionesExistentes = _context.CategoriaInventarios
                        .Where(ci => ci.InventarioId == id);
                    _context.CategoriaInventarios.RemoveRange(relacionesExistentes);

                    if (CategoriasSeleccionadas != null && CategoriasSeleccionadas.Any())
                    {
                        foreach (var categoriaId in CategoriasSeleccionadas)
                        {
                            _context.CategoriaInventarios.Add(new CategoriaInventario
                            {
                                InventarioId = inventario.Id,
                                CategoriaId = categoriaId
                            });
                        }
                    }

                    // 5. ACTUALIZAMOS LA ENTIDAD
                    _context.Update(inventario);
                    await _context.SaveChangesAsync();
                    
                    TempData["MensajeExito"] = "¡Producto actualizado exitosamente!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Error al guardar cambios: " + (ex.InnerException?.Message ?? ex.Message));
                }
            }

            ViewBag.Categorias = new SelectList(_context.Categorias.ToList(), "Id", "Nombre");
            return View(inventario);
        }

        // GET: Inventario/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var inventario = await _context.Inventarios
                .Include(i => i.CategoriaInventarios)
                    .ThenInclude(ci => ci.Categoria)
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
            // ✅ CORREGIDO: Eliminar primero las relaciones
            var inventario = await _context.Inventarios
                .Include(i => i.CategoriaInventarios)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventario != null)
            {
                // ELIMINAR PRIMERO LAS RELACIONES
                var relaciones = _context.CategoriaInventarios.Where(ci => ci.InventarioId == id);
                _context.CategoriaInventarios.RemoveRange(relaciones);
                
                _context.Inventarios.Remove(inventario);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool InventarioExists(int id)
        {
            return _context.Inventarios.Any(e => e.Id == id);
        }

        // GET: Inventario/ObtenerHistorialGeneralPartial
        public async Task<IActionResult> ObtenerHistorialGeneralPartial()
        {
            var historialCompleto = await _context.HistorialInventarios
                .Include(h => h.Inventario)
                .OrderByDescending(h => h.Fecha)
                .ToListAsync();

            return PartialView("_HistorialGeneralPartial", historialCompleto);
        }
    }
}