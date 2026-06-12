using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SancionController : ControllerBase
    {
        private readonly ISancionLN _sancionLN;

        public SancionController(ISancionLN sancionLN)
        {
            _sancionLN = sancionLN;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TSancion>> GetById(int id)
        {
            var sancion = await _sancionLN.ObtenerPorIdAsync(id);
            return sancion is null ? NotFound() : Ok(sancion);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TSancion>>> GetAll()
        {
            return Ok(await _sancionLN.ObtenerTodosAsync());
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] TSancion sancion)
        {
            await _sancionLN.CrearSancionAsync(sancion);
            return CreatedAtAction(nameof(GetById), new { id = sancion.IdSancion }, sancion);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] TSancion sancion)
        {
            if (id != sancion.IdSancion) return BadRequest();
            await _sancionLN.ActualizarSancionAsync(sancion);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _sancionLN.EliminarSancionAsync(id);
            return NoContent();
        }
    }
}
