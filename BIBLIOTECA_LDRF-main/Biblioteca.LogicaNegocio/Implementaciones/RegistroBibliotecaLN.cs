using Biblioteca.Dominio.Entidades;
using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Biblioteca.Dominio.InterfacesAD;

namespace Biblioteca.LogicaNegocio
{
    public class RegistroBibliotecaLN : IRegistroBibliotecaLN
    {
        private readonly IUnidadTrabajoEF _unidadTrabajo;

        public RegistroBibliotecaLN(IUnidadTrabajoEF unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }

        public async Task<TRegistroBiblioteca?> ObtenerPorIdAsync(int id)
        {
            var registro = await _unidadTrabajo.RegistrosBiblioteca.ObtenerPorIdAsync(id);
            if (registro is null)
            {
                return null;
            }

            var tipado = ToTipada(registro);
            await AgregarUsuariosAsync(new List<TRegistroBiblioteca> { tipado });
            return tipado;
        }

        public async Task<IEnumerable<TRegistroBiblioteca>> ObtenerTodosAsync()
        {
            var registros = await _unidadTrabajo.RegistrosBiblioteca.ObtenerTodosAsync();
            var tipados = registros
                .OrderByDescending(item => item.FechaHora)
                .Select(ToTipada)
                .ToList();

            await AgregarUsuariosAsync(tipados);
            return tipados;
        }

        public async Task<IEnumerable<TRegistroBiblioteca>> ObtenerPorFechaAsync(DateOnly fecha)
        {
            var inicio = fecha.ToDateTime(TimeOnly.MinValue);
            var fin = fecha.ToDateTime(TimeOnly.MaxValue);
            var registros = await _unidadTrabajo.RegistrosBiblioteca.BuscarAsync(item =>
                item.FechaHora >= inicio && item.FechaHora <= fin);

            var tipados = registros
                .OrderByDescending(item => item.FechaHora)
                .Select(ToTipada)
                .ToList();

            await AgregarUsuariosAsync(tipados);
            return tipados;
        }

        public async Task<IEnumerable<TUsuario>> BuscarEstudiantesAsync(string busqueda)
        {
            var texto = (busqueda ?? string.Empty).Trim().ToLowerInvariant();
            if (texto.Length < 2)
            {
                return Enumerable.Empty<TUsuario>();
            }

            var usuarios = await _unidadTrabajo.Usuarios.BuscarAsync(usuario =>
                usuario.Rol == "estudiante" &&
                usuario.Estado == "activo" &&
                (usuario.Nombres.ToLower().Contains(texto) ||
                 usuario.Apellidos.ToLower().Contains(texto) ||
                 usuario.Correo.ToLower().Contains(texto) ||
                 usuario.Cedula.ToLower().Contains(texto) ||
                 usuario.Credencial.ToLower().Contains(texto)));

            return usuarios
                .OrderBy(usuario => usuario.Apellidos)
                .ThenBy(usuario => usuario.Nombres)
                .Take(20)
                .Select(ToUsuarioTipado);
        }

        public async Task CrearRegistroAsync(TRegistroBiblioteca registro)
        {
            var usuario = await _unidadTrabajo.Usuarios.ObtenerPorIdAsync(registro.IdUsuario)
                ?? throw new InvalidOperationException("El estudiante no existe.");

            if (usuario.Rol != "estudiante")
            {
                throw new InvalidOperationException("Solo se pueden registrar entradas y salidas de estudiantes.");
            }

            if (registro.TipoMovimiento != "entrada" && registro.TipoMovimiento != "salida")
            {
                throw new InvalidOperationException("El tipo de movimiento debe ser entrada o salida.");
            }

            registro.RegistradoPor = await NormalizarRegistradorAsync(registro.RegistradoPor);

            var ahora = ObtenerAhoraCostaRica();
            var entidad = ToEntidad(registro);
            entidad.FechaHora = entidad.FechaHora == default ? ahora : entidad.FechaHora;
            entidad.CreadoEn = entidad.CreadoEn == default ? ahora : entidad.CreadoEn;

            await _unidadTrabajo.RegistrosBiblioteca.AgregarAsync(entidad);
            _unidadTrabajo.Completar();

            registro.IdRegistro = entidad.IdRegistro;
            registro.FechaHora = entidad.FechaHora;
            registro.CreadoEn = entidad.CreadoEn;
        }

        public async Task EliminarRegistroAsync(int id)
        {
            await _unidadTrabajo.RegistrosBiblioteca.EliminarAsync(id);
            _unidadTrabajo.Completar();
        }

        private static TRegistroBiblioteca ToTipada(RegistroBiblioteca registro) => new()
        {
            IdRegistro = registro.IdRegistro,
            IdUsuario = registro.IdUsuario,
            TipoMovimiento = registro.TipoMovimiento,
            FechaHora = registro.FechaHora,
            RegistradoPor = registro.RegistradoPor,
            Observaciones = registro.Observaciones,
            CreadoEn = registro.CreadoEn
        };

        private static RegistroBiblioteca ToEntidad(TRegistroBiblioteca registro) => new()
        {
            IdRegistro = registro.IdRegistro,
            IdUsuario = registro.IdUsuario,
            TipoMovimiento = registro.TipoMovimiento,
            FechaHora = registro.FechaHora,
            RegistradoPor = registro.RegistradoPor,
            Observaciones = registro.Observaciones,
            CreadoEn = registro.CreadoEn
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

        private async Task AgregarUsuariosAsync(List<TRegistroBiblioteca> registros)
        {
            if (registros.Count == 0)
            {
                return;
            }

            var idsUsuarios = registros
                .Select(registro => registro.IdUsuario)
                .Concat(registros.Select(registro => registro.RegistradoPor ?? 0))
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var usuarios = (await _unidadTrabajo.Usuarios.BuscarAsync(usuario => idsUsuarios.Contains(usuario.IdUsuario)))
                .ToDictionary(usuario => usuario.IdUsuario);

            foreach (var registro in registros)
            {
                if (usuarios.TryGetValue(registro.IdUsuario, out var usuario))
                {
                    registro.Usuario = ToUsuarioTipado(usuario);
                }

                if (registro.RegistradoPor.HasValue &&
                    usuarios.TryGetValue(registro.RegistradoPor.Value, out var registrador))
                {
                    registro.UsuarioRegistrador = ToUsuarioTipado(registrador);
                }
            }
        }
    }
}
