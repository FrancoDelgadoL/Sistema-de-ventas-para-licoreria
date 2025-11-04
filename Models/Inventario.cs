using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ezel_Market.Models
{
    public class Inventario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [StringLength(100)]
        [Display(Name = "Nombre del Producto")]
        public string NombreProducto { get; set; }

        // Relación con Categoría
        public int CategoriasId { get; set; }
        public Categorias Categoria { get; set; }

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
        [Display(Name = "Precio de venta")]
        public decimal PrecioVenta { get; set; }

        [Display(Name = "Fecha de ingreso")]
        [DataType(DataType.Date)]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        public string Marca { get; set; }
        public decimal? GradoAlcohol { get; set; }

        // Puedes guardar la imagen como URL
        public string Imagen { get; set; }
    }
}
