using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaLDRFApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrestamoController : ControllerBase
    {
        private readonly IPrestamoLN _prestamoLN;

        public PrestamoController(IPrestamoLN prestamoLN)
        {
            _prestamoLN = prestamoLN;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TPrestamo>> GetById(int id)
        {
            var prestamo = await _prestamoLN.ObtenerPorIdAsync(id);
            return prestamo is null ? NotFound() : Ok(prestamo);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TPrestamo>>> GetAll()
        {
            return Ok(await _prestamoLN.ObtenerTodosAsync());
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] TPrestamo prestamo)
        {
            try
            {
                await _prestamoLN.CrearPrestamoAsync(prestamo);
                return CreatedAtAction(nameof(GetById), new { id = prestamo.IdPrestamo }, prestamo);
            }
            catch (InvalidOperationException error)
            {
                return BadRequest(new { message = error.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "administracion,maestro")]
        public async Task<ActionResult> Update(int id, [FromBody] TPrestamo prestamo)
        {
            if (id != prestamo.IdPrestamo) return BadRequest();

            try
            {
                await _prestamoLN.ActualizarPrestamoAsync(prestamo);
                return NoContent();
            }
            catch (InvalidOperationException error)
            {
                return BadRequest(new { message = error.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "administracion,maestro")]
        public async Task<ActionResult> Delete(int id)
        {
            await _prestamoLN.EliminarPrestamoAsync(id);
            return NoContent();
        }
    }
}
