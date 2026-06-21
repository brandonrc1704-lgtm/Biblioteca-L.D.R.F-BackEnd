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
            if (inventario is null || !PuedeVer(inventario))
            {
                return NotFound();
            }

            return Ok(inventario);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TInventario>>> GetAll()
        {
            var inventario = await _inventarioLN.ObtenerTodosAsync();
            return Ok(inventario.Where(PuedeVer));
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

        private bool PuedeVer(TInventario inventario)
        {
            if (User.IsInRole("administracion") || User.IsInRole("maestro"))
            {
                return true;
            }

            if (inventario.TipoObjeto != "equipo_institucion")
            {
                return true;
            }

            var visibilidad = (inventario.Visibilidad ?? "todos").Trim().ToLowerInvariant();
            return visibilidad is "todos" or "estudiantes";
        }
    }
}
