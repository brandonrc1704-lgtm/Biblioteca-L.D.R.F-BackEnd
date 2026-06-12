using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Biblioteca.Dominio.InterfacesAD;

namespace TiendaBatarazo.AccesoDatos.Implementaciones
{
    public class RepositorioAD<TEntity> : IRepositorioAD<TEntity> where TEntity : class
    {
        #region Atributos
        protected readonly DbContext _context;
        private readonly DbSet<TEntity> _dbSet;
        #endregion

        #region Constructor
        public RepositorioAD(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }
        #endregion

        #region Implementación de la Interfaz

        // Ahora acepta object, funciona para int, string, Guid, etc.
        public async Task<TEntity?> ObtenerPorIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>> ObtenerTodosAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> BuscarAsync(Expression<Func<TEntity, bool>> predicado)
        {
            return await _dbSet.Where(predicado).ToListAsync();
        }

        public async Task AgregarAsync(TEntity entidad)
        {
            await _dbSet.AddAsync(entidad);
        }

        public async Task ActualizarAsync(TEntity entidad)
        {
            _dbSet.Update(entidad);
            await Task.CompletedTask;
        }

        public async Task EliminarAsync(object id)
        {
            var entidad = await _dbSet.FindAsync(id);
            if (entidad != null)
            {
                _dbSet.Remove(entidad);
            }
        }

        #endregion
    }
}
