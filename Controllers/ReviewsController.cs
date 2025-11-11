using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ezel_Market.Models;
using Ezel_Market.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Ezel_Market.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Reviews/Index
        public IActionResult Index(string sortOrder)
        {
            var reviews = _context.Reviews.Include(r => r.Inventario).AsQueryable();

            switch (sortOrder)
            {
                case "best":
                    reviews = reviews.OrderByDescending(r => r.Calificacion);
                    break;
                case "recent":
                default:
                    reviews = reviews.OrderByDescending(r => r.Fecha);
                    break;
            }

            // Cargar bebidas para el formulario
            ViewBag.Bebidas = new SelectList(_context.Inventarios.OrderBy(i => i.NombreProducto), "Id", "NombreProducto");

            ViewData["CurrentSort"] = sortOrder;
            return View(reviews.ToList());
        }

        // POST: /Reviews/AddReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddReview(Review newReview)
        {
            if (ModelState.IsValid)
            {
                newReview.Usuario = User.Identity.Name;
                newReview.Fecha = System.DateTime.Now;

                _context.Reviews.Add(newReview);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            // Si hay error, recargar bebidas y lista de reseñas
            ViewBag.Bebidas = new SelectList(_context.Inventarios.OrderBy(i => i.NombreProducto), "Id", "NombreProducto");
            var reviews = _context.Reviews.Include(r => r.Inventario).ToList();
            return View("Index", reviews);
        }
    }
}
