#nullable enable
public class Productos
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }

    // Relación con Categoría
    public int CategoriasId { get; set; }
    public Categorias? Categoria { get; set; }

    public string? Marca { get; set; }
    public decimal? GradoAlcohol { get; set; }

    // Puedes guardar la imagen como URL
    public string? Imagen { get; set; }
}
