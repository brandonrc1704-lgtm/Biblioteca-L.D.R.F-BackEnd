using Biblioteca.Dominio.EntidadesTipadas;

namespace Biblioteca.Dominio.InterfaceLN
{
    public interface IPrestamoLN
    {
        Task<TPrestamo?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<TPrestamo>> ObtenerTodosAsync();
        Task CrearPrestamoAsync(TPrestamo prestamo);
        Task ActualizarPrestamoAsync(TPrestamo prestamo);
        Task EliminarPrestamoAsync(int id);
    }
}
