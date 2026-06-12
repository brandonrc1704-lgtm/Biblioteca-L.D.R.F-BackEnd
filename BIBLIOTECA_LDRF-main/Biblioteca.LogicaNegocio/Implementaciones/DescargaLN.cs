using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Biblioteca.Utilitarios;
using Biblioteca.Dominio.Entidades;
using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Biblioteca.Dominio.InterfacesAD;

namespace Biblioteca.LogicaNegocio
{
    public class DescargaLN : IDescargaLN
    {
        private readonly IUnidadTrabajoEF _unidadTrabajo;

        public DescargaLN(IUnidadTrabajoEF unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }

        public async Task<TDescarga?> ObtenerPorIdAsync(int id)
        {
            var descarga = await _unidadTrabajo.Descargas.ObtenerPorIdAsync(id);
            return descarga is null ? null : ToTipada(descarga);
        }

        public async Task<IEnumerable<TDescarga>> ObtenerTodosAsync()
        {
            var descargas = await _unidadTrabajo.Descargas.ObtenerTodosAsync();
            return descargas.Select(ToTipada);
        }

        public async Task CrearDescargaAsync(TDescarga descarga)
        {
            await _unidadTrabajo.Descargas.AgregarAsync(ToEntidad(descarga));
            _unidadTrabajo.Completar();
        }

        public async Task EliminarDescargaAsync(int id)
        {
            await _unidadTrabajo.Descargas.EliminarAsync(id);
            _unidadTrabajo.Completar();
        }

        private static TDescarga ToTipada(Descarga descarga) => new()
        {
            IdDescarga = descarga.IdDescarga,
            IdUsuario = descarga.IdUsuario,
            IdProducto = descarga.IdProducto,
            FechaDescarga = descarga.FechaDescarga,
            IpDescarga = descarga.IpDescarga,
            Dispositivo = descarga.Dispositivo,
            Observaciones = descarga.Observaciones
        };

        private static Descarga ToEntidad(TDescarga descarga) => new()
        {
            IdDescarga = descarga.IdDescarga,
            IdUsuario = descarga.IdUsuario,
            IdProducto = descarga.IdProducto,
            FechaDescarga = descarga.FechaDescarga,
            IpDescarga = descarga.IpDescarga,
            Dispositivo = descarga.Dispositivo,
            Observaciones = descarga.Observaciones
        };
    }
}
