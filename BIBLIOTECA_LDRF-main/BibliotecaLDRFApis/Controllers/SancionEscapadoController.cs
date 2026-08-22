using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaLDRFApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SancionEscapadoController : ControllerBase
    {
        private readonly ISancionEscapadoLN _sancionEscapadoLN;

        public SancionEscapadoController(ISancionEscapadoLN sancionEscapadoLN)
        {
            _sancionEscapadoLN = sancionEscapadoLN;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TSancionEscapado>> GetById(int id)
        {
            var registro = await _sancionEscapadoLN.ObtenerPorIdAsync(id);
            return registro is null ? NotFound() : Ok(registro);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TSancionEscapado>>> GetAll([FromQuery] DateOnly? fecha)
        {
            if (fecha.HasValue)
            {
                return Ok(await _sancionEscapadoLN.ObtenerPorFechaAsync(fecha.Value));
            }

            return Ok(await _sancionEscapadoLN.ObtenerTodosAsync());
        }

        [HttpPost]
        [Authorize(Roles = "administracion,maestro")]
        public async Task<ActionResult> Create([FromBody] TSancionEscapado registro)
        {
            await _sancionEscapadoLN.CrearSancionEscapadoAsync(registro);
            return CreatedAtAction(nameof(GetById), new { id = registro.IdEscapado }, registro);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "administracion,maestro")]
        public async Task<ActionResult> Update(int id, [FromBody] TSancionEscapado registro)
        {
            if (id != registro.IdEscapado) return BadRequest();

            await _sancionEscapadoLN.ActualizarSancionEscapadoAsync(registro);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "administracion,maestro")]
        public async Task<ActionResult> Delete(int id)
        {
            await _sancionEscapadoLN.EliminarSancionEscapadoAsync(id);
            return NoContent();
        }
    }
}
