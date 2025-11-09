using System;

namespace Ezel_Market.Models
{
    public class ProductoVistaModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public string Imagen { get; set; }
        public string Categoria { get; set; }
        public int CantidadStock { get; set; }
        public string Marca { get; set; }
        public decimal? GradoAlcohol { get; set; }
    }
}