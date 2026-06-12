using Biblioteca.Dominio.EntidadesTipadas;

namespace Biblioteca.Dominio.InterfaceLN
{
    public interface IDescargaLN
    {
        Task<TDescarga?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<TDescarga>> ObtenerTodosAsync();
        Task CrearDescargaAsync(TDescarga descarga);
        Task EliminarDescargaAsync(int id);
    }
}
