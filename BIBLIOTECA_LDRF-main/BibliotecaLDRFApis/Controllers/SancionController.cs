using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaLDRFApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administracion,maestro")]
    public class SancionController : ControllerBase
    {
        private readonly ISancionLN _sancionLN;
        private readonly ILogger<SancionController> _logger;

        public SancionController(ISancionLN sancionLN, ILogger<SancionController> logger)
        {
            _sancionLN = sancionLN;
            _logger = logger;
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
            try
            {
                await _sancionLN.CrearSancionAsync(sancion);
                return CreatedAtAction(nameof(GetById), new { id = sancion.IdSancion }, sancion);
            }
            catch (InvalidOperationException error)
            {
                return BadRequest(new { message = error.Message });
            }
            catch (Exception error)
            {
                _logger.LogError(error, "No se pudo registrar la sancion.");
                return StatusCode(500, new { message = $"No se pudo registrar la sancion. Detalle: {error.GetBaseException().Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] TSancion sancion)
        {
            if (id != sancion.IdSancion) return BadRequest();

            try
            {
                await _sancionLN.ActualizarSancionAsync(sancion);
                return NoContent();
            }
            catch (InvalidOperationException error)
            {
                return BadRequest(new { message = error.Message });
            }
            catch (Exception error)
            {
                _logger.LogError(error, "No se pudo actualizar la sancion.");
                return StatusCode(500, new { message = $"No se pudo actualizar la sancion. Detalle: {error.GetBaseException().Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _sancionLN.EliminarSancionAsync(id);
            return NoContent();
        }
    }
}
