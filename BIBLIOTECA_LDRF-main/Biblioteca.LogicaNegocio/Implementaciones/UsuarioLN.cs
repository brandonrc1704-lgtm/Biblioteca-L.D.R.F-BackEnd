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
    public class UsuarioLN : IUsuarioLN
    {
        private readonly IUnidadTrabajoEF _unidadTrabajo;

        public UsuarioLN(IUnidadTrabajoEF unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }

        public async Task<TUsuario?> ObtenerPorIdAsync(int id)
        {
            var usuario = await _unidadTrabajo.Usuarios.ObtenerPorIdAsync(id);
            return usuario is null ? null : ToTipada(usuario);
        }

        public async Task<IEnumerable<TUsuario>> ObtenerTodosAsync()
        {
            var usuarios = await _unidadTrabajo.Usuarios.ObtenerTodosAsync();
            return usuarios.Select(ToTipada);
        }

        public async Task CrearUsuarioAsync(TUsuario usuario)
        {
            var entidad = ToEntidad(usuario);
            var ahora = DateTime.UtcNow;

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

            await _unidadTrabajo.Usuarios.AgregarAsync(entidad);
            _unidadTrabajo.Completar();
        }

        public async Task ActualizarUsuarioAsync(TUsuario usuario)
        {
            var usuarioExistente = await _unidadTrabajo.Usuarios.ObtenerPorIdAsync(usuario.IdUsuario);
            if (usuarioExistente is null)
            {
                throw new InvalidOperationException("El usuario no existe.");
            }

            usuarioExistente.Credencial = usuario.Credencial;
            usuarioExistente.Nombres = usuario.Nombres;
            usuarioExistente.Apellidos = usuario.Apellidos;
            usuarioExistente.Cedula = usuario.Cedula;
            usuarioExistente.Correo = usuario.Correo;
            usuarioExistente.Telefono = usuario.Telefono;
            usuarioExistente.FechaNacimiento = usuario.FechaNacimiento;
            usuarioExistente.Seccion = usuario.Seccion;
            usuarioExistente.Clave = usuario.Clave;
            usuarioExistente.Rol = usuario.Rol;
            usuarioExistente.Estado = usuario.Estado;
            usuarioExistente.TokenActivo = usuario.TokenActivo;
            usuarioExistente.CreadoEn = AsegurarUtc(usuarioExistente.CreadoEn);
            usuarioExistente.ActualizadoEn = DateTime.UtcNow;

            await _unidadTrabajo.Usuarios.ActualizarAsync(usuarioExistente);
            _unidadTrabajo.Completar();
        }

        public async Task EliminarUsuarioAsync(int id)
        {
            _unidadTrabajo.EmpezarTransaccion();

            try
            {
                var prestamosDelUsuario = (await _unidadTrabajo.Prestamos.BuscarAsync(prestamo =>
                    prestamo.IdUsuario == id)).ToList();
                var prestamosDondeParticipa = (await _unidadTrabajo.Prestamos.BuscarAsync(prestamo =>
                    prestamo.EntregadoPor == id || prestamo.RecibidoPor == id)).ToList();
                var idsPrestamosDelUsuario = prestamosDelUsuario.Select(prestamo => prestamo.IdPrestamo).ToHashSet();

                var sanciones = (await _unidadTrabajo.Sanciones.BuscarAsync(sancion =>
                    sancion.IdUsuario == id ||
                    (sancion.IdPrestamo.HasValue && idsPrestamosDelUsuario.Contains(sancion.IdPrestamo.Value)))).ToList();
                foreach (var sancion in sanciones)
                {
                    await _unidadTrabajo.Sanciones.EliminarAsync(sancion.IdSancion);
                }

                var descargas = await _unidadTrabajo.Descargas.BuscarAsync(descarga => descarga.IdUsuario == id);
                foreach (var descarga in descargas)
                {
                    await _unidadTrabajo.Descargas.EliminarAsync(descarga.IdDescarga);
                }

                foreach (var prestamo in prestamosDondeParticipa)
                {
                    if (prestamo.EntregadoPor == id)
                    {
                        prestamo.EntregadoPor = null;
                    }

                    if (prestamo.RecibidoPor == id)
                    {
                        prestamo.RecibidoPor = null;
                    }

                    prestamo.FechaPrestamo = AsegurarUtc(prestamo.FechaPrestamo);
                    prestamo.CreadoEn = AsegurarUtc(prestamo.CreadoEn);
                    prestamo.ActualizadoEn = DateTime.UtcNow;
                    await _unidadTrabajo.Prestamos.ActualizarAsync(prestamo);
                }

                foreach (var prestamo in prestamosDelUsuario)
                {
                    await _unidadTrabajo.Prestamos.EliminarAsync(prestamo.IdPrestamo);
                }

                await _unidadTrabajo.Usuarios.EliminarAsync(id);
                _unidadTrabajo.CompletarTran();
            }
            catch
            {
                _unidadTrabajo.Rollback();
                throw;
            }
        }

        private static TUsuario ToTipada(Usuario usuario) => new()
        {
            IdUsuario = usuario.IdUsuario,
            Credencial = usuario.Credencial,
            Nombres = usuario.Nombres,
            Apellidos = usuario.Apellidos,
            Cedula = usuario.Cedula,
            Correo = usuario.Correo,
            Telefono = usuario.Telefono,
            FechaNacimiento = usuario.FechaNacimiento,
            Seccion = usuario.Seccion,
            Clave = usuario.Clave,
            Rol = usuario.Rol,
            Estado = usuario.Estado,
            CreadoEn = usuario.CreadoEn,
            ActualizadoEn = usuario.ActualizadoEn,
            TokenActivo = usuario.TokenActivo
        };

        private static Usuario ToEntidad(TUsuario usuario) => new()
        {
            IdUsuario = usuario.IdUsuario,
            Credencial = usuario.Credencial,
            Nombres = usuario.Nombres,
            Apellidos = usuario.Apellidos,
            Cedula = usuario.Cedula,
            Correo = usuario.Correo,
            Telefono = usuario.Telefono,
            FechaNacimiento = usuario.FechaNacimiento,
            Seccion = usuario.Seccion,
            Clave = usuario.Clave,
            Rol = usuario.Rol,
            Estado = usuario.Estado,
            CreadoEn = usuario.CreadoEn,
            ActualizadoEn = usuario.ActualizadoEn,
            TokenActivo = usuario.TokenActivo
        };

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
    }
}
