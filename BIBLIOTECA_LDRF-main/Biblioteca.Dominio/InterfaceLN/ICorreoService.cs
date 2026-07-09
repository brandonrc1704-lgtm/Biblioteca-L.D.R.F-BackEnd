namespace Biblioteca.Dominio.InterfaceLN
{
    public interface ICorreoService
    {
        Task EnviarAsync(string destino, string asunto, string mensaje);
    }
}
