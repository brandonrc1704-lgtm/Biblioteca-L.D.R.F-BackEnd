using Biblioteca.Dominio.EntidadesTipadas;

namespace Biblioteca.Dominio.InterfaceLN
{
    public interface INoticiaLN
    {
        Task<TNoticia?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<TNoticia>> ObtenerTodasAsync(bool soloPublicadas);
        Task CrearNoticiaAsync(TNoticia noticia);
        Task ActualizarNoticiaAsync(TNoticia noticia);
        Task EliminarNoticiaAsync(int id);
    }
}
