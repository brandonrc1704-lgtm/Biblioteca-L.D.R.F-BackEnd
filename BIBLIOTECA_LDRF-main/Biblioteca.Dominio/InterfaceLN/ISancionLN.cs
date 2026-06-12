using Biblioteca.Dominio.EntidadesTipadas;

namespace Biblioteca.Dominio.InterfaceLN
{
    public interface ISancionLN
    {
        Task<TSancion?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<TSancion>> ObtenerTodosAsync();
        Task CrearSancionAsync(TSancion sancion);
        Task ActualizarSancionAsync(TSancion sancion);
        Task EliminarSancionAsync(int id);
    }
}
