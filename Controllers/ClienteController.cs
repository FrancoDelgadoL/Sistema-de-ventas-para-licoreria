using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ezel_Market.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Ezel_Market.Data; 
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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

        // ========== MÉTODO INDEX DEL CATÁLOGO ==========
        public async Task<IActionResult> Index()
        {
            var viewModel = new ClienteCatalogoViewModel();
            viewModel.Productos = await _context.Inventario 
                .Include(p => p.Categoria)
                .Where(p => p.Cantidad > 0) // Solo productos con stock
                .OrderBy(p => p.NombreProducto)
                .ToListAsync();                 

            viewModel.Categorias = await _context.Categoria
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return View(viewModel);
        }

        // ========== MÉTODOS DEL CARRITO ==========

        private string GetUsuarioId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AgregarAlCarrito([FromBody] AgregarAlCarritoModel model)
        {
            try
            {
                var usuarioId = GetUsuarioId();
        
                if (string.IsNullOrEmpty(usuarioId))
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                // VERIFICACIÓN MEJORADA del producto
                var producto = await _context.Inventario
                    .FirstOrDefaultAsync(p => p.Id == model.InventarioId && p.Cantidad > 0); // Solo productos con stock

                if (producto == null)
                {
                    return Json(new { success = false, message = "Producto no disponible o sin stock" });
                }

                // Verificar stock disponible - CORREGIDO
                if (model.Cantidad <= 0 || model.Cantidad > producto.Cantidad)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Cantidad inválida. Stock disponible: {producto.Cantidad} unidades" 
                    });
                }

                // Verificar si el producto ya está en el carrito - CORREGIDO
                var itemExistente = await _context.Carrito
                    .Include(c => c.Inventario) // Incluir el producto para verificar stock
                    .FirstOrDefaultAsync(ci => ci.UsuarioId == usuarioId && ci.InventarioId == model.InventarioId);

                if (itemExistente != null)
                {
                    // Calcular nueva cantidad total
                    var nuevaCantidadTotal = itemExistente.Cantidad + model.Cantidad;
            
                    // Verificar que no exceda el stock - USAR EL PRODUCTO REAL
                    if (nuevaCantidadTotal > producto.Cantidad)
                    {
                        return Json(new { 
                            success = false, 
                            message = $"No puedes agregar más unidades. Stock disponible: {producto.Cantidad}" 
                        });
                    }
            
                    itemExistente.Cantidad = nuevaCantidadTotal;
                    itemExistente.FechaAgregado = DateTime.Now; // Actualizar fecha
                }
                else
                {
                    var nuevoItem = new Carrito
                    {
                        UsuarioId = usuarioId,
                        InventarioId = model.InventarioId,
                        Cantidad = model.Cantidad,
                        FechaAgregado = DateTime.Now
                    };
            _context.Carrito.Add(nuevoItem);
                }

                await _context.SaveChangesAsync();
        
                // Obtener cantidad actualizada del carrito para el contador
                var cantidadEnCarrito = await _context.Carrito
                    .Where(ci => ci.UsuarioId == usuarioId)
                    .SumAsync(ci => ci.Cantidad);
            
                return Json(new { 
                    success = true, 
                    message = "Producto agregado al carrito",
                    cantidadItems = cantidadEnCarrito
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar producto al carrito");
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerCarrito()
        {
            try
            {
                var usuarioId = GetUsuarioId();
                
                if (string.IsNullOrEmpty(usuarioId))
                {
                    return Unauthorized();
                }

                var items = await _context.Carrito
                    .Where(ci => ci.UsuarioId == usuarioId)
                    .Include(ci => ci.Inventario)
                    .ThenInclude(i => i.Categoria)
                    .Select(ci => new
                    {
                        id = ci.Id,
                        productoId = ci.InventarioId,
                        nombre = ci.Inventario.NombreProducto,
                        precio = ci.Inventario.PrecioVentaMinorista,
                        cantidad = ci.Cantidad,
                        imagenUrl = ci.Inventario.Imagen,
                        categoria = ci.Inventario.Categoria.Nombre,
                        subtotal = ci.Inventario.PrecioVentaMinorista * ci.Cantidad,
                        stockDisponible = ci.Inventario.Cantidad
                    })
                    .ToListAsync();

                var subtotal = items.Sum(i => i.subtotal);
                var cantidadItems = items.Sum(i => i.cantidad);

                return Json(new
                {
                    items,
                    subtotal,
                    cantidadItems
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener carrito");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ActualizarCantidad([FromBody] ActualizarCantidadModel model)
        {
            try
            {
                var item = await _context.Carrito
                    .Include(ci => ci.Inventario)
                    .FirstOrDefaultAsync(ci => ci.Id == model.ItemId);
        
                if (item == null)
                {
                    return Json(new { success = false, message = "Item no encontrado" });
                }

                // Verificar stock disponible
                if (model.NuevaCantidad > item.Inventario.Cantidad)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Stock insuficiente. Máximo disponible: {item.Inventario.Cantidad}" 
                    });
                }

                if (model.NuevaCantidad <= 0)
                {
                    _context.Carrito.Remove(item);
                }
                else
                {
                    item.Cantidad = model.NuevaCantidad;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cantidad actualizada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cantidad");
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EliminarDelCarrito([FromBody] EliminarDelCarritoModel model)
        {
            try
            {
                var item = await _context.Carrito
                    .FirstOrDefaultAsync(ci => ci.Id == model.ItemId);
                
                if (item == null)
                {
                    return Json(new { success = false, message = "Item no encontrado" });
                }

                _context.Carrito.Remove(item);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Producto eliminado del carrito" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar del carrito");
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> LimpiarCarrito()
        {
            try
            {
                var usuarioId = GetUsuarioId();
                
                if (string.IsNullOrEmpty(usuarioId))
                {
                    return Unauthorized();
                }

                var items = await _context.Carrito
                    .Where(ci => ci.UsuarioId == usuarioId)
                    .ToListAsync();

                _context.Carrito.RemoveRange(items);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Carrito limpiado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar carrito");
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }

        // ========== MÉTODOS DE CUPONES ==========

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AplicarCupon([FromBody] AplicarCuponModel model)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var cupon = await _context.Cupones
                    .FirstOrDefaultAsync(c => c.Codigo == model.CodigoCupon && c.Activo);

                if (cupon == null)
                {
                    return Json(new { success = false, message = "Cupón no válido" });
                }

                // Verificar si el cupón está activo y disponible
                if (!cupon.EsValido)
                {
                    return Json(new { success = false, message = "Cupón no disponible" });
                }

                // Obtener subtotal del carrito para validar monto mínimo
                var carritoItems = await _context.Carrito
                    .Where(ci => ci.UsuarioId == usuarioId)
                    .Include(ci => ci.Inventario)
                    .ToListAsync();

                var subtotal = carritoItems.Sum(ci => ci.Inventario.PrecioVentaMinorista * ci.Cantidad);

                if (subtotal < cupon.MontoMinimoCompra)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Monto mínimo no alcanzado. Requiere: S/ {cupon.MontoMinimoCompra}" 
                    });
                }

                // Calcular descuento
                var descuento = cupon.CalcularDescuento(subtotal);

                return Json(new
                {
                    success = true,
                    descuento = descuento,
                    tipoDescuento = cupon.TipoDescuento.ToString(),
                    mensaje = $"Cupón aplicado: {cupon.Descripcion}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aplicar cupón");
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }

        // ========== MÉTODOS DE PEDIDOS ==========

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ProcesarPedido([FromBody] ProcesarPedidoModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
    
            try
            {
                var usuarioId = GetUsuarioId();
        
                if (string.IsNullOrEmpty(usuarioId))
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                // Obtener items del carrito con INCLUDE para el producto
                var carritoItems = await _context.Carrito
                    .Where(ci => ci.UsuarioId == usuarioId)
                    .Include(ci => ci.Inventario) // IMPORTANTE: Incluir el producto
                    .ToListAsync();

                if (!carritoItems.Any())
                {
                    return Json(new { success = false, message = "El carrito está vacío" });
                }

                // VERIFICACIÓN MEJORADA de stock antes de procesar
                var productosSinStock = new List<string>();
                foreach (var item in carritoItems)
                {
                    if (item.Inventario == null)
                    {
                        productosSinStock.Add($"Producto ID {item.InventarioId} no encontrado");
                        continue;
                    }
            
                    if (item.Cantidad > item.Inventario.Cantidad)
                    {
                        productosSinStock.Add($"{item.Inventario.NombreProducto} (Solicitado: {item.Cantidad}, Disponible: {item.Inventario.Cantidad})");
                    }
                }

                if (productosSinStock.Any())
                {
                    return Json(new { 
                        success = false, 
                        message = $"Stock insuficiente: {string.Join(", ", productosSinStock)}" 
                    });
                }

                // Calcular totales
                var subtotal = carritoItems.Sum(ci => ci.Inventario.PrecioVentaMinorista * ci.Cantidad);
        
                // Aplicar descuento si existe
                var descuento = model.DescuentoAplicado;
                var subtotalConDescuento = subtotal - descuento;
                var igv = subtotalConDescuento * 0.18m;
                var total = subtotalConDescuento + igv;

                // Crear pedido
                var pedido = new Pedido
                {
                    UsuarioId = usuarioId,
                    FechaPedido = DateTime.Now,
                    Subtotal = subtotal,
                    IGV = igv,
                    Descuento = descuento,
                    Total = total,
                    DireccionEnvio = model.DireccionEnvio,
                    MetodoPago = model.MetodoPago,
                    Estado = "Confirmado"
                };

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync(); // Guardar para obtener el ID del pedido

                // ACTUALIZACIÓN DE STOCK MEJORADA
                foreach (var item in carritoItems)
                {
                    // Crear detalle del pedido
                    var detalle = new PedidoDetalle
                    {
                        PedidoId = pedido.Id,
                        InventarioId = item.InventarioId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.Inventario.PrecioVentaMinorista
                    };
                    _context.PedidoDetalles.Add(detalle);

                    // ACTUALIZAR STOCK del producto - ESTA ES LA PARTE IMPORTANTE
                    item.Inventario.Cantidad -= item.Cantidad;
                    _context.Inventario.Update(item.Inventario); // Asegurar que se actualice
                }

                // Incrementar uso del cupón si se aplicó
                if (!string.IsNullOrEmpty(model.CodigoCupon))
                {
                    var cupon = await _context.Cupones
                        .FirstOrDefaultAsync(c => c.Codigo == model.CodigoCupon);
                    if (cupon != null)
                    {
                        cupon.UsosActuales++;
                        _context.Cupones.Update(cupon);
                    }
                }

                // Limpiar carrito
                _context.Carrito.RemoveRange(carritoItems);
        
                // GUARDAR TODOS LOS CAMBIOS
                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // Confirmar transacción

                return Json(new { 
                    success = true, 
                    pedidoId = pedido.Id,
                    total = total,
                    message = "Pedido procesado correctamente. Stock actualizado."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Revertir en caso de error
                _logger.LogError(ex, "Error al procesar pedido");
                return Json(new { success = false, message = "Error interno del servidor: " + ex.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MisPedidos()
        {
            try
            {
                var usuarioId = GetUsuarioId();
                
                if (string.IsNullOrEmpty(usuarioId))
                {
                    return Unauthorized();
                }

                var pedidos = await _context.Pedidos
                    .Where(p => p.UsuarioId == usuarioId)
                    .OrderByDescending(p => p.FechaPedido)
                    .Select(p => new
                    {
                        id = p.Id,
                        fecha = p.FechaPedido.ToString("dd/MM/yyyy HH:mm"),
                        total = p.Total,
                        estado = p.Estado,
                        direccion = p.DireccionEnvio,
                        metodoPago = p.MetodoPago
                    })
                    .ToListAsync();

                return Json(pedidos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pedidos");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // ========== MÉTODOS AUXILIARES ==========

        [HttpGet]
        public async Task<IActionResult> ObtenerDetallesProducto(int id)
        {
            try
            {
                var producto = await _context.Inventario
                    .Include(p => p.Categoria)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (producto == null)
                {
                    return NotFound();
                }

                return Json(new
                {
                    id = producto.Id,
                    nombre = producto.NombreProducto,
                    descripcion = $"Categoría: {producto.Categoria?.Nombre} | Marca: {producto.Marca}",
                    precio = producto.PrecioVentaMinorista,
                    stock = producto.Cantidad,
                    imagenUrl = producto.Imagen,
                    caracteristicas = $"Grado Alcohol: {producto.GradoAlcohol}° | Precio Mayorista: S/ {producto.PrecioVentaMayorista}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles del producto");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProductosPorCategoria(int categoriaId)
        {
            try
            {
                var productos = await _context.Inventario
                    .Where(p => p.CategoriasId == categoriaId && p.Cantidad > 0)
                    .Include(p => p.Categoria)
                    .OrderBy(p => p.NombreProducto)
                    .Select(p => new
                    {
                        id = p.Id,
                        nombre = p.NombreProducto,
                        precio = p.PrecioVentaMinorista,
                        imagen = p.Imagen,
                        stock = p.Cantidad,
                        categoria = p.Categoria.Nombre
                    })
                    .ToListAsync();

                return Json(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por categoría");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
    }

    // ========== MODELOS AUXILIARES ==========

    public class AgregarAlCarritoModel
    {
        public int InventarioId { get; set; }
        public int Cantidad { get; set; }
    }

    public class ActualizarCantidadModel
    {
        public int ItemId { get; set; }
        public int NuevaCantidad { get; set; }
    }

    public class EliminarDelCarritoModel
    {
        public int ItemId { get; set; }
    }

    public class AplicarCuponModel
    {
        public string CodigoCupon { get; set; }
    }

    public class ProcesarPedidoModel
    {
        public string DireccionEnvio { get; set; }
        public string MetodoPago { get; set; }
        public decimal DescuentoAplicado { get; set; }
        public string CodigoCupon { get; set; }
    }
}