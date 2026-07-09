using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaBatarazo.AccesoDatos.Context;

namespace BibliotecaLDRFApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administracion")]
    public class ReporteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReporteController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("mensual")]
        public async Task<ActionResult<ReporteMensualResponse>> GetMensual([FromQuery] int anio, [FromQuery] int mes)
        {
            if (anio < 2000 || mes < 1 || mes > 12)
            {
                return BadRequest(new { message = "Debe indicar un anio y mes validos." });
            }

            var fechaInicio = new DateTime(anio, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1);

            var masSolicitadosRaw = await _context.Prestamos
                .AsNoTracking()
                .Where(prestamo => prestamo.FechaPrestamo >= fechaInicio && prestamo.FechaPrestamo < fechaFin)
                .Join(
                    _context.Inventario.AsNoTracking(),
                    prestamo => prestamo.IdProducto,
                    producto => producto.IdProducto,
                    (prestamo, producto) => producto)
                .GroupBy(producto => new
                {
                    producto.IdProducto,
                    producto.Nombre,
                    producto.TipoObjeto,
                    producto.CodigoInterno
                })
                .Select(grupo => new
                {
                    grupo.Key.IdProducto,
                    grupo.Key.Nombre,
                    grupo.Key.TipoObjeto,
                    grupo.Key.CodigoInterno,
                    TotalSolicitudes = grupo.Count()
                })
                .OrderByDescending(item => item.TotalSolicitudes)
                .ThenBy(item => item.Nombre)
                .Take(10)
                .ToListAsync();

            var masSolicitados = masSolicitadosRaw
                .Select(item => new RecursoSolicitadoReporte(
                    item.IdProducto,
                    item.Nombre,
                    item.TipoObjeto,
                    item.CodigoInterno,
                    item.TotalSolicitudes))
                .ToList();

            var lectoresRaw = await _context.Prestamos
                .AsNoTracking()
                .Where(prestamo => prestamo.FechaPrestamo >= fechaInicio && prestamo.FechaPrestamo < fechaFin)
                .Join(
                    _context.Inventario.AsNoTracking().Where(producto => producto.TipoObjeto == "libro_fisico"),
                    prestamo => prestamo.IdProducto,
                    producto => producto.IdProducto,
                    (prestamo, producto) => prestamo)
                .Join(
                    _context.Usuarios.AsNoTracking().Where(usuario => usuario.Rol == "estudiante"),
                    prestamo => prestamo.IdUsuario,
                    usuario => usuario.IdUsuario,
                    (prestamo, usuario) => usuario)
                .GroupBy(usuario => new
                {
                    usuario.IdUsuario,
                    usuario.Nombres,
                    usuario.Apellidos,
                    usuario.Correo,
                    usuario.Seccion
                })
                .Select(grupo => new
                {
                    grupo.Key.IdUsuario,
                    grupo.Key.Nombres,
                    grupo.Key.Apellidos,
                    grupo.Key.Correo,
                    grupo.Key.Seccion,
                    TotalLecturas = grupo.Count()
                })
                .OrderByDescending(item => item.TotalLecturas)
                .ThenBy(item => item.Apellidos)
                .ThenBy(item => item.Nombres)
                .Take(5)
                .ToListAsync();

            var lectores = lectoresRaw
                .Select(item => new LectorMesReporte(
                    item.IdUsuario,
                    $"{item.Nombres} {item.Apellidos}".Trim(),
                    item.Correo,
                    item.Seccion,
                    item.TotalLecturas))
                .ToList();

            return Ok(new ReporteMensualResponse(
                anio,
                mes,
                fechaInicio,
                fechaFin.AddDays(-1),
                masSolicitados,
                lectores.FirstOrDefault(),
                lectores));
        }
    }

    public record ReporteMensualResponse(
        int Anio,
        int Mes,
        DateTime FechaInicio,
        DateTime FechaFin,
        IEnumerable<RecursoSolicitadoReporte> MasSolicitados,
        LectorMesReporte? LectorDelMes,
        IEnumerable<LectorMesReporte> TopLectores);

    public record RecursoSolicitadoReporte(
        int IdProducto,
        string Nombre,
        string TipoObjeto,
        string? CodigoInterno,
        int TotalSolicitudes);

    public record LectorMesReporte(
        int IdUsuario,
        string NombreCompleto,
        string Correo,
        string? Seccion,
        int TotalLecturas);
}
