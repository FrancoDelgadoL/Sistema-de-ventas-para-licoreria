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

    // Relación uno a muchos con Productos
    public ICollection<Inventario>? Inventario { get; set; }
    }
}
