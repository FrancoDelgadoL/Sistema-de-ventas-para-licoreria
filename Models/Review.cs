using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ezel_Market.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        // Clave foránea hacia Inventario
        [Required]
        public int InventarioId { get; set; }

        [ForeignKey("InventarioId")]
        public Inventario Inventario { get; set; } = null!; // No-nullable para EF Core

        // Usuario que dejó la reseña
        [Required]
        public string Usuario { get; set; } = null!;

        // Calificación entre 1 y 5
        [Range(1, 5)]
        public int Calificacion { get; set; }

        // Comentario de la reseña
        [Required]
        public string Comentario { get; set; } = null!;

        // Fecha de la reseña (por defecto fecha actual)
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
