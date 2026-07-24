using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BibliotecaLDRFApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administracion,maestro")]
    public class RegistroSeccionBibliotecaController : ControllerBase
    {
        private readonly IRegistroSeccionBibliotecaLN _registroSeccionBibliotecaLN;
        private readonly ILogger<RegistroSeccionBibliotecaController> _logger;

        public RegistroSeccionBibliotecaController(
            IRegistroSeccionBibliotecaLN registroSeccionBibliotecaLN,
            ILogger<RegistroSeccionBibliotecaController> logger)
        {
            _registroSeccionBibliotecaLN = registroSeccionBibliotecaLN;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TRegistroSeccionBiblioteca>> GetById(int id)
        {
            var registro = await _registroSeccionBibliotecaLN.ObtenerPorIdAsync(id);
            return registro is null ? NotFound() : Ok(registro);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TRegistroSeccionBiblioteca>>> GetAll([FromQuery] DateOnly? fecha)
        {
            var registros = fecha.HasValue
                ? await _registroSeccionBibliotecaLN.ObtenerPorFechaAsync(fecha.Value)
                : await _registroSeccionBibliotecaLN.ObtenerTodosAsync();

            return Ok(registros);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] TRegistroSeccionBiblioteca registro)
        {
            try
            {
                registro.RegistradoPor ??= ObtenerIdUsuarioActual();
                await _registroSeccionBibliotecaLN.CrearRegistroAsync(registro);
                return CreatedAtAction(nameof(GetById), new { id = registro.IdRegistroSeccion }, registro);
            }
            catch (InvalidOperationException error)
            {
                return BadRequest(new { message = error.Message });
            }
            catch (Exception error)
            {
                _logger.LogError(error, "No se pudo registrar el uso de biblioteca por seccion.");
                return StatusCode(500, new
                {
                    message = $"No se pudo registrar el uso de biblioteca por seccion. Detalle: {error.GetBaseException().Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "administracion")]
        public async Task<ActionResult> Delete(int id)
        {
            await _registroSeccionBibliotecaLN.EliminarRegistroAsync(id);
            return NoContent();
        }

        private int? ObtenerIdUsuarioActual()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}
