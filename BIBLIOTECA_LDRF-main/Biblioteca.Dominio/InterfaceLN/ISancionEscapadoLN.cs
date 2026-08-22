using Biblioteca.Dominio.EntidadesTipadas;

namespace Biblioteca.Dominio.InterfaceLN
{
    public interface ISancionEscapadoLN
    {
        Task<TSancionEscapado?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<TSancionEscapado>> ObtenerTodosAsync();
        Task<IEnumerable<TSancionEscapado>> ObtenerPorFechaAsync(DateOnly fecha);
        Task CrearSancionEscapadoAsync(TSancionEscapado sancionEscapado);
        Task ActualizarSancionEscapadoAsync(TSancionEscapado sancionEscapado);
        Task EliminarSancionEscapadoAsync(int id);
    }
}
