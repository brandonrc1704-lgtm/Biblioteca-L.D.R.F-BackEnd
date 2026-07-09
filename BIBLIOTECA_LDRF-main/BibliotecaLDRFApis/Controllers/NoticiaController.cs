using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BibliotecaLDRFApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NoticiaController : ControllerBase
    {
        private readonly INoticiaLN _noticiaLN;

        public NoticiaController(INoticiaLN noticiaLN)
        {
            _noticiaLN = noticiaLN;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TNoticia>> GetById(int id)
        {
            var noticia = await _noticiaLN.ObtenerPorIdAsync(id);
            if (noticia is null)
            {
                return NotFound();
            }

            if (!EsAdministracion() && noticia.Estado != "publicada")
            {
                return NotFound();
            }

            return Ok(noticia);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TNoticia>>> GetAll()
        {
            return Ok(await _noticiaLN.ObtenerTodasAsync(!EsAdministracion()));
        }

        [HttpPost]
        [Authorize(Roles = "administracion")]
        public async Task<ActionResult> Create([FromBody] TNoticia noticia)
        {
            try
            {
                noticia.CreadoPor = ObtenerIdUsuarioActual() ?? noticia.CreadoPor;
                await _noticiaLN.CrearNoticiaAsync(noticia);
                return CreatedAtAction(nameof(GetById), new { id = noticia.IdNoticia }, noticia);
            }
            catch (InvalidOperationException error)
            {
                return BadRequest(new { message = error.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "administracion")]
        public async Task<ActionResult> Update(int id, [FromBody] TNoticia noticia)
        {
            if (id != noticia.IdNoticia) return BadRequest();

            try
            {
                await _noticiaLN.ActualizarNoticiaAsync(noticia);
                return NoContent();
            }
            catch (InvalidOperationException error)
            {
                return BadRequest(new { message = error.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "administracion")]
        public async Task<ActionResult> Delete(int id)
        {
            await _noticiaLN.EliminarNoticiaAsync(id);
            return NoContent();
        }

        private bool EsAdministracion()
        {
            return User.IsInRole("administracion");
        }

        private int? ObtenerIdUsuarioActual()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}
