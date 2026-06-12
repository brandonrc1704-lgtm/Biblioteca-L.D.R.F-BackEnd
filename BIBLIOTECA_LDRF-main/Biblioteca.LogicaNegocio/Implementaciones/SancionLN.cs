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
    public class SancionLN : ISancionLN
    {
        private readonly IUnidadTrabajoEF _unidadTrabajo;

        public SancionLN(IUnidadTrabajoEF unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }

        public async Task<TSancion?> ObtenerPorIdAsync(int id)
        {
            var sancion = await _unidadTrabajo.Sanciones.ObtenerPorIdAsync(id);
            return sancion is null ? null : ToTipada(sancion);
        }

        public async Task<IEnumerable<TSancion>> ObtenerTodosAsync()
        {
            var sanciones = await _unidadTrabajo.Sanciones.ObtenerTodosAsync();
            return sanciones.Select(ToTipada);
        }

        public async Task CrearSancionAsync(TSancion sancion)
        {
            var entidad = ToEntidad(sancion);
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var ahora = DateTime.UtcNow;

            if (entidad.FechaInicio == default)
            {
                entidad.FechaInicio = hoy;
            }

            if (entidad.FechaFin == default)
            {
                entidad.FechaFin = hoy.AddDays(entidad.DiasSancion);
            }

            if (entidad.CreadoEn == default)
            {
                entidad.CreadoEn = ahora;
            }

            if (entidad.ActualizadoEn == default)
            {
                entidad.ActualizadoEn = ahora;
            }

            await _unidadTrabajo.Sanciones.AgregarAsync(entidad);
            _unidadTrabajo.Completar();
        }

        public async Task ActualizarSancionAsync(TSancion sancion)
        {
            var existente = await _unidadTrabajo.Sanciones.ObtenerPorIdAsync(sancion.IdSancion);
            if (existente is null)
            {
                throw new InvalidOperationException("La sancion no existe.");
            }

            existente.IdUsuario = sancion.IdUsuario;
            existente.IdPrestamo = sancion.IdPrestamo;
            existente.Motivo = sancion.Motivo;
            existente.DiasSancion = sancion.DiasSancion;
            existente.FechaInicio = sancion.FechaInicio == default ? existente.FechaInicio : sancion.FechaInicio;
            existente.FechaFin = sancion.FechaFin == default ? existente.FechaFin : sancion.FechaFin;
            existente.Estado = sancion.Estado;
            existente.Observaciones = sancion.Observaciones;
            existente.CreadoEn = existente.CreadoEn == default ? DateTime.UtcNow : existente.CreadoEn;
            existente.ActualizadoEn = DateTime.UtcNow;

            _unidadTrabajo.Completar();
        }

        public async Task EliminarSancionAsync(int id)
        {
            await _unidadTrabajo.Sanciones.EliminarAsync(id);
            _unidadTrabajo.Completar();
        }

        private static TSancion ToTipada(Sancion sancion) => new()
        {
            IdSancion = sancion.IdSancion,
            IdUsuario = sancion.IdUsuario,
            IdPrestamo = sancion.IdPrestamo,
            Motivo = sancion.Motivo,
            DiasSancion = sancion.DiasSancion,
            FechaInicio = sancion.FechaInicio,
            FechaFin = sancion.FechaFin,
            Estado = sancion.Estado,
            Observaciones = sancion.Observaciones,
            CreadoEn = sancion.CreadoEn,
            ActualizadoEn = sancion.ActualizadoEn
        };

        private static Sancion ToEntidad(TSancion sancion) => new()
        {
            IdSancion = sancion.IdSancion,
            IdUsuario = sancion.IdUsuario,
            IdPrestamo = sancion.IdPrestamo,
            Motivo = sancion.Motivo,
            DiasSancion = sancion.DiasSancion,
            FechaInicio = sancion.FechaInicio,
            FechaFin = sancion.FechaFin,
            Estado = sancion.Estado,
            Observaciones = sancion.Observaciones,
            CreadoEn = sancion.CreadoEn,
            ActualizadoEn = sancion.ActualizadoEn
        };
    }
}
