using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaLDRFApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DescargaController : ControllerBase
    {
        private readonly IDescargaLN _descargaLN;

        public DescargaController(IDescargaLN descargaLN)
        {
            _descargaLN = descargaLN;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TDescarga>> GetById(int id)
        {
            var descarga = await _descargaLN.ObtenerPorIdAsync(id);
            return descarga is null ? NotFound() : Ok(descarga);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TDescarga>>> GetAll()
        {
            return Ok(await _descargaLN.ObtenerTodosAsync());
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] TDescarga descarga)
        {
            await _descargaLN.CrearDescargaAsync(descarga);
            return CreatedAtAction(nameof(GetById), new { id = descarga.IdDescarga }, descarga);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "administracion,maestro")]
        public async Task<ActionResult> Delete(int id)
        {
            await _descargaLN.EliminarDescargaAsync(id);
            return NoContent();
        }
    }
}
