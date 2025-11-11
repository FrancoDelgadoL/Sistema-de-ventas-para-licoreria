using System;
using System.ComponentModel.DataAnnotations;

namespace Ezel_Market.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Bebida { get; set; }

        [Required]
        public string Usuario { get; set; }

        [Range(1, 5)]
        public int Calificacion { get; set; }

        [Required]
        public string Comentario { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
