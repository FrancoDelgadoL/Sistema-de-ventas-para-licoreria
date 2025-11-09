using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Ezel_Market.Models

{
#nullable enable
    public class Categorias
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public string? Descripcion { get; set; }

        public ICollection<Categorias>? Categoria { get; set; }

        // Sigue siendo ICollection, ahora para Muchos a Muchos
        public ICollection<Inventario>? Inventario { get; set; }
    }
}
