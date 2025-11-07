using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ezel_Market.Models
{
    public class Cupon
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

        [Display(Name = "Solo Primera Compra")]
        public bool SoloPrimeraCompra { get; set; } = false;

        // 🔥 CORRECCIÓN: Propiedad de estado mejorada
        [Display(Name = "Estado")]
        public string Estado
        {
            get
            {
                if (DateTime.Now < FechaInicio) return "Programado";
                if (DateTime.Now > FechaExpiracion) return "Expirado";
                if (UsosActuales >= UsosMaximos) return "Límite Alcanzado";
                return "Activo";
            }
        }

        // 🔥 CORRECCIÓN: Propiedad EsValido más flexible
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
    }

    public enum TipoDescuento
    {
        [Display(Name = "Monto Fijo")]
        MontoFijo,
        
        [Display(Name = "Porcentaje")]
        Porcentaje
    }
}