using Biblioteca.Dominio.EntidadesTipadas;

namespace Biblioteca.Dominio.InterfaceLN
{
    public interface IInventarioLN
    {
        Task<TInventario?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<TInventario>> ObtenerTodosAsync();
        Task CrearInventarioAsync(TInventario inventario);
        Task ActualizarInventarioAsync(TInventario inventario);
        Task EliminarInventarioAsync(int id);
    }
}
