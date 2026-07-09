using Biblioteca.Dominio.Entidades;

namespace Biblioteca.Dominio.InterfacesAD
{
    public interface IUnidadTrabajoEF : IDisposable
    {
        IRepositorioAD<Usuario> Usuarios { get; }
        IRepositorioAD<Inventario> Inventario { get; }
        IRepositorioAD<Descarga> Descargas { get; }
        IRepositorioAD<Prestamo> Prestamos { get; }
        IRepositorioAD<Sancion> Sanciones { get; }
        IRepositorioAD<HorarioSeccion> HorariosSecciones { get; }
        IRepositorioAD<RegistroBiblioteca> RegistrosBiblioteca { get; }
        IRepositorioAD<Noticia> Noticias { get; }
        IRepositorioAD<CorreoEnviado> CorreosEnviados { get; }

        int Completar();
        void EmpezarTransaccion();
        void CompletarTran();
        void Rollback();
        void CerraConexion();
    }
}
