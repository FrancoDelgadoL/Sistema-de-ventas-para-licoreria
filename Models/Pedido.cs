using System.ComponentModel.DataAnnotations.Schema;

namespace Ezel_Market.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; }
        public DateTime FechaPedido { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal IGV { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Descuento { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public string Estado { get; set; } = "Pendiente";
        public string DireccionEnvio { get; set; }
        public string MetodoPago { get; set; }

        // Navigation properties
        public virtual ICollection<PedidoDetalle> Detalles { get; set; }
    }
}