using System.Collections.Generic;

namespace Ezel_Market.Models
{
    // Este ViewModel nos permite pasar más de un tipo de
    // datos a la vista (la lista de productos Y la lista de categorías).
    public class ClienteCatalogoViewModel
    {
        public IEnumerable<Inventario> Productos { get; set; }
        public IEnumerable<Categorias> Categorias { get; set; }
        
        // Constructor para inicializar las listas
        public ClienteCatalogoViewModel()
        {
            Productos = new List<Inventario>();
            Categorias = new List<Categorias>();
        }
    }
}