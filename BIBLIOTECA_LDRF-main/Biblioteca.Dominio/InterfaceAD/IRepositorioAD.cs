using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace Biblioteca.Dominio.InterfacesAD
{
    public interface IRepositorioAD<TEntity> where TEntity : class
    {
        Task<TEntity?> ObtenerPorIdAsync(object id);
        Task<IEnumerable<TEntity>> ObtenerTodosAsync();
        Task<IEnumerable<TEntity>> BuscarAsync(Expression<Func<TEntity, bool>> predicado);
        Task AgregarAsync(TEntity entidad);
        Task ActualizarAsync(TEntity entidad);
        Task EliminarAsync(object id);
    }
}
