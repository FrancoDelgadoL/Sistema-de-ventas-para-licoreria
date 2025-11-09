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
                .Include(i => i.Categorias)
                .ToListAsync();

            return View(lista);
        }

        // GET: Inventario/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var inventario = await _context.Inventarios
                .Include(i => i.Categorias)
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
        public async Task<IActionResult> Create([Bind("Id,NombreProducto,CategoriasId,Cantidad,PrecioCompra,PrecioVentaMinorista,PrecioVentaMayorista,FechaIngreso")] Inventario inventario, IFormFile ImagenArchivo)
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

            ViewBag.Categorias = new SelectList(_context.Categorias.ToList(), "Id", "Nombre");
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

            ViewBag.Categorias = new SelectList(_context.Categorias.ToList(), "Id", "Nombre");
            return View(inventario);
        }

        // POST: Inventario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreProducto,CategoriasId,Cantidad,PrecioCompra,PrecioVentaMinorista,PrecioVentaMayorista,FechaIngreso,Imagen")] Inventario inventario, IFormFile ImagenArchivo)
        {
            if (id != inventario.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid) // Esto ya incluye tu validación de precios del Modelo
                {
                    // 1. OBTENEMOS EL ESTADO ANTIGUO (¡SIN RASTREO!)
                    var inventarioAntiguo = await _context.Inventarios
                        .AsNoTracking() // <-- Muy importante
                        .FirstOrDefaultAsync(i => i.Id == id);

                    if (inventarioAntiguo == null)
                    {
                        return NotFound();
                    }

                    try
                    {
                        // 2. LÓGICA DE HISTORIAL
                        int cantidadAntigua = inventarioAntiguo.Cantidad;
                        int cantidadNueva = inventario.Cantidad; // La del formulario

                        if (cantidadAntigua != cantidadNueva)
                        {
                            
                            // 👇 ¡AQUÍ ESTÁ LA MAGIA! 👇
                            string usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                            var historial = new HistorialInventario
                            {
                                InventarioId = inventario.Id,
                                CantidadAnterior = cantidadAntigua,
                                CantidadNueva = cantidadNueva,
                                TipoMovimiento = "Edición Manual",
                                Fecha = DateTime.Now,
                                UsuarioId = usuarioId  // <--- AÑADA ESTA LÍNEA
                            };
                            _context.HistorialInventarios.Add(historial); // Añadimos historial
                        }

                        // 3. LÓGICA DE IMAGEN
                        if (ImagenArchivo != null && ImagenArchivo.Length > 0)
                        {
                            // ... (tu código para guardar la imagen) ...
                            string carpetaImagenes = Path.Combine(_webHostEnvironment.WebRootPath, "imagenes");
                            if (!Directory.Exists(carpetaImagenes)) Directory.CreateDirectory(carpetaImagenes);
                            string nombreArchivo = Path.GetFileName(ImagenArchivo.FileName);
                            string rutaArchivo = Path.Combine(carpetaImagenes, nombreArchivo);
                            using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                            {
                                await ImagenArchivo.CopyToAsync(stream);
                            }
                            inventario.Imagen = "/imagenes/" + nombreArchivo; // Asignamos la nueva imagen
                        }
                        else
                        {
                            // Si no se subió imagen, mantenemos la antigua
                            inventario.Imagen = inventarioAntiguo.Imagen;
                        }

                        // 4. ACTUALIZAMOS LA ENTIDAD
                        _context.Update(inventario); // <-- EF se encarga de actualizar todo el objeto

                        // 5. GUARDAMOS TODO (Inventario e Historial)
                        await _context.SaveChangesAsync();
                        
                        TempData["MensajeExito"] = "¡Producto actualizado exitosamente!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateException ex)
                    {
                        ModelState.AddModelError("", "Error al guardar cambios: " + (ex.InnerException?.Message ?? ex.Message));
                    }
                }

                // Si el modelo no es válido
                ViewBag.Categorias = new SelectList(_context.Categorias.ToList(), "Id", "Nombre");
                return View(inventario);
            }

        // GET: Inventario/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var inventario = await _context.Inventarios
                .Include(i => i.Categorias)
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

        // GET: Inventario/ObtenerHistorialGeneralPartial
        public async Task<IActionResult> ObtenerHistorialGeneralPartial()
        {
            // Obtenemos TODO el historial
            var historialCompleto = await _context.HistorialInventarios
                .Include(h => h.Inventario) // <-- Incluimos el producto para saber su nombre
                .OrderByDescending(h => h.Fecha) // Lo ordenamos por fecha
                .ToListAsync();

            // Devolvemos una nueva vista parcial
            return PartialView("_HistorialGeneralPartial", historialCompleto);
        }
    }
}
