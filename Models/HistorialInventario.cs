// Archivo: Models/HistorialInventario.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Ezel_Market.Models
{
    public class HistorialInventario
    {
        [Key]
        public int Id { get; set; }

        // Clave foránea para saber de qué producto es este historial
        public int InventarioId { get; set; }
        public Inventario Inventario { get; set; }

        public int CantidadAnterior { get; set; }
        public int CantidadNueva { get; set; }

        // Para saber qué pasó: "Edición Manual", "Venta Realizada", "Compra a Proveedor"
        [Display(Name = "Tipo de Movimiento")]
        public string TipoMovimiento { get; set; }

        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Now;

        // Opcional: Si tuvieras sistema de usuarios, aquí iría quién hizo el cambio
        public string UsuarioId { get; set; } 
    }
}