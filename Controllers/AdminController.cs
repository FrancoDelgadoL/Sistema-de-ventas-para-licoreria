using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Ezel_Market.Data;
using Ezel_Market.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Ezel_Market.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly UserManager<Usuarios> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context,
                             ILogger<AdminController> logger,
                             UserManager<Usuarios> userManager,
                             RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario != null)
                {
                    ViewBag.NombreUsuario = $"{usuario.Nombre} {usuario.Apellido}";
                    ViewBag.Nombre = usuario.Nombre;
                    ViewBag.Apellido = usuario.Apellido;
                    ViewBag.Email = usuario.Email;
                }
                else
                {
                    // DEBUG: Si el usuario es null
                    ViewBag.DebugInfo = "Usuario no encontrado en la base de datos";
                }
            }
            else
            {
                // DEBUG: Si no está autenticado
                ViewBag.DebugInfo = "Usuario no autenticado";
            }

            return View();
        }

        // [MANTENER TODOS TUS OTROS MÉTODOS EXISTENTES AQUÍ...]
        public async Task<IActionResult> ListarUsuarios()
        {
            var usuariosConRoles = new List<UsuarioConRol>();
            var todosUsuarios = _context.Users.ToList();

            foreach (var user in todosUsuarios)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var usuario = new UsuarioConRol
                {
                    Id = user.Id,
                    Nombre = user.Nombre,
                    Apellido = user.Apellido,
                    UserName = user.UserName,
                    Email = user.Email,
                    Rol = roles.FirstOrDefault() ?? "Sin rol"
                };

                usuariosConRoles.Add(usuario);
            }

            return View(usuariosConRoles);
        }

        // GET: AsignarRoles - AHORA INCLUYE FORMULARIO PARA AGREGAR USUARIOS
        public async Task<IActionResult> AsignarRoles()
        {
            try
            {
                // Asegurar que los roles existan
                await CrearRolesSiNoExisten();

                // Obtener usuarios existentes
                var usuariosConRoles = new List<UsuarioConRol>();
                var todosUsuarios = _context.Users.ToList();

                foreach (var user in todosUsuarios)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    var usuario = new UsuarioConRol
                    {
                        Id = user.Id,
                        Nombre = user.Nombre,
                        Apellido = user.Apellido,
                        UserName = user.UserName,
                        Email = user.Email,
                        Rol = roles.FirstOrDefault() ?? "Sin rol"
                    };
                    usuariosConRoles.Add(usuario);
                }

                // Crear ViewModel que incluye tanto usuarios existentes como formulario para nuevo usuario
                var viewModel = new AsignarRoles
                {
                    Usuarios = usuariosConRoles,
                    RolesDisponibles = await GetRolesDisponibles(),
                    NuevoUsuario = new AgregarUsuario() // Formulario vacío para nuevo usuario
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la vista de asignar roles");
                TempData["Error"] = "Error al cargar los datos";
                return RedirectToAction("Index");
            }
        }

        // POST: AgregarNuevoUsuario - DESDE LA VISTA ASIGNARROLES
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarNuevoUsuario(AgregarUsuario model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Verificar si el email ya existe
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        TempData["Error"] = "El email ya está registrado.";
                        return RedirectToAction("AsignarRoles");
                    }

                    // Verificar si el username ya existe
                    var existingUserName = await _userManager.FindByNameAsync(model.UserName);
                    if (existingUserName != null)
                    {
                        TempData["Error"] = "El nombre de usuario ya existe.";
                        return RedirectToAction("AsignarRoles");
                    }

                    // Crear nuevo usuario
                    var user = new Usuarios
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        Nombre = model.Nombre,
                        Apellido = model.Apellido,
                        EmailConfirmed = true
                    };

                    // Crear el usuario con Identity
                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        // ASIGNAR ROL - Esto actualiza AspNetUserRoles automáticamente
                        var roleResult = await _userManager.AddToRoleAsync(user, model.RolSeleccionado);

                        if (roleResult.Succeeded)
                        {
                            _logger.LogInformation($"Usuario {model.Email} creado con rol {model.RolSeleccionado}");
                            TempData["Success"] = $"Usuario {model.Nombre} {model.Apellido} creado exitosamente con rol {model.RolSeleccionado}";
                        }
                        else
                        {
                            // Si falla la asignación de rol, eliminar el usuario creado
                            await _userManager.DeleteAsync(user);
                            var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                            TempData["Error"] = $"Error al asignar rol: {roleErrors}";
                        }
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        TempData["Error"] = $"Error al crear usuario: {errors}";
                    }
                }
                else
                {
                    TempData["Error"] = "Por favor complete todos los campos requeridos correctamente.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                TempData["Error"] = "Ocurrió un error al crear el usuario";
            }

            return RedirectToAction("AsignarRoles");
        }

        // POST: AsignarRol a usuario existente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarRol(string usuarioId, string rolSeleccionado)
        {
            try
            {
                if (string.IsNullOrEmpty(usuarioId) || string.IsNullOrEmpty(rolSeleccionado))
                {
                    TempData["Error"] = "Debe seleccionar un usuario y un rol";
                    return RedirectToAction("AsignarRoles");
                }

                var usuario = await _userManager.FindByIdAsync(usuarioId);
                if (usuario == null)
                {
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction("AsignarRoles");
                }

                // Verificar que el rol existe
                if (!await _roleManager.RoleExistsAsync(rolSeleccionado))
                {
                    TempData["Error"] = $"El rol {rolSeleccionado} no existe";
                    return RedirectToAction("AsignarRoles");
                }

                // Obtener roles actuales del usuario
                var rolesActuales = await _userManager.GetRolesAsync(usuario);

                // Remover todos los roles actuales
                if (rolesActuales.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
                    if (!removeResult.Succeeded)
                    {
                        var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                        TempData["Error"] = $"Error al remover roles: {errors}";
                        return RedirectToAction("AsignarRoles");
                    }
                }

                // Agregar el nuevo rol
                var addResult = await _userManager.AddToRoleAsync(usuario, rolSeleccionado);

                if (addResult.Succeeded)
                {
                    _logger.LogInformation($"Rol {rolSeleccionado} asignado al usuario {usuario.Email}");
                    TempData["Success"] = $"Rol {rolSeleccionado} asignado correctamente a {usuario.Nombre} {usuario.Apellido}";
                }
                else
                {
                    var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    _logger.LogError($"Error al asignar rol: {errors}");
                    TempData["Error"] = $"Error al asignar rol: {errors}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar rol");
                TempData["Error"] = "Ocurrió un error al asignar el rol";
            }

            return RedirectToAction("AsignarRoles");
        }

        // POST: Eliminar Usuario con Identity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUsuario(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    TempData["Error"] = "ID de usuario no válido";
                    return RedirectToAction(nameof(ListarUsuarios));
                }

                // Buscar usuario con Identity
                var usuario = await _userManager.FindByIdAsync(id);
                if (usuario == null)
                {
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction(nameof(ListarUsuarios));
                }

                // Verificar que no sea el usuario actual
                var currentUserId = _userManager.GetUserId(User);
                if (usuario.Id == currentUserId)
                {
                    TempData["Error"] = "No puedes eliminar tu propio usuario";
                    return RedirectToAction(nameof(ListarUsuarios));
                }

                // Eliminar usuario con Identity
                var result = await _userManager.DeleteAsync(usuario);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Usuario {usuario.Email} eliminado exitosamente");
                    TempData["Success"] = "Usuario eliminado exitosamente";
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError($"Error al eliminar usuario: {errors}");
                    TempData["Error"] = $"Error al eliminar el usuario: {errors}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario");
                TempData["Error"] = "Ocurrió un error al eliminar el usuario";
            }

            return RedirectToAction(nameof(ListarUsuarios));
        }

        // Métodos auxiliares
        private async Task<List<SelectListItem>> GetRolesDisponibles()
        {
            return await _context.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                })
                .ToListAsync();
        }

        private async Task CrearRolesSiNoExisten()
        {
            string[] roles = { "Administrador", "Gerente", "Inventario", "Cliente" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        // ⚙️ MÉTODO TEMPORAL para asignar rol Administrador
        [AllowAnonymous]
        [HttpGet("Admin/AsignarAdmin")]
        public async Task<IActionResult> AsignarAdmin()
        {
            try
            {
                // 📌 Configura aquí el correo de tu usuario administrador
                string adminEmail = "tu_correo_admin@correo.com";

                // Buscar el usuario
                var usuario = await _userManager.FindByEmailAsync(adminEmail);

                if (usuario == null)
                {
                    return Content($"❌ No se encontró ningún usuario con el correo {adminEmail}");
                }

                // Crear el rol si no existe
                if (!await _roleManager.RoleExistsAsync("Administrador"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Administrador"));
                }

                // Asignar el rol si aún no lo tiene
                if (!await _userManager.IsInRoleAsync(usuario, "Administrador"))
                {
                    await _userManager.AddToRoleAsync(usuario, "Administrador");
                    return Content($"✅ El usuario {adminEmail} ahora tiene el rol de Administrador");
                }
                else
                {
                    return Content($"ℹ️ El usuario {adminEmail} ya tenía el rol de Administrador");
                }
            }
            catch (Exception ex)
            {
                return Content($"⚠️ Error al asignar rol: {ex.Message}");
            }
        }

        //===========Controller para cupones======================================================================================
        //Controller para cupones
        // GET: ListarCupones - Vista única con todo
        public async Task<IActionResult> ListarCupones()
        {
            var cupones = await _context.Cupones.ToListAsync();
            return View("Cupones", cupones);
        }

        // POST: CrearCupon - Desde el modal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCupon(Cupon cupon)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Verificar si el código ya existe
                    if (await _context.Cupones.AnyAsync(c => c.Codigo == cupon.Codigo))
                    {
                        TempData["Error"] = "Este código de cupón ya existe";
                        return RedirectToAction(nameof(ListarCupones));
                    }

                    // Validar fechas
                    if (cupon.FechaInicio >= cupon.FechaExpiracion)
                    {
                        TempData["Error"] = "La fecha de inicio debe ser anterior a la fecha de expiración";
                        return RedirectToAction(nameof(ListarCupones));
                    }

                    // Validar tipo de descuento
                    if (cupon.TipoDescuento == TipoDescuento.Porcentaje)
                    {
                        if (!cupon.PorcentajeDescuento.HasValue || cupon.PorcentajeDescuento <= 0)
                        {
                            TempData["Error"] = "El porcentaje de descuento es requerido";
                            return RedirectToAction(nameof(ListarCupones));
                        }
                        cupon.ValorDescuento = 0; // Limpiar valor si es porcentaje
                    }
                    else
                    {
                        if (cupon.ValorDescuento <= 0)
                        {
                            TempData["Error"] = "El valor de descuento es requerido";
                            return RedirectToAction(nameof(ListarCupones));
                        }
                        cupon.PorcentajeDescuento = null; // Limpiar porcentaje si es monto fijo
                    }

                    // Inicializar usos actuales
                    cupon.UsosActuales = 0;

                    _context.Add(cupon);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Cupón '{cupon.Codigo}' creado exitosamente";
                    _logger.LogInformation($"Cupón {cupon.Codigo} creado por {User.Identity.Name}");
                }
                else
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    TempData["Error"] = $"Error en los datos: {errors}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cupón");
                TempData["Error"] = "Ocurrió un error al crear el cupón";
            }

            return RedirectToAction(nameof(ListarCupones));
        }

        // POST: EditarCupon - Desde el modal (VERSIÓN CORREGIDA)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCupon(Cupon cupon)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    TempData["Error"] = $"Error en los datos: {errors}";
                    return RedirectToAction(nameof(ListarCupones));
                }

                // Verificar si existe otro cupón con el mismo código
                if (await _context.Cupones.AnyAsync(c => c.Codigo == cupon.Codigo && c.Id != cupon.Id))
                {
                    TempData["Error"] = "Este código de cupón ya existe en otro cupón";
                    return RedirectToAction(nameof(ListarCupones));
                }

                // Validar fechas
                if (cupon.FechaInicio >= cupon.FechaExpiracion)
                {
                    TempData["Error"] = "La fecha de inicio debe ser anterior a la fecha de expiración";
                    return RedirectToAction(nameof(ListarCupones));
                }

                // Cargar el cupón existente y actualizar propiedades individualmente
                var cuponExistente = await _context.Cupones.FindAsync(cupon.Id);
                if (cuponExistente == null)
                {
                    TempData["Error"] = "El cupón no existe";
                    return RedirectToAction(nameof(ListarCupones));
                }

                // ACTUALIZAR PROPIEDADES UNA POR UNA (evita problemas de tracking)
                cuponExistente.Codigo = cupon.Codigo;
                cuponExistente.Descripcion = cupon.Descripcion;
                cuponExistente.TipoDescuento = cupon.TipoDescuento;

                // Manejar campos de descuento según el tipo
                if (cupon.TipoDescuento == TipoDescuento.MontoFijo)
                {
                    cuponExistente.ValorDescuento = cupon.ValorDescuento;
                    cuponExistente.PorcentajeDescuento = null;
                }
                else
                {
                    cuponExistente.PorcentajeDescuento = cupon.PorcentajeDescuento;
                    cuponExistente.ValorDescuento = 0;
                }

                cuponExistente.FechaInicio = cupon.FechaInicio;
                cuponExistente.FechaExpiracion = cupon.FechaExpiracion;
                cuponExistente.UsosMaximos = cupon.UsosMaximos;
                cuponExistente.MontoMinimoCompra = cupon.MontoMinimoCompra;
                cuponExistente.Activo = cupon.Activo;

                // ❌ ELIMINADO: cuponExistente.SoloPrimeraCompra = cupon.SoloPrimeraCompra;

                // No actualizar UsosActuales - mantener el valor existente

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Cupón '{cupon.Codigo}' actualizado exitosamente";
                _logger.LogInformation($"Cupón {cupon.Codigo} actualizado por {User.Identity.Name}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CuponExists(cupon.Id))
                {
                    TempData["Error"] = "El cupón no existe";
                }
                else
                {
                    TempData["Error"] = "Error de concurrencia al actualizar el cupón";
                    _logger.LogError("Error de concurrencia al actualizar cupón {CuponId}", cupon.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cupón");
                TempData["Error"] = "Ocurrió un error al actualizar el cupón";
            }

            return RedirectToAction(nameof(ListarCupones));
        }

        // POST: EliminarCupon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCupon(int id)
        {
            try
            {
                var cupon = await _context.Cupones.FindAsync(id);
                if (cupon == null)
                {
                    TempData["Error"] = "Cupón no encontrado";
                    return RedirectToAction(nameof(ListarCupones));
                }

                var codigoCupon = cupon.Codigo;
                _context.Cupones.Remove(cupon);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Cupón '{codigoCupon}' eliminado exitosamente";
                _logger.LogInformation($"Cupón {codigoCupon} eliminado por {User.Identity.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cupón");
                TempData["Error"] = "Ocurrió un error al eliminar el cupón";
            }

            return RedirectToAction(nameof(ListarCupones));
        }

        // API para validar cupón (opcional - para futuras ventas)
        [HttpGet]
        public async Task<JsonResult> ValidarCupon(string codigo, decimal subtotal, string usuarioId = null)
        {
            try
            {
                var cupon = await _context.Cupones
                    .FirstOrDefaultAsync(c => c.Codigo == codigo);

                if (cupon == null)
                    return Json(new { valido = false, mensaje = "Cupón no encontrado" });

                if (!cupon.EsValido)
                    return Json(new { valido = false, mensaje = "Cupón no válido o expirado" });

                if (subtotal < cupon.MontoMinimoCompra)
                    return Json(new { valido = false, mensaje = $"Monto mínimo requerido: {cupon.MontoMinimoCompra:C}" });

                if (cupon.UsosActuales >= cupon.UsosMaximos)
                    return Json(new { valido = false, mensaje = "Cupón ya fue utilizado el máximo de veces" });

                var descuento = cupon.CalcularDescuento(subtotal);

                return Json(new
                {
                    valido = true,
                    mensaje = "Cupón aplicado correctamente",
                    descuento = descuento,
                    tipoDescuento = cupon.TipoDescuento.ToString(),
                    cupon = new
                    {
                        codigo = cupon.Codigo,
                        descripcion = cupon.Descripcion
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar cupón");
                return Json(new { valido = false, mensaje = "Error al validar el cupón" });
            }
        }

        private bool CuponExists(int id)
        {
            return _context.Cupones.Any(e => e.Id == id);
        }
    }
}
