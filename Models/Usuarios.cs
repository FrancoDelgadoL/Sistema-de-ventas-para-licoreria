using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Ezel_Market.Models
{
    public class Usuarios : IdentityUser
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [CustomValidation(typeof(Usuarios), "ValidateFechaNacimiento")]
        public DateTime FechaNacimiento { get; set; }

        // MÉTODO DE VALIDACIÓN REQUERIDO - AGREGA ESTO
        public static ValidationResult ValidateFechaNacimiento(DateTime fechaNacimiento, ValidationContext validationContext)
        {
            // Validar que no sea fecha futura
            if (fechaNacimiento > DateTime.Today)
            {
                return new ValidationResult("La fecha de nacimiento no puede ser en el futuro.");
            }

            // Calcular edad
            int edad = DateTime.Today.Year - fechaNacimiento.Year;
            
            // Ajustar si aún no ha cumplido años este año
            if (fechaNacimiento.Date > DateTime.Today.AddYears(-edad))
            {
                edad--;
            }

            // Validar edad mínima (12 años)
            if (edad < 18)
            {
                return new ValidationResult("Debes tener al menos 18 años para registrarte.");
            }

            // Validar edad máxima razonable (120 años)
            if (edad > 70)
            {
                return new ValidationResult("La fecha de nacimiento no es válida.");
            }

            return ValidationResult.Success;
        }
    }
}