using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaLDRFApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventarioController : ControllerBase
    {
        private readonly IInventarioLN _inventarioLN;

        public InventarioController(IInventarioLN inventarioLN)
        {
            _inventarioLN = inventarioLN;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TInventario>> GetById(int id)
        {
            var inventario = await _inventarioLN.ObtenerPorIdAsync(id);
            return inventario is null ? NotFound() : Ok(inventario);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TInventario>>> GetAll()
        {
            return Ok(await _inventarioLN.ObtenerTodosAsync());
        }

        [HttpPost]
        [Authorize(Roles = "administracion,maestro")]
        public async Task<ActionResult> Create([FromBody] TInventario inventario)
        {
            await _inventarioLN.CrearInventarioAsync(inventario);
            return CreatedAtAction(nameof(GetById), new { id = inventario.IdProducto }, inventario);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "administracion,maestro")]
        public async Task<ActionResult> Update(int id, [FromBody] TInventario inventario)
        {
            if (id != inventario.IdProducto) return BadRequest();

            await _inventarioLN.ActualizarInventarioAsync(inventario);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "administracion,maestro")]
        public async Task<ActionResult> Delete(int id)
        {
            await _inventarioLN.EliminarInventarioAsync(id);
            return NoContent();
        }
    }
}
