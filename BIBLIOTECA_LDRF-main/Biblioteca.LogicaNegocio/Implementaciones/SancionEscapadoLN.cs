using Biblioteca.Dominio.Entidades;
using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Biblioteca.Dominio.InterfacesAD;

namespace Biblioteca.LogicaNegocio
{
    public class SancionEscapadoLN : ISancionEscapadoLN
    {
        private readonly IUnidadTrabajoEF _unidadTrabajo;

        public SancionEscapadoLN(IUnidadTrabajoEF unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }

        public async Task<TSancionEscapado?> ObtenerPorIdAsync(int id)
        {
            var registro = await _unidadTrabajo.SancionesEscapado.ObtenerPorIdAsync(id);
            return registro is null ? null : ToTipada(registro);
        }

        public async Task<IEnumerable<TSancionEscapado>> ObtenerTodosAsync()
        {
            var registros = await _unidadTrabajo.SancionesEscapado.ObtenerTodosAsync();
            return registros.Select(ToTipada)
                .OrderByDescending(registro => registro.Fecha)
                .ThenByDescending(registro => registro.Hora);
        }

        public async Task<IEnumerable<TSancionEscapado>> ObtenerPorFechaAsync(DateOnly fecha)
        {
            var registros = await _unidadTrabajo.SancionesEscapado.BuscarAsync(registro => registro.Fecha == fecha);
            return registros.Select(ToTipada)
                .OrderByDescending(registro => registro.Hora);
        }

        public async Task CrearSancionEscapadoAsync(TSancionEscapado sancionEscapado)
        {
            var existente = (await _unidadTrabajo.SancionesEscapado.BuscarAsync(registro =>
                registro.IdUsuario == sancionEscapado.IdUsuario &&
                registro.IdHorario == sancionEscapado.IdHorario &&
                registro.Fecha == sancionEscapado.Fecha &&
                registro.Estado == "pendiente")).FirstOrDefault();

            if (existente is not null)
            {
                sancionEscapado.IdEscapado = existente.IdEscapado;
                return;
            }

            var entidad = ToEntidad(sancionEscapado);
            entidad.Estado = string.IsNullOrWhiteSpace(entidad.Estado) ? "pendiente" : entidad.Estado;
            entidad.Observaciones = string.IsNullOrWhiteSpace(entidad.Observaciones)
                ? "Se escapo este dia: entro a biblioteca durante un bloque marcado como clase."
                : entidad.Observaciones;

            await _unidadTrabajo.SancionesEscapado.AgregarAsync(entidad);
            _unidadTrabajo.Completar();
            sancionEscapado.IdEscapado = entidad.IdEscapado;
        }

        public async Task ActualizarSancionEscapadoAsync(TSancionEscapado sancionEscapado)
        {
            var existente = await _unidadTrabajo.SancionesEscapado.ObtenerPorIdAsync(sancionEscapado.IdEscapado);
            if (existente is null)
            {
                return;
            }

            existente.IdUsuario = sancionEscapado.IdUsuario;
            existente.IdHorario = sancionEscapado.IdHorario;
            existente.Fecha = sancionEscapado.Fecha;
            existente.Hora = sancionEscapado.Hora;
            existente.DiaSemana = sancionEscapado.DiaSemana;
            existente.Seccion = sancionEscapado.Seccion;
            existente.HoraInicio = sancionEscapado.HoraInicio;
            existente.HoraFin = sancionEscapado.HoraFin;
            existente.Materia = sancionEscapado.Materia;
            existente.Docente = sancionEscapado.Docente;
            existente.Aula = sancionEscapado.Aula;
            existente.Estado = sancionEscapado.Estado;
            existente.Observaciones = sancionEscapado.Observaciones;
            existente.IdSancion = sancionEscapado.IdSancion;

            await _unidadTrabajo.SancionesEscapado.ActualizarAsync(existente);
            _unidadTrabajo.Completar();
        }

        public async Task EliminarSancionEscapadoAsync(int id)
        {
            await _unidadTrabajo.SancionesEscapado.EliminarAsync(id);
            _unidadTrabajo.Completar();
        }

        private static TSancionEscapado ToTipada(SancionEscapado registro) => new()
        {
            IdEscapado = registro.IdEscapado,
            IdUsuario = registro.IdUsuario,
            IdHorario = registro.IdHorario,
            Fecha = registro.Fecha,
            Hora = registro.Hora,
            DiaSemana = registro.DiaSemana,
            Seccion = registro.Seccion,
            HoraInicio = registro.HoraInicio,
            HoraFin = registro.HoraFin,
            Materia = registro.Materia,
            Docente = registro.Docente,
            Aula = registro.Aula,
            Estado = registro.Estado,
            Observaciones = registro.Observaciones,
            IdSancion = registro.IdSancion,
            CreadoEn = registro.CreadoEn,
            ActualizadoEn = registro.ActualizadoEn
        };

        private static SancionEscapado ToEntidad(TSancionEscapado registro) => new()
        {
            IdEscapado = registro.IdEscapado,
            IdUsuario = registro.IdUsuario,
            IdHorario = registro.IdHorario,
            Fecha = registro.Fecha,
            Hora = registro.Hora,
            DiaSemana = registro.DiaSemana,
            Seccion = registro.Seccion,
            HoraInicio = registro.HoraInicio,
            HoraFin = registro.HoraFin,
            Materia = registro.Materia,
            Docente = registro.Docente,
            Aula = registro.Aula,
            Estado = registro.Estado,
            Observaciones = registro.Observaciones,
            IdSancion = registro.IdSancion,
            CreadoEn = registro.CreadoEn,
            ActualizadoEn = registro.ActualizadoEn
        };
    }
}
