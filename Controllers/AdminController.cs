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

        public IActionResult Index()
        {
            return View();
        }

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
    }
}