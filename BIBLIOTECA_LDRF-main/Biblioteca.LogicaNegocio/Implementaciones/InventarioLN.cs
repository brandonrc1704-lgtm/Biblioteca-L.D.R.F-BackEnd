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
            ValidarInventario(inventario);
            await ValidarCodigoInternoDisponibleAsync(inventario.CodigoInterno, null);

            var entidad = ToEntidad(inventario);
            var ahora = ObtenerFechaHoraCostaRica();

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

            inventario.IdProducto = entidad.IdProducto;
            inventario.CreadoEn = entidad.CreadoEn;
            inventario.ActualizadoEn = entidad.ActualizadoEn;
        }

        public async Task ActualizarInventarioAsync(TInventario inventario)
        {
            var existente = await _unidadTrabajo.Inventario.ObtenerPorIdAsync(inventario.IdProducto);
            if (existente is null)
            {
                throw new InvalidOperationException("El recurso no existe en inventario.");
            }

            ValidarInventario(inventario);
            await ValidarCodigoInternoDisponibleAsync(inventario.CodigoInterno, inventario.IdProducto);

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
            existente.CreadoEn = existente.CreadoEn == default
                ? ObtenerFechaHoraCostaRica()
                : NormalizarTimestamp(existente.CreadoEn);
            existente.ActualizadoEn = ObtenerFechaHoraCostaRica();

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
            CreadoEn = NormalizarTimestamp(inventario.CreadoEn),
            ActualizadoEn = NormalizarTimestamp(inventario.ActualizadoEn)
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

        private async Task ValidarCodigoInternoDisponibleAsync(string? codigoInterno, int? idActual)
        {
            var codigo = (codigoInterno ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return;
            }

            var codigoNormalizado = codigo.ToLowerInvariant();
            var existentes = await _unidadTrabajo.Inventario.BuscarAsync(item =>
                item.CodigoInterno != null &&
                item.CodigoInterno.ToLower() == codigoNormalizado &&
                (!idActual.HasValue || item.IdProducto != idActual.Value));

            if (existentes.Any())
            {
                throw new InvalidOperationException($"Ya existe un recurso con el codigo interno \"{codigo}\".");
            }
        }

        private static void ValidarInventario(TInventario inventario)
        {
            if (string.IsNullOrWhiteSpace(inventario.Nombre))
            {
                throw new InvalidOperationException("El nombre del recurso es obligatorio.");
            }

            if (!new[] { "libro_fisico", "libro_digital", "equipo_institucion" }.Contains(inventario.TipoObjeto))
            {
                throw new InvalidOperationException("El tipo de objeto no es valido.");
            }

            if (!string.IsNullOrWhiteSpace(inventario.TipoArchivo) &&
                !new[] { "pdf", "epub" }.Contains(inventario.TipoArchivo))
            {
                throw new InvalidOperationException("El tipo de archivo no es valido.");
            }

            if (!new[] { "disponible", "prestado", "danado", "baja" }.Contains(inventario.Estado))
            {
                throw new InvalidOperationException("El estado del recurso no es valido.");
            }

            if (inventario.StockFisico < 0)
            {
                throw new InvalidOperationException("El stock fisico no puede ser negativo.");
            }
        }

        private static DateTime ObtenerFechaHoraCostaRica()
        {
            return DateTime.SpecifyKind(DateTime.UtcNow.AddHours(-6), DateTimeKind.Unspecified);
        }

        private static DateTime NormalizarTimestamp(DateTime fecha)
        {
            return fecha == default ? fecha : DateTime.SpecifyKind(fecha, DateTimeKind.Unspecified);
        }
    }
}
