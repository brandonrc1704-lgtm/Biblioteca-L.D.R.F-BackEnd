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
    public class HorarioSeccionLN : IHorarioSeccionLN
    {
        private readonly IUnidadTrabajoEF _unidadTrabajo;

        public HorarioSeccionLN(IUnidadTrabajoEF unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }

        public async Task<THorarioSeccion?> ObtenerPorIdAsync(int id)
        {
            var horario = await _unidadTrabajo.HorariosSecciones.ObtenerPorIdAsync(id);
            return horario is null ? null : ToTipada(horario);
        }

        public async Task<IEnumerable<THorarioSeccion>> ObtenerTodosAsync()
        {
            var horarios = await _unidadTrabajo.HorariosSecciones.ObtenerTodosAsync();
            return horarios.Select(ToTipada);
        }

        public async Task CrearHorarioSeccionAsync(THorarioSeccion horario)
        {
            await _unidadTrabajo.HorariosSecciones.AgregarAsync(ToEntidad(horario));
            _unidadTrabajo.Completar();
        }

        public async Task ActualizarHorarioSeccionAsync(THorarioSeccion horario)
        {
            await _unidadTrabajo.HorariosSecciones.ActualizarAsync(ToEntidad(horario));
            _unidadTrabajo.Completar();
        }

        public async Task EliminarHorarioSeccionAsync(int id)
        {
            await _unidadTrabajo.HorariosSecciones.EliminarAsync(id);
            _unidadTrabajo.Completar();
        }

        private static THorarioSeccion ToTipada(HorarioSeccion horario) => new()
        {
            IdHorario = horario.IdHorario,
            Seccion = horario.Seccion,
            DiaSemana = horario.DiaSemana,
            HoraInicio = horario.HoraInicio,
            HoraFin = horario.HoraFin,
            Materia = horario.Materia,
            Docente = horario.Docente,
            Aula = horario.Aula,
            Estado = horario.Estado,
            Observaciones = horario.Observaciones,
            CreadoEn = horario.CreadoEn,
            ActualizadoEn = horario.ActualizadoEn
        };

        private static HorarioSeccion ToEntidad(THorarioSeccion horario) => new()
        {
            IdHorario = horario.IdHorario,
            Seccion = horario.Seccion,
            DiaSemana = horario.DiaSemana,
            HoraInicio = horario.HoraInicio,
            HoraFin = horario.HoraFin,
            Materia = horario.Materia,
            Docente = horario.Docente,
            Aula = horario.Aula,
            Estado = horario.Estado,
            Observaciones = horario.Observaciones,
            CreadoEn = horario.CreadoEn,
            ActualizadoEn = horario.ActualizadoEn
        };
    }
}
