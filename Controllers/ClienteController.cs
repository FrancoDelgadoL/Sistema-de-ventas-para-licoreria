using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ezel_Market.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Ezel_Market.Data; 
using Microsoft.EntityFrameworkCore; 

namespace Ezel_Market.Controllers
{
    public class ClienteController : Controller
    {
        private readonly ILogger<ClienteController> _logger;
        private readonly UserManager<Usuarios> _userManager;
        private readonly SignInManager<Usuarios> _signInManager;
        private readonly ApplicationDbContext _context;

        public ClienteController(
            ILogger<ClienteController> logger,
            UserManager<Usuarios> userManager,
            SignInManager<Usuarios> signInManager,
            ApplicationDbContext context) 
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context; 
        } 

        // --- 6. MÉTODO INDEX TOTALMENTE ACTUALIZADO ---
        public async Task<IActionResult> Index()
        {
            // 1. Crear la instancia del ViewModel
            var viewModel = new ClienteCatalogoViewModel();

            // 2. Llenar la lista de Productos
            viewModel.Productos = await _context.Inventario 
                .Include(p => p.Categorias) // Incluye la info de Categoria
                .OrderBy(p => p.NombreProducto) // Ordena alfabéticamente
                .ToListAsync();                 

            // 3. Llenar la lista de Categorías para el filtro
            viewModel.Categorias = await _context.Categorias
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            // 4. Pasa el ViewModel (que contiene AMBAS listas) a la vista
            return View(viewModel);
        }
        
        // ... (Aquí podrían ir tus otras acciones como Perfil, etc.) ...
    }
}