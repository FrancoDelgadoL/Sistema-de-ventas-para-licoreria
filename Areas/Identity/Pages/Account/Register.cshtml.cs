// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Ezel_Market.Models;

namespace Ezel_Market.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<Usuarios> _signInManager;
        private readonly UserManager<Usuarios> _userManager;
        private readonly IUserStore<Usuarios> _userStore;
        private readonly IUserEmailStore<Usuarios> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        

        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<Usuarios> userManager,
            IUserStore<Usuarios> userStore,
            SignInManager<Usuarios> signInManager,
            ILogger<RegisterModel> logger,
           
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            
            _roleManager = roleManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {

            [Required(ErrorMessage = "El nombre es obligatorio")]
            [StringLength(30, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
            [Display(Name = "Nombre")]
            public string Nombre { get; set; }     // Usuario

            [Required(ErrorMessage = "El nombre es obligatorio")]
            [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
            [Display(Name = "Apellido")]
            public string Apellido { get; set; }   // Usuario

            [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
            [DataType(DataType.Date)]
            [Display(Name = "Fecha de Nacimiento")]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
            [CustomValidation(typeof(Usuarios), "ValidateFechaNacimiento")]
            public DateTime FechaNacimiento { get; set; }
            
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                
                user.Nombre = Input.Nombre;
                user.Apellido = Input.Apellido;
                user.FechaNacimiento = Input.FechaNacimiento;              
                
                
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (!await _roleManager.RoleExistsAsync("Cliente"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Cliente"));
                    }

                    await _userManager.AddToRoleAsync(user, "Cliente");

                    System.Diagnostics.Debug.WriteLine("REGISTRO EXITOSO - Mostrando mensaje");
                    
                    //Limpia el formulario para que no queden los datos
                    ModelState.Clear();
                    
                    // ✅ CREAR NUEVO MODELO VACÍO
                    Input = new InputModel();
            
                    // ✅ MENSAJE DE ÉXITO
                    ViewData["SuccessMessage"] = $"¡Registro exitoso! Bienvenido {user.Nombre}. Tu cuenta ha sido creada correctamente.";

                    
                    return Page();
                    //Inicio de Sesiòn automatico
                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    //return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private Usuarios CreateUser()
        {
            try
            {
                return Activator.CreateInstance<Usuarios>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<Usuarios> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<Usuarios>)_userStore;
        }
    }
}
