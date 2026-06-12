using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaLDRFApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioLN _usuarioLN;

        public UsuarioController(IUsuarioLN usuarioLN)
        {
            _usuarioLN = usuarioLN;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioPublico>> GetById(int id)
        {
            var usuario = await _usuarioLN.ObtenerPorIdAsync(id);
            return usuario is null ? NotFound() : Ok(ToPublico(usuario));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioPublico>>> GetAll()
        {
            var usuarios = await _usuarioLN.ObtenerTodosAsync();
            return Ok(usuarios.Select(ToPublico));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Create([FromBody] TUsuario usuario)
        {
            try
            {
                if (User.Identity?.IsAuthenticated != true)
                {
                    usuario.Rol = "estudiante";
                    usuario.Estado = "activo";
                    usuario.TokenActivo = null;
                }

                await _usuarioLN.CrearUsuarioAsync(usuario);
                return CreatedAtAction(nameof(GetById), new { id = usuario.IdUsuario }, ToPublico(usuario));
            }
            catch (InvalidOperationException error)
            {
                return BadRequest(new { message = error.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "administracion")]
        public async Task<ActionResult> Update(int id, [FromBody] TUsuario usuario)
        {
            if (id != usuario.IdUsuario) return BadRequest();

            try
            {
                await _usuarioLN.ActualizarUsuarioAsync(usuario);
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
            try
            {
                await _usuarioLN.EliminarUsuarioAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException error)
            {
                return BadRequest(new { message = error.Message });
            }
        }

        private static UsuarioPublico ToPublico(TUsuario usuario) => new(
            usuario.IdUsuario,
            usuario.Credencial,
            usuario.Nombres,
            usuario.Apellidos,
            usuario.Cedula,
            usuario.Correo,
            usuario.Telefono,
            usuario.FechaNacimiento,
            usuario.Seccion,
            usuario.Rol,
            usuario.Estado,
            usuario.CreadoEn,
            usuario.ActualizadoEn);
    }
}
