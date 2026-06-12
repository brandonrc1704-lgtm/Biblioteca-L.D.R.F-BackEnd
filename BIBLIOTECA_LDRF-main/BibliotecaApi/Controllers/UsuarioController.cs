using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioLN _usuarioLN;

        public UsuarioController(IUsuarioLN usuarioLN)
        {
            _usuarioLN = usuarioLN;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TUsuario>> GetById(int id)
        {
            var usuario = await _usuarioLN.ObtenerPorIdAsync(id);
            return usuario is null ? NotFound() : Ok(usuario);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TUsuario>>> GetAll()
        {
            return Ok(await _usuarioLN.ObtenerTodosAsync());
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] TUsuario usuario)
        {
            await _usuarioLN.CrearUsuarioAsync(usuario);
            return CreatedAtAction(nameof(GetById), new { id = usuario.IdUsuario }, usuario);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] TUsuario usuario)
        {
            if (id != usuario.IdUsuario) return BadRequest();
            await _usuarioLN.ActualizarUsuarioAsync(usuario);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _usuarioLN.EliminarUsuarioAsync(id);
            return NoContent();
        }
    }
}
