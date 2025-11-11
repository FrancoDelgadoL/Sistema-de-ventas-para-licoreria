using Microsoft.AspNetCore.Mvc;
using Ezel_Market.Models;
using Ezel_Market.Data; // Tu DbContext
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
            var reviews = from r in _context.Reviews
                          select r;

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

            ViewData["CurrentSort"] = sortOrder;
            return View(reviews.ToList());
        }

        // POST: /Reviews/AddReview
        [HttpPost]
        public IActionResult AddReview(Review newReview)
        {
            if (ModelState.IsValid)
            {
                // Asignamos el usuario logueado y la fecha actual
                newReview.Usuario = User.Identity.Name; 
                newReview.Fecha = System.DateTime.Now;

                _context.Reviews.Add(newReview);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
