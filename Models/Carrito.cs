namespace Ezel_Market.Models
{
    public class Carrito{
        public int Id { get; set; }

        public string UsuarioId { get; set; }
        public int InventarioId { get; set; }
        public int Cantidad { get; set; }
        public DateTime FechaAgregado { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Inventario Inventario { get; set; }
        public virtual Usuarios Usuario { get; set; }
    }
}
