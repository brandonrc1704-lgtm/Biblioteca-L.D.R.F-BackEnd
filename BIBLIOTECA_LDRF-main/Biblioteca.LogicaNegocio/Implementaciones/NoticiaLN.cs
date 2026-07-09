using Biblioteca.Dominio.Entidades;
using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Biblioteca.Dominio.InterfacesAD;

namespace Biblioteca.LogicaNegocio
{
    public class NoticiaLN : INoticiaLN
    {
        private readonly IUnidadTrabajoEF _unidadTrabajo;

        public NoticiaLN(IUnidadTrabajoEF unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }

        public async Task<TNoticia?> ObtenerPorIdAsync(int id)
        {
            var noticia = await _unidadTrabajo.Noticias.ObtenerPorIdAsync(id);
            return noticia is null ? null : ToTipada(noticia);
        }

        public async Task<IEnumerable<TNoticia>> ObtenerTodasAsync(bool soloPublicadas)
        {
            var noticias = soloPublicadas
                ? await _unidadTrabajo.Noticias.BuscarAsync(noticia => noticia.Estado == "publicada")
                : await _unidadTrabajo.Noticias.ObtenerTodosAsync();

            return noticias
                .OrderByDescending(noticia => noticia.CreadoEn)
                .Select(ToTipada);
        }

        public async Task CrearNoticiaAsync(TNoticia noticia)
        {
            Validar(noticia);

            var entidad = ToEntidad(noticia);
            var ahora = DateTime.UtcNow;
            entidad.CreadoEn = entidad.CreadoEn == default ? ahora : entidad.CreadoEn;
            entidad.ActualizadoEn = entidad.ActualizadoEn == default ? ahora : entidad.ActualizadoEn;

            await _unidadTrabajo.Noticias.AgregarAsync(entidad);
            _unidadTrabajo.Completar();

            noticia.IdNoticia = entidad.IdNoticia;
            noticia.CreadoEn = entidad.CreadoEn;
            noticia.ActualizadoEn = entidad.ActualizadoEn;
        }

        public async Task ActualizarNoticiaAsync(TNoticia noticia)
        {
            Validar(noticia);

            var existente = await _unidadTrabajo.Noticias.ObtenerPorIdAsync(noticia.IdNoticia);
            if (existente is null)
            {
                throw new InvalidOperationException("La noticia no existe.");
            }

            existente.Titulo = noticia.Titulo.Trim();
            existente.Mensaje = noticia.Mensaje.Trim();
            existente.Tipo = noticia.Tipo;
            existente.Estado = noticia.Estado;
            existente.ActualizadoEn = DateTime.UtcNow;

            await _unidadTrabajo.Noticias.ActualizarAsync(existente);
            _unidadTrabajo.Completar();
        }

        public async Task EliminarNoticiaAsync(int id)
        {
            await _unidadTrabajo.Noticias.EliminarAsync(id);
            _unidadTrabajo.Completar();
        }

        private static void Validar(TNoticia noticia)
        {
            if (string.IsNullOrWhiteSpace(noticia.Titulo))
            {
                throw new InvalidOperationException("El titulo de la noticia es requerido.");
            }

            if (string.IsNullOrWhiteSpace(noticia.Mensaje))
            {
                throw new InvalidOperationException("El mensaje de la noticia es requerido.");
            }

            if (!new[] { "general", "actividad", "aviso", "urgente" }.Contains(noticia.Tipo))
            {
                throw new InvalidOperationException("El tipo de noticia no es valido.");
            }

            if (!new[] { "publicada", "oculta" }.Contains(noticia.Estado))
            {
                throw new InvalidOperationException("El estado de la noticia no es valido.");
            }
        }

        private static TNoticia ToTipada(Noticia noticia) => new()
        {
            IdNoticia = noticia.IdNoticia,
            Titulo = noticia.Titulo,
            Mensaje = noticia.Mensaje,
            Tipo = noticia.Tipo,
            Estado = noticia.Estado,
            CreadoPor = noticia.CreadoPor,
            CreadoEn = noticia.CreadoEn,
            ActualizadoEn = noticia.ActualizadoEn
        };

        private static Noticia ToEntidad(TNoticia noticia) => new()
        {
            IdNoticia = noticia.IdNoticia,
            Titulo = noticia.Titulo.Trim(),
            Mensaje = noticia.Mensaje.Trim(),
            Tipo = noticia.Tipo,
            Estado = noticia.Estado,
            CreadoPor = noticia.CreadoPor,
            CreadoEn = noticia.CreadoEn,
            ActualizadoEn = noticia.ActualizadoEn
        };
    }
}
