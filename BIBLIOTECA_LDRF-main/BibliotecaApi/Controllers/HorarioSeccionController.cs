using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HorarioSeccionController : ControllerBase
    {
        private readonly IHorarioSeccionLN _horarioSeccionLN;

        public HorarioSeccionController(IHorarioSeccionLN horarioSeccionLN)
        {
            _horarioSeccionLN = horarioSeccionLN;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<THorarioSeccion>> GetById(int id)
        {
            var horario = await _horarioSeccionLN.ObtenerPorIdAsync(id);
            return horario is null ? NotFound() : Ok(horario);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<THorarioSeccion>>> GetAll()
        {
            return Ok(await _horarioSeccionLN.ObtenerTodosAsync());
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] THorarioSeccion horario)
        {
            await _horarioSeccionLN.CrearHorarioSeccionAsync(horario);
            return CreatedAtAction(nameof(GetById), new { id = horario.IdHorario }, horario);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] THorarioSeccion horario)
        {
            if (id != horario.IdHorario) return BadRequest();
            await _horarioSeccionLN.ActualizarHorarioSeccionAsync(horario);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _horarioSeccionLN.EliminarHorarioSeccionAsync(id);
            return NoContent();
        }
    }
}
