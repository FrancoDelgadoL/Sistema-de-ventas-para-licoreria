using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ezel_Market.Models
{
    public class Inventario : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [StringLength(100)]
        [Display(Name = "Nombre del Producto")]
        public string NombreProducto { get; set; }

        public ICollection<Categorias>? Categorias { get; set; }

        public ICollection<CategoriaInventario>? CategoriaInventarios { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor o igual a 0")]
        [Display(Name = "Cantidad en stock")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El precio de compra es obligatorio")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Precio de compra")]
        public decimal PrecioCompra { get; set; }

        [Required(ErrorMessage = "El precio de venta es obligatorio")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Precio Venta Minorista")]
        public decimal PrecioVentaMinorista { get; set; }

        [Required(ErrorMessage = "El precio mayorista es obligatorio")] // <-- AÑADE ESTO
        [Column(TypeName = "decimal(10,2)")] // <-- AÑADE ESTO
        [Display(Name = "Precio Venta Mayorista")] // <-- AÑADE ESTO
        public decimal PrecioVentaMayorista { get; set; }

        // 2. Añade este método de validación
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PrecioVentaMinorista < PrecioVentaMayorista)
            {
                yield return new ValidationResult(
                    "El precio minorista no puede ser menor que el precio mayorista.",
                    new[] { "PrecioVentaMinorista", "PrecioVentaMayorista" }
                );
            }
        }    

        [Display(Name = "Fecha de ingreso")]
        [DataType(DataType.Date)]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        public string Marca { get; set; }
        public decimal? GradoAlcohol { get; set; }

        // Puedes guardar la imagen como URL
        public string Imagen { get; set; }
    }
}
