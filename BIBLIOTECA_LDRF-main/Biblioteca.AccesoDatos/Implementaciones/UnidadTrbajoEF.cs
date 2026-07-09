using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Biblioteca.Dominio.Entidades;
using Biblioteca.Dominio.InterfacesAD;
namespace TiendaBatarazo.AccesoDatos.Implementaciones
{
    public class UnidadTrabajoEF : IUnidadTrabajoEF
    {
        #region Atributos
        private readonly DbContext _context;
        private readonly IConfiguration _configuration;
        private IDbContextTransaction? _transaction;

        private RepositorioAD<Usuario>? _usuarios;
        private RepositorioAD<Inventario>? _inventario;
        private RepositorioAD<Descarga>? _descargas;
        private RepositorioAD<Prestamo>? _prestamos;
        private RepositorioAD<Sancion>? _sanciones;
        private RepositorioAD<HorarioSeccion>? _horariosSecciones;
        private RepositorioAD<RegistroBiblioteca>? _registrosBiblioteca;
        private RepositorioAD<Noticia>? _noticias;
        private RepositorioAD<CorreoEnviado>? _correosEnviados;
        #endregion

        #region Constructor
        public UnidadTrabajoEF(DbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        #endregion

        #region Repositorios
        public IRepositorioAD<Usuario> Usuarios => _usuarios ??= new RepositorioAD<Usuario>(_context);
        public IRepositorioAD<Inventario> Inventario => _inventario ??= new RepositorioAD<Inventario>(_context);
        public IRepositorioAD<Descarga> Descargas => _descargas ??= new RepositorioAD<Descarga>(_context);
        public IRepositorioAD<Prestamo> Prestamos => _prestamos ??= new RepositorioAD<Prestamo>(_context);
        public IRepositorioAD<Sancion> Sanciones => _sanciones ??= new RepositorioAD<Sancion>(_context);
        public IRepositorioAD<HorarioSeccion> HorariosSecciones => _horariosSecciones ??= new RepositorioAD<HorarioSeccion>(_context);
        public IRepositorioAD<RegistroBiblioteca> RegistrosBiblioteca => _registrosBiblioteca ??= new RepositorioAD<RegistroBiblioteca>(_context);
        public IRepositorioAD<Noticia> Noticias => _noticias ??= new RepositorioAD<Noticia>(_context);
        public IRepositorioAD<CorreoEnviado> CorreosEnviados => _correosEnviados ??= new RepositorioAD<CorreoEnviado>(_context);
        #endregion

        #region Métodos
        public int Completar()
        {
            return _context.SaveChanges();
        }

        public void EmpezarTransaccion()
        {
            _transaction = _context.Database.BeginTransaction();
        }

        public void CompletarTran()
        {
            try
            {
                _context.SaveChanges();
                _transaction?.Commit();
            }
            catch
            {
                _transaction?.Rollback();
                throw;
            }
        }

        public void Rollback()
        {
            _transaction?.Rollback();
        }

        public void CerraConexion()
        {
            _context.Database.CloseConnection();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
        #endregion
    }
}
