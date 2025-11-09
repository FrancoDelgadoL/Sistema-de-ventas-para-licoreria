// 1. Crear la clase modelo para la tabla intermedia
namespace Ezel_Market.Models
{
    public class CategoriaInventario
    {
        public int CategoriaId { get; set; }
        public int InventarioId { get; set; }

        // Propiedades de navegación (opcionales pero recomendadas)
        public Categorias Categoria { get; set; }
        public Inventario Inventario { get; set; }
    }
}
