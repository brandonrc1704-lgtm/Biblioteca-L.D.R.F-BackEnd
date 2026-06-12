using Biblioteca.Dominio.EntidadesTipadas;

namespace Biblioteca.Dominio.InterfaceLN
{
    public interface IUsuarioLN
    {
        Task<TUsuario?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<TUsuario>> ObtenerTodosAsync();
        Task CrearUsuarioAsync(TUsuario usuario);
        Task ActualizarUsuarioAsync(TUsuario usuario);
        Task EliminarUsuarioAsync(int id);
    }
}
