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
            
            viewModel.Productos = await _context.Inventarios
                .Include(p => p.CategoriaInventarios)
                    .ThenInclude(ci => ci.Categoria)
                .Where(p => p.Cantidad > 0)
                .OrderBy(p => p.NombreProducto)
                .ToListAsync();                 

            viewModel.Categorias = await _context.Categorias
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

                var producto = await _context.Inventarios
                    .FirstOrDefaultAsync(p => p.Id == model.InventarioId);

                if (producto == null)
                {
                    return Json(new { success = false, message = "Producto no encontrado" });
                }

                if (producto.Cantidad <= 0)
                {
                    return Json(new { success = false, message = "Producto sin stock disponible" });
                }

                if (model.Cantidad <= 0 || model.Cantidad > producto.Cantidad)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Cantidad inválida. Stock disponible: {producto.Cantidad} unidades" 
                    });
                }

                var itemExistente = await _context.Carrito
                    .Include(c => c.Inventario)
                    .FirstOrDefaultAsync(ci => ci.UsuarioId == usuarioId && ci.InventarioId == model.InventarioId);

                if (itemExistente != null)
                {
                    var nuevaCantidadTotal = itemExistente.Cantidad + model.Cantidad;
            
                    if (nuevaCantidadTotal > producto.Cantidad)
                    {
                        return Json(new { 
                            success = false, 
                            message = $"No puedes agregar más unidades. Stock disponible: {producto.Cantidad}" 
                        });
                    }
            
                    itemExistente.Cantidad = nuevaCantidadTotal;
                    itemExistente.FechaAgregado = DateTime.Now;
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
                        .ThenInclude(i => i.CategoriaInventarios)
                            .ThenInclude(ci => ci.Categoria)
                    .Select(ci => new
                    {
                        id = ci.Id,
                        productoId = ci.InventarioId,
                        nombre = ci.Inventario.NombreProducto,
                        precio = ci.Inventario.PrecioVentaMinorista,
                        cantidad = ci.Cantidad,
                        imagenUrl = ci.Inventario.Imagen,
                        categoria = ci.Inventario.CategoriaInventarios.FirstOrDefault().Categoria.Nombre ?? "Sin categoría",
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

        // ========== MÉTODOS DE CUPONES CORREGIDOS ==========

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AplicarCupon([FromBody] AplicarCuponModel model)
        {
            try
            {
                var usuarioId = GetUsuarioId();
        
                // Validación básica del modelo
                if (string.IsNullOrWhiteSpace(model.CodigoCupon))
                {
                    return Json(new { success = false, message = "El código del cupón es requerido" });
                }

                // Obtener el cupón
                var cupon = await _context.Cupones
                    .FirstOrDefaultAsync(c => c.Codigo == model.CodigoCupon.Trim());

                if (cupon == null)
                {
                    return Json(new { success = false, message = "Cupón no encontrado" });
                }

                // Obtener subtotal del carrito
                var carritoItems = await _context.Carrito
                    .Where(ci => ci.UsuarioId == usuarioId)
                    .Include(ci => ci.Inventario)
                    .ToListAsync();

                if (!carritoItems.Any())
                {
                    return Json(new { success = false, message = "El carrito está vacío" });
                }

                var subtotal = carritoItems.Sum(ci => ci.Inventario.PrecioVentaMinorista * ci.Cantidad);
                
                // ✅ CORRECCIÓN: Solo validar, NO aplicar el cupón aún
                var validacion = cupon.ValidarParaUsuario(usuarioId, subtotal);

                if (!validacion.esValido)
                {
                    return Json(new { success = false, message = validacion.mensaje });
                }

                // ✅ CORRECCIÓN: Solo calcular el descuento, NO incrementar UsosActuales
                var descuento = cupon.CalcularDescuento(subtotal);

                if (descuento <= 0)
                {
                    return Json(new { success = false, message = "No se pudo calcular el descuento" });
                }

                return Json(new
                {
                    success = true,
                    descuento = descuento,
                    tipoDescuento = cupon.TipoDescuento.ToString(),
                    mensaje = $"Cupón aplicado: {cupon.Descripcion} - Descuento: S/ {descuento:0.00}",
                    cuponInfo = new {
                        descripcion = cupon.Descripcion,
                        codigo = cupon.Codigo,
                        expiracion = cupon.FechaExpiracion.ToString("dd/MM/yyyy")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aplicar cupón");
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }

        // 🔥 MÉTODO MEJORADO para obtener cupones disponibles
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerTodosCuponesDisponibles()
        {
            try
            {
                var usuarioId = GetUsuarioId();

                if (string.IsNullOrEmpty(usuarioId))
                {
                    return Unauthorized(new { success = false, message = "Usuario no autenticado" });
                }

                // Obtener subtotal del carrito para validaciones
                var carritoItems = await _context.Carrito
                    .Where(ci => ci.UsuarioId == usuarioId)
                    .Include(ci => ci.Inventario)
                    .ToListAsync();
                var subtotal = carritoItems.Sum(ci => ci.Inventario?.PrecioVentaMinorista * ci.Cantidad ?? 0);

                // ✅ CORRECCIÓN: Incluir cupones expirados para mostrarlos como no disponibles
                var cupones = await _context.Cupones
                    .Where(c => c.Activo && c.FechaInicio <= DateTime.Now)
                    .ToListAsync();
                    
                var cuponesDisponibles = new List<object>();

                foreach (var cupon in cupones)
                {
                    var validacion = cupon.ValidarParaUsuario(usuarioId, subtotal);
                
                    cuponesDisponibles.Add(new
                    {
                        id = cupon.Id,
                        codigo = cupon.Codigo,
                        descripcion = cupon.Descripcion,
                        tipo = cupon.TipoDescuento.ToString(),
                        descuento = cupon.TipoDescuento == TipoDescuento.Porcentaje ?
                                $"{cupon.PorcentajeDescuento}%" :
                                $"S/ {cupon.ValorDescuento:0.00}",
                        valorNumerico = cupon.TipoDescuento == TipoDescuento.Porcentaje ?
                                    cupon.PorcentajeDescuento : cupon.ValorDescuento,
                        expiracion = cupon.FechaExpiracion.ToString("dd/MM/yyyy"),
                        diasRestantes = (cupon.FechaExpiracion - DateTime.Now).Days,
                        minimoCompra = cupon.MontoMinimoCompra,
                        minimoCompraTexto = cupon.MontoMinimoCompra > 0 ?
                                        $"Mínimo: S/ {cupon.MontoMinimoCompra:0.00}" : "Sin mínimo de compra",
                        usosDisponibles = cupon.UsosMaximos - cupon.UsosActuales,
                        esValido = validacion.esValido,
                        mensajeValidacion = validacion.esValido ? "Disponible" : validacion.mensaje,
                        estado = cupon.Estado
                    });
                }

                return Json(new
                {
                    success = true,
                    cupones = cuponesDisponibles,
                    totalCupones = cuponesDisponibles.Count,
                    subtotalActual = subtotal
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los cupones disponibles");
                return Json(new
                {
                    success = false,
                    message = "Error al cargar los cupones",
                    error = ex.Message
                });
            }
        }

        // ========== MÉTODOS DE PEDIDOS CORREGIDOS ==========

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

                var carritoItems = await _context.Carrito
                    .Where(ci => ci.UsuarioId == usuarioId)
                    .Include(ci => ci.Inventario)
                    .ToListAsync();

                if (!carritoItems.Any())
                {
                    return Json(new { success = false, message = "El carrito está vacío" });
                }

                // ✅ CORRECCIÓN: Validar stock ANTES de procesar
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

                var subtotal = carritoItems.Sum(ci => ci.Inventario.PrecioVentaMinorista * ci.Cantidad);
        
                // ✅ CORRECCIÓN: Validar que el descuento no sea mayor al subtotal
                var descuento = Math.Min(model.DescuentoAplicado, subtotal);
                var subtotalConDescuento = subtotal - descuento;
                var igv = subtotalConDescuento * 0.18m;
                var total = subtotalConDescuento + igv;

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
                await _context.SaveChangesAsync();

                // Procesar items del carrito
                foreach (var item in carritoItems)
                {
                    var detalle = new PedidoDetalle
                    {
                        PedidoId = pedido.Id,
                        InventarioId = item.InventarioId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.Inventario.PrecioVentaMinorista
                    };
                    _context.PedidoDetalles.Add(detalle);

                    // Actualizar stock
                    item.Inventario.Cantidad -= item.Cantidad;
                }

                // ✅ CORRECCIÓN: Incrementar usos del cupón SOLO si se aplicó uno
                if (!string.IsNullOrEmpty(model.CodigoCupon))
                {
                    var cupon = await _context.Cupones
                        .FirstOrDefaultAsync(c => c.Codigo == model.CodigoCupon);
                    if (cupon != null)
                    {
                        cupon.UsosActuales++;
                        // Desactivar cupón si alcanzó el límite
                        if (cupon.UsosActuales >= cupon.UsosMaximos)
                        {
                            cupon.Activo = false;
                        }
                        _context.Cupones.Update(cupon);
                    }
                }

                // Limpiar carrito
                _context.Carrito.RemoveRange(carritoItems);
        
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    pedidoId = pedido.Id,
                    total = total,
                    message = "Pedido procesado correctamente. Stock actualizado."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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
        var producto = await _context.Inventarios
            .Include(p => p.CategoriaInventarios)
                .ThenInclude(ci => ci.Categoria)
            .Include(p => p.Reviews) // <-- Agregar esta línea para incluir las reseñas
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto == null)
        {
            return NotFound();
        }

        // Calculamos el promedio de calificaciones
        double promedioCalificacion = 0;
        if (producto.Reviews != null && producto.Reviews.Any())
        {
            promedioCalificacion = producto.Reviews.Average(r => r.Calificacion);
        }

        var categoriaNombre = producto.CategoriaInventarios?.FirstOrDefault()?.Categoria?.Nombre ?? "Sin categoría";

        return Json(new
        {
            id = producto.Id,
            nombre = producto.NombreProducto,
            descripcion = $"Categoría: {categoriaNombre} | Marca: {producto.Marca}",
            precio = producto.PrecioVentaMinorista,
            stock = producto.Cantidad,
            imagenUrl = producto.Imagen,
            caracteristicas = $"Grado Alcohol: {producto.GradoAlcohol}° | Precio Mayorista: S/ {producto.PrecioVentaMayorista}",
            promedioCalificacion = promedioCalificacion // <-- Aquí devolvemos el promedio
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
                var productos = await _context.Inventarios
                    .Where(p => p.CategoriaInventarios.Any(ci => ci.CategoriaId == categoriaId) && p.Cantidad > 0)
                    .Include(p => p.CategoriaInventarios)
                        .ThenInclude(ci => ci.Categoria)
                    .OrderBy(p => p.NombreProducto)
                    .Select(p => new
                    {
                        id = p.Id,
                        nombre = p.NombreProducto,
                        precio = p.PrecioVentaMinorista,
                        imagen = p.Imagen,
                        stock = p.Cantidad,
                        categoria = p.CategoriaInventarios.FirstOrDefault().Categoria.Nombre ?? "Sin categoría"
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