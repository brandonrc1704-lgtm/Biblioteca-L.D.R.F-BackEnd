using Biblioteca.Dominio.EntidadesTipadas;

namespace Biblioteca.Dominio.InterfaceLN
{
    public interface IHorarioSeccionLN
    {
        Task<THorarioSeccion?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<THorarioSeccion>> ObtenerTodosAsync();
        Task CrearHorarioSeccionAsync(THorarioSeccion horario);
        Task ActualizarHorarioSeccionAsync(THorarioSeccion horario);
        Task EliminarHorarioSeccionAsync(int id);
    }
}
