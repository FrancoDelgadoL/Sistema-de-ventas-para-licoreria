using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ezel_Market.Models
{
    public class Cupon : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El código del cupón es obligatorio")]
        [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        [Display(Name = "Código del Cupón")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(100, ErrorMessage = "La descripción no puede exceder 100 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El tipo de descuento es obligatorio")]
        [Display(Name = "Tipo de Descuento")]
        public TipoDescuento TipoDescuento { get; set; }

        [Display(Name = "Valor de Descuento")]
        [Range(0.01, 10000, ErrorMessage = "El valor debe ser mayor a 0")]
        public decimal ValorDescuento { get; set; }

        [Display(Name = "Porcentaje de Descuento")]
        [Range(1, 100, ErrorMessage = "El porcentaje debe estar entre 1% y 100%")]
        public decimal? PorcentajeDescuento { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.DateTime)]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de expiración es obligatoria")]
        [Display(Name = "Fecha de Expiración")]
        [DataType(DataType.DateTime)]
        public DateTime FechaExpiracion { get; set; }

        [Display(Name = "Usos Máximos")]
        [Range(1, 1000000, ErrorMessage = "Los usos máximos deben ser al menos 1")]
        public int UsosMaximos { get; set; }

        [Display(Name = "Usos Actuales")]
        public int UsosActuales { get; set; } = 0;

        [Display(Name = "Monto Mínimo de Compra")]
        [Range(0, 1000000, ErrorMessage = "El monto mínimo no puede ser negativo")]
        public decimal MontoMinimoCompra { get; set; } = 0;

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Propiedad de estado mejorada
        [Display(Name = "Estado")]
        public string Estado
        {
            get
            {
                if (!Activo) return "Inactivo";
                if (DateTime.Now < FechaInicio) return "Programado";
                if (DateTime.Now > FechaExpiracion) return "Expirado";
                if (UsosActuales >= UsosMaximos) return "Límite Alcanzado";
                return "Activo";
            }
        }

        [Display(Name = "Válido")]
        public bool EsValido => Estado == "Activo";

        [Display(Name = "Disponible")]
        public bool EstaDisponible => EsValido && UsosActuales < UsosMaximos;

        public decimal CalcularDescuento(decimal subtotal)
        {
            if (subtotal < MontoMinimoCompra)
                return 0;

            if (TipoDescuento == TipoDescuento.Porcentaje && PorcentajeDescuento.HasValue)
            {
                return subtotal * (PorcentajeDescuento.Value / 100);
            }
            else if (TipoDescuento == TipoDescuento.MontoFijo)
            {
                return Math.Min(ValorDescuento, subtotal);
            }

            return 0;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaInicio >= FechaExpiracion)
            {
                yield return new ValidationResult(
                    "La fecha de inicio debe ser anterior a la fecha de expiración",
                    new[] { nameof(FechaInicio), nameof(FechaExpiracion) }
                );
            }

            if (TipoDescuento == TipoDescuento.Porcentaje && (!PorcentajeDescuento.HasValue || PorcentajeDescuento <= 0))
            {
                yield return new ValidationResult(
                    "El porcentaje de descuento es requerido para descuentos porcentuales",
                    new[] { nameof(PorcentajeDescuento) }
                );
            }

            if (TipoDescuento == TipoDescuento.MontoFijo && ValorDescuento <= 0)
            {
                yield return new ValidationResult(
                    "El valor de descuento es requerido para descuentos de monto fijo",
                    new[] { nameof(ValorDescuento) }
                );
            }

            if (PorcentajeDescuento.HasValue && PorcentajeDescuento > 100)
            {
                yield return new ValidationResult(
                    "El porcentaje no puede ser mayor al 100%",
                    new[] { nameof(PorcentajeDescuento) }
                );
            }

            if (UsosActuales > UsosMaximos)
            {
                yield return new ValidationResult(
                    "Los usos actuales no pueden exceder los usos máximos",
                    new[] { nameof(UsosActuales) }
                );
            }

            // 🎯 BLOQUE ELIMINADO: La validación de fecha expirada en el pasado
            // ya no está aquí para evitar conflictos con ValidarParaUsuario
        }

        public (bool esValido, string mensaje) ValidarParaUsuario(string usuarioId, decimal subtotal)
        {
            if (!Activo)
                return (false, "El cupón no está activo");

            if (DateTime.Now < FechaInicio)
                return (false, "El cupón aún no está vigente");

            if (DateTime.Now > FechaExpiracion)
                return (false, "El cupón ha expirado");

            if (UsosActuales >= UsosMaximos)
                return (false, "El cupón ha alcanzado su límite de usos");

            if (subtotal < MontoMinimoCompra)
                return (false, $"Requiere un monto mínimo de S/ {MontoMinimoCompra:0.00}");

            return (true, "Cupón válido");
        }

        public (bool aplicado, decimal descuento, string mensaje) AplicarCupon(string usuarioId, decimal subtotal)
        {
            var validacion = ValidarParaUsuario(usuarioId, subtotal);
            if (!validacion.esValido)
            {
                return (false, 0, validacion.mensaje);
            }

            var descuento = CalcularDescuento(subtotal);
            if (descuento > 0)
            {
                UsosActuales++;
                return (true, descuento, $"Cupón aplicado: {Descripcion} - Descuento: S/ {descuento:0.00}");
            }

            return (false, 0, "No se pudo calcular el descuento");
        }
    }

    public enum TipoDescuento
    {
        [Display(Name = "Monto Fijo")]
        MontoFijo,
        
        [Display(Name = "Porcentaje")]
        Porcentaje
    }
}