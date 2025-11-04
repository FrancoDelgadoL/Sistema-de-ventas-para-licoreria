using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ezel_Market.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Ezel_Market.Data; // <-- 1. AÑADIR: Para que reconozca tu DbContext
using Microsoft.EntityFrameworkCore; // <-- 2. AÑADIR: Para que funcione .Include() y .ToListAsync()

namespace Ezel_Market.Controllers
{
    public class ClienteController : Controller
    {
        private readonly ILogger<ClienteController> _logger;
        private readonly UserManager<Usuarios> _userManager;
        private readonly SignInManager<Usuarios> _signInManager;
        
        // --- 3. AÑADIR: El campo para guardar el DbContext ---
        private readonly ApplicationDbContext _context;

        public ClienteController(
            ILogger<ClienteController> logger,
            UserManager<Usuarios> userManager,
            SignInManager<Usuarios> signInManager,
            ApplicationDbContext context) // <-- 4. AÑADIR: Pide el DbContext aquí
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context; // <-- 5. AÑADIR: Asigna el DbContext
        }        

        // --- 6. MODIFICAR: El método Index para jalar los datos ---
        public async Task<IActionResult> Index()
        {
            // Esta es la consulta que jala los productos
            var productos = await _context.Productos  // De la tabla Productos
                .Include(p => p.Categoria)           // Incluye la info de Categoria
                .ToListAsync();                      // Tráelos como una lista

            // Pasa la lista de productos a la vista
            return View(productos);
        }
    }
}