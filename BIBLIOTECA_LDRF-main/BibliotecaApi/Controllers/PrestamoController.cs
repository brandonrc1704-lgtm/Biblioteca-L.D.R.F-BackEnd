using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            await _prestamoLN.CrearPrestamoAsync(prestamo);
            return CreatedAtAction(nameof(GetById), new { id = prestamo.IdPrestamo }, prestamo);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] TPrestamo prestamo)
        {
            if (id != prestamo.IdPrestamo) return BadRequest();
            await _prestamoLN.ActualizarPrestamoAsync(prestamo);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _prestamoLN.EliminarPrestamoAsync(id);
            return NoContent();
        }
    }
}
