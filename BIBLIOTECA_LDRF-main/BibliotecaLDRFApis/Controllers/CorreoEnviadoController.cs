using Biblioteca.Dominio.EntidadesTipadas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaBatarazo.AccesoDatos.Context;

namespace BibliotecaLDRFApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administracion")]
    public class CorreoEnviadoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CorreoEnviadoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TCorreoEnviado>>> GetAll()
        {
            var correos = await _context.CorreosEnviados
                .AsNoTracking()
                .OrderByDescending(correo => correo.CreadoEn)
                .Take(200)
                .Select(correo => new TCorreoEnviado
                {
                    IdCorreo = correo.IdCorreo,
                    IdUsuario = correo.IdUsuario,
                    IdSancion = correo.IdSancion,
                    CorreoDestino = correo.CorreoDestino,
                    Asunto = correo.Asunto,
                    Mensaje = correo.Mensaje,
                    Estado = correo.Estado,
                    ErrorEnvio = correo.ErrorEnvio,
                    EnviadoEn = correo.EnviadoEn,
                    CreadoEn = correo.CreadoEn
                })
                .ToListAsync();

            return Ok(correos);
        }
    }
}
