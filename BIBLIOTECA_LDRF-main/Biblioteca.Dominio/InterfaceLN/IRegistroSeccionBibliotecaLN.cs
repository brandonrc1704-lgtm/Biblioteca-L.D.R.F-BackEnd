using Biblioteca.Dominio.EntidadesTipadas;

namespace Biblioteca.Dominio.InterfaceLN
{
    public interface IRegistroSeccionBibliotecaLN
    {
        Task<TRegistroSeccionBiblioteca?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<TRegistroSeccionBiblioteca>> ObtenerTodosAsync();
        Task<IEnumerable<TRegistroSeccionBiblioteca>> ObtenerPorFechaAsync(DateOnly fecha);
        Task CrearRegistroAsync(TRegistroSeccionBiblioteca registro);
        Task EliminarRegistroAsync(int id);
    }
}
