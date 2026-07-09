using Biblioteca.Dominio.EntidadesTipadas;

namespace Biblioteca.Dominio.InterfaceLN
{
    public interface IRegistroBibliotecaLN
    {
        Task<TRegistroBiblioteca?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<TRegistroBiblioteca>> ObtenerTodosAsync();
        Task<IEnumerable<TRegistroBiblioteca>> ObtenerPorFechaAsync(DateOnly fecha);
        Task<IEnumerable<TUsuario>> BuscarEstudiantesAsync(string busqueda);
        Task CrearRegistroAsync(TRegistroBiblioteca registro);
        Task EliminarRegistroAsync(int id);
    }
}
