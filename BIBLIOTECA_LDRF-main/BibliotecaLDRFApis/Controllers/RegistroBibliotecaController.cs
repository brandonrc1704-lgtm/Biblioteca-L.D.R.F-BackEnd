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
    public class RegistroBibliotecaController : ControllerBase
    {
        private readonly IRegistroBibliotecaLN _registroBibliotecaLN;
        private readonly ILogger<RegistroBibliotecaController> _logger;

        public RegistroBibliotecaController(
            IRegistroBibliotecaLN registroBibliotecaLN,
            ILogger<RegistroBibliotecaController> logger)
        {
            _registroBibliotecaLN = registroBibliotecaLN;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TRegistroBiblioteca>> GetById(int id)
        {
            var registro = await _registroBibliotecaLN.ObtenerPorIdAsync(id);
            return registro is null ? NotFound() : Ok(registro);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TRegistroBiblioteca>>> GetAll([FromQuery] DateOnly? fecha)
        {
            var registros = fecha.HasValue
                ? await _registroBibliotecaLN.ObtenerPorFechaAsync(fecha.Value)
                : await _registroBibliotecaLN.ObtenerTodosAsync();

            return Ok(registros);
        }

        [HttpGet("buscar-estudiantes")]
        public async Task<ActionResult<IEnumerable<TUsuario>>> BuscarEstudiantes([FromQuery] string q)
        {
            return Ok(await _registroBibliotecaLN.BuscarEstudiantesAsync(q));
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] TRegistroBiblioteca registro)
        {
            try
            {
                registro.RegistradoPor ??= ObtenerIdUsuarioActual();
                await _registroBibliotecaLN.CrearRegistroAsync(registro);
                return CreatedAtAction(nameof(GetById), new { id = registro.IdRegistro }, registro);
            }
            catch (InvalidOperationException error)
            {
                return BadRequest(new { message = error.Message });
            }
            catch (Exception error)
            {
                _logger.LogError(error, "No se pudo registrar el movimiento en la biblioteca.");
                return StatusCode(500, new
                {
                    message = $"No se pudo registrar el movimiento en la biblioteca. Detalle: {error.GetBaseException().Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "administracion")]
        public async Task<ActionResult> Delete(int id)
        {
            await _registroBibliotecaLN.EliminarRegistroAsync(id);
            return NoContent();
        }

        private int? ObtenerIdUsuarioActual()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}
