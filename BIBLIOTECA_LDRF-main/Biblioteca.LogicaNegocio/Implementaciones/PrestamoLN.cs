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
    public class PrestamoLN : IPrestamoLN
    {
        private readonly IUnidadTrabajoEF _unidadTrabajo;
        private readonly ICorreoService _correoService;

        public PrestamoLN(IUnidadTrabajoEF unidadTrabajo, ICorreoService correoService)
        {
            _unidadTrabajo = unidadTrabajo;
            _correoService = correoService;
        }

        public async Task<TPrestamo?> ObtenerPorIdAsync(int id)
        {
            var prestamo = await _unidadTrabajo.Prestamos.ObtenerPorIdAsync(id);
            return prestamo is null ? null : ToTipada(prestamo);
        }

        public async Task<IEnumerable<TPrestamo>> ObtenerTodosAsync()
        {
            var prestamos = await _unidadTrabajo.Prestamos.ObtenerTodosAsync();
            return prestamos.Select(ToTipada);
        }

        public async Task CrearPrestamoAsync(TPrestamo prestamo)
        {
            var entidad = ToEntidad(prestamo);
            var ahora = DateTime.UtcNow;
            var ahoraCostaRica = ObtenerFechaHoraCostaRica();
            var hoyCostaRica = DateOnly.FromDateTime(ahoraCostaRica);
            var esSolicitud = string.Equals(entidad.Estado, "solicitado", StringComparison.OrdinalIgnoreCase);
            var producto = await _unidadTrabajo.Inventario.ObtenerPorIdAsync(entidad.IdProducto)
                ?? throw new InvalidOperationException("El recurso solicitado no existe en inventario.");

            if (!esSolicitud)
            {
                await AplicarSancionPorEquipoVencidoAsync(entidad.IdUsuario, ahoraCostaRica, hoyCostaRica);
            }

            if (!producto.PermitePrestamo)
            {
                throw new InvalidOperationException("Este recurso no permite prestamos.");
            }

            if (producto.StockFisico <= 0)
            {
                throw new InvalidOperationException($"No hay stock disponible para \"{producto.Nombre}\".");
            }

            if (entidad.FechaPrestamo == default)
            {
                entidad.FechaPrestamo = ahora;
            }
            entidad.FechaPrestamo = AsegurarUtc(entidad.FechaPrestamo);

            if (!esSolicitud && EsEquipoInstitucion(producto))
            {
                entidad.FechaLimite = hoyCostaRica;
                entidad.Observaciones = AgregarObservacionEquipo(entidad.Observaciones);
            }

            if (entidad.CreadoEn == default)
            {
                entidad.CreadoEn = ahora;
            }
            entidad.CreadoEn = AsegurarUtc(entidad.CreadoEn);

            if (entidad.ActualizadoEn == default)
            {
                entidad.ActualizadoEn = ahora;
            }
            entidad.ActualizadoEn = AsegurarUtc(entidad.ActualizadoEn);

            await _unidadTrabajo.Prestamos.AgregarAsync(entidad);
            producto.StockFisico -= 1;
            producto.ActualizadoEn = ahora;
            NormalizarFechasInventario(producto);
            await _unidadTrabajo.Inventario.ActualizarAsync(producto);
            _unidadTrabajo.Completar();

            prestamo.IdPrestamo = entidad.IdPrestamo;
            prestamo.FechaPrestamo = entidad.FechaPrestamo;
            prestamo.FechaLimite = entidad.FechaLimite;
            prestamo.Observaciones = entidad.Observaciones;
            prestamo.CreadoEn = entidad.CreadoEn;
            prestamo.ActualizadoEn = entidad.ActualizadoEn;
        }

        public async Task ActualizarPrestamoAsync(TPrestamo prestamo)
        {
            var existente = await _unidadTrabajo.Prestamos.ObtenerPorIdAsync(prestamo.IdPrestamo);
            if (existente is null)
            {
                throw new InvalidOperationException("El prestamo no existe.");
            }

            var estabaPendiente = existente.FechaDevolucion is null &&
                !string.Equals(existente.Estado, "devuelto", StringComparison.OrdinalIgnoreCase);
            var quedaDevuelto = prestamo.FechaDevolucion is not null ||
                string.Equals(prestamo.Estado, "devuelto", StringComparison.OrdinalIgnoreCase);

            existente.IdUsuario = prestamo.IdUsuario;
            existente.IdProducto = prestamo.IdProducto;
            existente.FechaLimite = prestamo.FechaLimite;
            existente.FechaDevolucion = prestamo.FechaDevolucion;
            existente.Renovaciones = prestamo.Renovaciones;
            existente.Estado = prestamo.Estado;
            existente.EntregadoPor = prestamo.EntregadoPor;
            existente.RecibidoPor = prestamo.RecibidoPor;
            existente.Observaciones = prestamo.Observaciones;
            existente.FechaPrestamo = AsegurarUtc(existente.FechaPrestamo);
            existente.CreadoEn = AsegurarUtc(existente.CreadoEn);
            existente.ActualizadoEn = DateTime.UtcNow;

            if (estabaPendiente && quedaDevuelto)
            {
                existente.Estado = "devuelto";
                existente.FechaDevolucion ??= DateOnly.FromDateTime(ObtenerFechaHoraCostaRica());

                var producto = await _unidadTrabajo.Inventario.ObtenerPorIdAsync(existente.IdProducto);
                if (producto is not null)
                {
                    producto.StockFisico += 1;
                    producto.ActualizadoEn = DateTime.UtcNow;
                    NormalizarFechasInventario(producto);
                    await _unidadTrabajo.Inventario.ActualizarAsync(producto);
                }
            }

            await _unidadTrabajo.Prestamos.ActualizarAsync(existente);
            _unidadTrabajo.Completar();
        }

        private async Task AplicarSancionPorEquipoVencidoAsync(int idUsuario, DateTime ahoraCostaRica, DateOnly hoyCostaRica)
        {
            var sancionesActivas = await _unidadTrabajo.Sanciones.BuscarAsync(sancion =>
                sancion.IdUsuario == idUsuario &&
                sancion.Estado == "activa" &&
                sancion.FechaFin >= hoyCostaRica);

            if (sancionesActivas.Any())
            {
                throw new InvalidOperationException("No puede realizar prestamos porque tiene una sancion activa.");
            }

            var prestamosActivos = await _unidadTrabajo.Prestamos.BuscarAsync(prestamo =>
                prestamo.IdUsuario == idUsuario &&
                prestamo.FechaDevolucion == null &&
                (prestamo.Estado == "activo" || prestamo.Estado == "atrasado"));

            foreach (var prestamoActivo in prestamosActivos)
            {
                var producto = await _unidadTrabajo.Inventario.ObtenerPorIdAsync(prestamoActivo.IdProducto);
                if (producto is null || !EsEquipoInstitucion(producto) || !EquipoEstaVencido(prestamoActivo, ahoraCostaRica))
                {
                    continue;
                }

                var sancion = new Sancion
                {
                    IdUsuario = idUsuario,
                    IdPrestamo = prestamoActivo.IdPrestamo,
                    Motivo = "Equipo institucional no devuelto antes de las 3:00 p. m.",
                    DiasSancion = 3650,
                    FechaInicio = hoyCostaRica,
                    FechaFin = DateOnly.MaxValue,
                    Estado = "activa",
                    Observaciones = "Sancion generada automaticamente. Debe ser levantada por administracion.",
                    CreadoEn = DateTime.UtcNow,
                    ActualizadoEn = DateTime.UtcNow
                };

                prestamoActivo.Estado = "atrasado";
                prestamoActivo.FechaPrestamo = AsegurarUtc(prestamoActivo.FechaPrestamo);
                prestamoActivo.CreadoEn = AsegurarUtc(prestamoActivo.CreadoEn);
                prestamoActivo.ActualizadoEn = DateTime.UtcNow;
                await _unidadTrabajo.Prestamos.ActualizarAsync(prestamoActivo);
                await _unidadTrabajo.Sanciones.AgregarAsync(sancion);
                _unidadTrabajo.Completar();
                await RegistrarCorreoSancionAsync(sancion);

                throw new InvalidOperationException("Tiene un equipo institucional vencido. Se genero una sancion activa hasta que administracion la retire.");
            }
        }

        private async Task RegistrarCorreoSancionAsync(Sancion sancion)
        {
            var usuario = await _unidadTrabajo.Usuarios.ObtenerPorIdAsync(sancion.IdUsuario);
            if (usuario is null || string.IsNullOrWhiteSpace(usuario.Correo))
            {
                return;
            }

            var asunto = "Aviso de sancion - Biblioteca C.D.R.F";
            var mensaje = CrearMensajeSancion(usuario, sancion);
            var correo = new CorreoEnviado
            {
                IdUsuario = usuario.IdUsuario,
                IdSancion = sancion.IdSancion,
                CorreoDestino = usuario.Correo,
                Asunto = asunto,
                Mensaje = mensaje,
                Estado = "pendiente",
                CreadoEn = DateTime.UtcNow
            };

            try
            {
                await _correoService.EnviarAsync(usuario.Correo, asunto, mensaje);
                correo.Estado = "enviado";
                correo.EnviadoEn = DateTime.UtcNow;
            }
            catch (Exception error)
            {
                correo.Estado = "fallido";
                correo.ErrorEnvio = error.Message;
            }

            await _unidadTrabajo.CorreosEnviados.AgregarAsync(correo);
            _unidadTrabajo.Completar();
        }

        private static string CrearMensajeSancion(Usuario usuario, Sancion sancion)
        {
            return $"""
            Hola {usuario.Nombres} {usuario.Apellidos}.

            Se registro una sancion en la biblioteca.

            Motivo: {sancion.Motivo}
            Fecha de inicio: {sancion.FechaInicio:yyyy-MM-dd}
            Fecha de fin: {sancion.FechaFin:yyyy-MM-dd}

            Debe presentarse a la biblioteca para resolver la situacion y solicitar que administracion levante la sancion cuando corresponda.

            Biblioteca C.D.R.F
            """;
        }

        private static bool EquipoEstaVencido(Prestamo prestamo, DateTime ahoraCostaRica)
        {
            var fechaLimite = prestamo.FechaLimite;
            var fechaActual = DateOnly.FromDateTime(ahoraCostaRica);
            var horaActual = TimeOnly.FromDateTime(ahoraCostaRica);

            return fechaLimite < fechaActual ||
                (fechaLimite == fechaActual && horaActual > new TimeOnly(15, 0));
        }

        private static bool EsEquipoInstitucion(Inventario producto)
        {
            return string.Equals(producto.TipoObjeto, "equipo_institucion", StringComparison.OrdinalIgnoreCase);
        }

        private static DateTime ObtenerFechaHoraCostaRica()
        {
            return DateTime.SpecifyKind(DateTime.UtcNow.AddHours(-6), DateTimeKind.Unspecified);
        }

        private static string AgregarObservacionEquipo(string? observaciones)
        {
            const string mensaje = "Devolver equipo institucional antes de las 3:00 p. m. del dia del prestamo.";
            if (string.IsNullOrWhiteSpace(observaciones))
            {
                return mensaje;
            }

            if (observaciones.Contains(mensaje, StringComparison.OrdinalIgnoreCase))
            {
                return observaciones;
            }

            return $"{observaciones} | {mensaje}";
        }

        private static DateTime AsegurarUtc(DateTime fecha)
        {
            if (fecha == default)
            {
                return DateTime.UtcNow;
            }

            return fecha.Kind switch
            {
                DateTimeKind.Utc => fecha,
                DateTimeKind.Local => fecha.ToUniversalTime(),
                _ => DateTime.SpecifyKind(fecha, DateTimeKind.Utc)
            };
        }

        private static void NormalizarFechasInventario(Inventario producto)
        {
            producto.CreadoEn = AsegurarUtc(producto.CreadoEn);
            producto.ActualizadoEn = AsegurarUtc(producto.ActualizadoEn);
        }

        public async Task EliminarPrestamoAsync(int id)
        {
            await _unidadTrabajo.Prestamos.EliminarAsync(id);
            _unidadTrabajo.Completar();
        }

        private static TPrestamo ToTipada(Prestamo prestamo) => new()
        {
            IdPrestamo = prestamo.IdPrestamo,
            IdUsuario = prestamo.IdUsuario,
            IdProducto = prestamo.IdProducto,
            FechaPrestamo = prestamo.FechaPrestamo,
            FechaLimite = prestamo.FechaLimite,
            FechaDevolucion = prestamo.FechaDevolucion,
            Renovaciones = prestamo.Renovaciones,
            Estado = prestamo.Estado,
            EntregadoPor = prestamo.EntregadoPor,
            RecibidoPor = prestamo.RecibidoPor,
            Observaciones = prestamo.Observaciones,
            CreadoEn = prestamo.CreadoEn,
            ActualizadoEn = prestamo.ActualizadoEn
        };

        private static Prestamo ToEntidad(TPrestamo prestamo) => new()
        {
            IdPrestamo = prestamo.IdPrestamo,
            IdUsuario = prestamo.IdUsuario,
            IdProducto = prestamo.IdProducto,
            FechaPrestamo = prestamo.FechaPrestamo,
            FechaLimite = prestamo.FechaLimite,
            FechaDevolucion = prestamo.FechaDevolucion,
            Renovaciones = prestamo.Renovaciones,
            Estado = prestamo.Estado,
            EntregadoPor = prestamo.EntregadoPor,
            RecibidoPor = prestamo.RecibidoPor,
            Observaciones = prestamo.Observaciones,
            CreadoEn = prestamo.CreadoEn,
            ActualizadoEn = prestamo.ActualizadoEn
        };
    }
}
