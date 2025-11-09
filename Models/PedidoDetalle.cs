

using System.ComponentModel.DataAnnotations.Schema;

namespace Ezel_Market.Models
{
    public class PedidoDetalle
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int InventarioId { get; set; }
        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        // Navigation properties
        public virtual Pedido Pedido { get; set; }
        public virtual Inventario Inventario { get; set; }
    }

}