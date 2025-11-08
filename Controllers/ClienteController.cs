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

        public async Task<IActionResult> Index()
        {
            try
            {
                // 1. Obtener TODAS las categorías desde la BD
                var categorias = await _context.Categoria
                    .OrderBy(c => c.Nombre)
                    .Select(c => c.Nombre)
                    .ToListAsync();

                ViewBag.Categorias = categorias;

                // 2. Obtener productos con stock desde la BD
                var productos = await _context.Inventario
                    .Include(p => p.Categoria)
                    .Where(p => p.Cantidad > 0)
                    .Select(p => new ProductoVistaModel
                    {
                        Id = p.Id,
                        Nombre = p.NombreProducto,
                        Precio = p.PrecioVenta,
                        Imagen = p.Imagen ?? "",
                        Categoria = p.Categoria.Nombre,
                        CantidadStock = p.Cantidad,
                        Marca = p.Marca ?? "",
                        GradoAlcohol = p.GradoAlcohol
                    })
                    .ToListAsync();

                return View(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar productos");
                ViewBag.Categorias = new List<string>();
                return View(new List<ProductoVistaModel>());
            }
        }

        public IActionResult Perfil()
        {
            return View();
        }
    }
}