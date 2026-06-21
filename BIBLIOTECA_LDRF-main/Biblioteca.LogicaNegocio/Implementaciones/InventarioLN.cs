using Biblioteca.Dominio.Entidades;
using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Biblioteca.Dominio.InterfacesAD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Biblioteca.Utilitarios;

namespace Biblioteca.LogicaNegocio
{
    public class InventarioLN : IInventarioLN
    {
        private readonly IUnidadTrabajoEF _unidadTrabajo;

        public InventarioLN(IUnidadTrabajoEF unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }

        public async Task<TInventario?> ObtenerPorIdAsync(int id)
        {
            var inventario = await _unidadTrabajo.Inventario.ObtenerPorIdAsync(id);
            return inventario is null ? null : ToTipada(inventario);
        }

        public async Task<IEnumerable<TInventario>> ObtenerTodosAsync()
        {
            var inventario = await _unidadTrabajo.Inventario.ObtenerTodosAsync();
            return inventario.Select(ToTipada);
        }

        public async Task CrearInventarioAsync(TInventario inventario)
        {
            var entidad = ToEntidad(inventario);
            var ahora = DateTime.UtcNow;

            if (entidad.CreadoEn == default)
            {
                entidad.CreadoEn = ahora;
            }

            if (entidad.ActualizadoEn == default)
            {
                entidad.ActualizadoEn = ahora;
            }

            await _unidadTrabajo.Inventario.AgregarAsync(entidad);
            _unidadTrabajo.Completar();
        }

        public async Task ActualizarInventarioAsync(TInventario inventario)
        {
            var existente = await _unidadTrabajo.Inventario.ObtenerPorIdAsync(inventario.IdProducto);
            if (existente is null)
            {
                throw new InvalidOperationException("El recurso no existe en inventario.");
            }

            existente.CodigoInterno = inventario.CodigoInterno;
            existente.TipoObjeto = inventario.TipoObjeto;
            existente.Nombre = inventario.Nombre;
            existente.Descripcion = inventario.Descripcion;
            existente.Autor = inventario.Autor;
            existente.Editorial = inventario.Editorial;
            existente.NumeroSaga = inventario.NumeroSaga;
            existente.Edicion = inventario.Edicion;
            existente.Categoria = inventario.Categoria;
            existente.Ubicacion = inventario.Ubicacion;
            existente.Portada = inventario.Portada;
            existente.ArchivoUrl = inventario.ArchivoUrl;
            existente.TipoArchivo = inventario.TipoArchivo;
            existente.TamanoMb = inventario.TamanoMb;
            existente.StockFisico = inventario.StockFisico;
            existente.PermitePrestamo = inventario.PermitePrestamo;
            existente.PermiteDescarga = inventario.PermiteDescarga;
            existente.Visibilidad = NormalizarVisibilidad(inventario.Visibilidad);
            existente.Estado = inventario.Estado;
            existente.CreadoEn = existente.CreadoEn == default ? DateTime.UtcNow : existente.CreadoEn;
            existente.ActualizadoEn = DateTime.UtcNow;

            _unidadTrabajo.Completar();
        }

        public async Task EliminarInventarioAsync(int id)
        {
            _unidadTrabajo.EmpezarTransaccion();

            try
            {
                var prestamos = (await _unidadTrabajo.Prestamos.BuscarAsync(prestamo =>
                    prestamo.IdProducto == id)).ToList();
                var idsPrestamos = prestamos.Select(prestamo => prestamo.IdPrestamo).ToHashSet();

                var sanciones = await _unidadTrabajo.Sanciones.BuscarAsync(sancion =>
                    sancion.IdPrestamo.HasValue && idsPrestamos.Contains(sancion.IdPrestamo.Value));
                foreach (var sancion in sanciones)
                {
                    await _unidadTrabajo.Sanciones.EliminarAsync(sancion.IdSancion);
                }

                var descargas = await _unidadTrabajo.Descargas.BuscarAsync(descarga => descarga.IdProducto == id);
                foreach (var descarga in descargas)
                {
                    await _unidadTrabajo.Descargas.EliminarAsync(descarga.IdDescarga);
                }

                foreach (var prestamo in prestamos)
                {
                    await _unidadTrabajo.Prestamos.EliminarAsync(prestamo.IdPrestamo);
                }

                await _unidadTrabajo.Inventario.EliminarAsync(id);
                _unidadTrabajo.CompletarTran();
            }
            catch
            {
                _unidadTrabajo.Rollback();
                throw;
            }
        }

        private static TInventario ToTipada(Inventario inventario) => new()
        {
            IdProducto = inventario.IdProducto,
            CodigoInterno = inventario.CodigoInterno,
            TipoObjeto = inventario.TipoObjeto,
            Nombre = inventario.Nombre,
            Descripcion = inventario.Descripcion,
            Autor = inventario.Autor,
            Editorial = inventario.Editorial,
            NumeroSaga = inventario.NumeroSaga,
            Edicion = inventario.Edicion,
            Categoria = inventario.Categoria,
            Ubicacion = inventario.Ubicacion,
            Portada = inventario.Portada,
            ArchivoUrl = inventario.ArchivoUrl,
            TipoArchivo = inventario.TipoArchivo,
            TamanoMb = inventario.TamanoMb,
            StockFisico = inventario.StockFisico,
            PermitePrestamo = inventario.PermitePrestamo,
            PermiteDescarga = inventario.PermiteDescarga,
            Visibilidad = NormalizarVisibilidad(inventario.Visibilidad),
            Estado = inventario.Estado,
            CreadoEn = inventario.CreadoEn,
            ActualizadoEn = inventario.ActualizadoEn
        };

        private static Inventario ToEntidad(TInventario inventario) => new()
        {
            IdProducto = inventario.IdProducto,
            CodigoInterno = inventario.CodigoInterno,
            TipoObjeto = inventario.TipoObjeto,
            Nombre = inventario.Nombre,
            Descripcion = inventario.Descripcion,
            Autor = inventario.Autor,
            Editorial = inventario.Editorial,
            NumeroSaga = inventario.NumeroSaga,
            Edicion = inventario.Edicion,
            Categoria = inventario.Categoria,
            Ubicacion = inventario.Ubicacion,
            Portada = inventario.Portada,
            ArchivoUrl = inventario.ArchivoUrl,
            TipoArchivo = inventario.TipoArchivo,
            TamanoMb = inventario.TamanoMb,
            StockFisico = inventario.StockFisico,
            PermitePrestamo = inventario.PermitePrestamo,
            PermiteDescarga = inventario.PermiteDescarga,
            Visibilidad = NormalizarVisibilidad(inventario.Visibilidad),
            Estado = inventario.Estado,
            CreadoEn = inventario.CreadoEn,
            ActualizadoEn = inventario.ActualizadoEn
        };

        private static string NormalizarVisibilidad(string? visibilidad)
        {
            return (visibilidad ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "estudiantes" => "estudiantes",
                "personal" => "personal",
                _ => "todos"
            };
        }
    }
}
