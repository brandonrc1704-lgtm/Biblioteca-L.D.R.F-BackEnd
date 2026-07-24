using Biblioteca.Dominio.Entidades;
using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Biblioteca.Dominio.InterfacesAD;

namespace Biblioteca.LogicaNegocio
{
    public class RegistroSeccionBibliotecaLN : IRegistroSeccionBibliotecaLN
    {
        private readonly IUnidadTrabajoEF _unidadTrabajo;

        public RegistroSeccionBibliotecaLN(IUnidadTrabajoEF unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }

        public async Task<TRegistroSeccionBiblioteca?> ObtenerPorIdAsync(int id)
        {
            var registro = await _unidadTrabajo.RegistrosSeccionesBiblioteca.ObtenerPorIdAsync(id);
            if (registro is null)
            {
                return null;
            }

            var tipado = ToTipada(registro);
            await AgregarUsuariosAsync(new List<TRegistroSeccionBiblioteca> { tipado });
            return tipado;
        }

        public async Task<IEnumerable<TRegistroSeccionBiblioteca>> ObtenerTodosAsync()
        {
            var registros = await _unidadTrabajo.RegistrosSeccionesBiblioteca.ObtenerTodosAsync();
            var tipados = registros
                .OrderByDescending(registro => registro.FechaUso)
                .ThenByDescending(registro => registro.CreadoEn)
                .Select(ToTipada)
                .ToList();

            await AgregarUsuariosAsync(tipados);
            return tipados;
        }

        public async Task<IEnumerable<TRegistroSeccionBiblioteca>> ObtenerPorFechaAsync(DateOnly fecha)
        {
            var registros = await _unidadTrabajo.RegistrosSeccionesBiblioteca.BuscarAsync(registro =>
                registro.FechaUso == fecha);

            var tipados = registros
                .OrderByDescending(registro => registro.CreadoEn)
                .Select(ToTipada)
                .ToList();

            await AgregarUsuariosAsync(tipados);
            return tipados;
        }

        public async Task CrearRegistroAsync(TRegistroSeccionBiblioteca registro)
        {
            Validar(registro);

            registro.RegistradoPor = await NormalizarRegistradorAsync(registro.RegistradoPor);

            var ahora = ObtenerAhoraCostaRica();
            var entidad = ToEntidad(registro);
            entidad.FechaUso = entidad.FechaUso == default ? DateOnly.FromDateTime(ahora) : entidad.FechaUso;
            entidad.CreadoEn = entidad.CreadoEn == default ? ahora : entidad.CreadoEn;
            entidad.ActualizadoEn = entidad.ActualizadoEn == default ? ahora : entidad.ActualizadoEn;

            await _unidadTrabajo.RegistrosSeccionesBiblioteca.AgregarAsync(entidad);
            _unidadTrabajo.Completar();

            registro.IdRegistroSeccion = entidad.IdRegistroSeccion;
            registro.FechaUso = entidad.FechaUso;
            registro.CreadoEn = entidad.CreadoEn;
            registro.ActualizadoEn = entidad.ActualizadoEn;
        }

        public async Task EliminarRegistroAsync(int id)
        {
            await _unidadTrabajo.RegistrosSeccionesBiblioteca.EliminarAsync(id);
            _unidadTrabajo.Completar();
        }

        private static void Validar(TRegistroSeccionBiblioteca registro)
        {
            if (string.IsNullOrWhiteSpace(registro.Grado))
            {
                throw new InvalidOperationException("El grado es requerido.");
            }

            if (string.IsNullOrWhiteSpace(registro.Seccion))
            {
                throw new InvalidOperationException("La seccion es requerida.");
            }

            if (string.IsNullOrWhiteSpace(registro.UsoBiblioteca))
            {
                throw new InvalidOperationException("Indica para que se uso la biblioteca.");
            }

            if (string.IsNullOrWhiteSpace(registro.ReservadoPor))
            {
                throw new InvalidOperationException("Indica quien aparto la biblioteca.");
            }
        }

        private static TRegistroSeccionBiblioteca ToTipada(RegistroSeccionBiblioteca registro) => new()
        {
            IdRegistroSeccion = registro.IdRegistroSeccion,
            Grado = registro.Grado,
            Seccion = registro.Seccion,
            FechaUso = registro.FechaUso,
            UsoBiblioteca = registro.UsoBiblioteca,
            ReservadoPor = registro.ReservadoPor,
            RegistradoPor = registro.RegistradoPor,
            Observaciones = registro.Observaciones,
            CreadoEn = registro.CreadoEn,
            ActualizadoEn = registro.ActualizadoEn
        };

        private static RegistroSeccionBiblioteca ToEntidad(TRegistroSeccionBiblioteca registro) => new()
        {
            IdRegistroSeccion = registro.IdRegistroSeccion,
            Grado = registro.Grado.Trim(),
            Seccion = registro.Seccion.Trim(),
            FechaUso = registro.FechaUso,
            UsoBiblioteca = registro.UsoBiblioteca.Trim(),
            ReservadoPor = registro.ReservadoPor.Trim(),
            RegistradoPor = registro.RegistradoPor,
            Observaciones = string.IsNullOrWhiteSpace(registro.Observaciones) ? null : registro.Observaciones.Trim(),
            CreadoEn = registro.CreadoEn,
            ActualizadoEn = registro.ActualizadoEn
        };

        private static TUsuario ToUsuarioTipado(Usuario usuario) => new()
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
            Rol = usuario.Rol,
            Estado = usuario.Estado,
            CreadoEn = usuario.CreadoEn,
            ActualizadoEn = usuario.ActualizadoEn
        };

        private static DateTime ObtenerAhoraCostaRica()
        {
            return DateTime.SpecifyKind(DateTime.UtcNow.AddHours(-6), DateTimeKind.Unspecified);
        }

        private async Task<int?> NormalizarRegistradorAsync(int? registradoPor)
        {
            if (!registradoPor.HasValue || registradoPor.Value <= 0)
            {
                return null;
            }

            var usuario = await _unidadTrabajo.Usuarios.ObtenerPorIdAsync(registradoPor.Value);
            return usuario is null ? null : registradoPor;
        }

        private async Task AgregarUsuariosAsync(List<TRegistroSeccionBiblioteca> registros)
        {
            var idsUsuarios = registros
                .Select(registro => registro.RegistradoPor ?? 0)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (idsUsuarios.Count == 0)
            {
                return;
            }

            var usuarios = (await _unidadTrabajo.Usuarios.BuscarAsync(usuario => idsUsuarios.Contains(usuario.IdUsuario)))
                .ToDictionary(usuario => usuario.IdUsuario);

            foreach (var registro in registros)
            {
                if (registro.RegistradoPor.HasValue &&
                    usuarios.TryGetValue(registro.RegistradoPor.Value, out var registrador))
                {
                    registro.UsuarioRegistrador = ToUsuarioTipado(registrador);
                }
            }
        }
    }
}
