
using Ezel_Market.Models;
namespace Ezel_Market.Services;
public interface ICuponService
{
    Task<List<Cupon>> GetCuponesDisponiblesAsync(string usuarioId, bool esPrimeraCompra = false);
    Task<Cupon> ValidarCuponAsync(string codigo, string usuarioId, decimal totalCompra);
    Task<bool> UsarCuponAsync(int cuponId, string usuarioId);
}
