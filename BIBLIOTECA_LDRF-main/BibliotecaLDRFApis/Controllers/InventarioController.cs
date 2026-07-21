using Biblioteca.Dominio.EntidadesTipadas;
using Biblioteca.Dominio.InterfaceLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BibliotecaLDRFApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventarioController : ControllerBase
    {
        private readonly IInventarioLN _inventarioLN;
        private readonly ILogger<InventarioController> _logger;

        public InventarioController(IInventarioLN inventarioLN, ILogger<InventarioController> logger)
        {
            _inventarioLN = inventarioLN;
            _logger = logger;
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
            try
            {
                await _inventarioLN.CrearInventarioAsync(inventario);
                return CreatedAtAction(nameof(GetById), new { id = inventario.IdProducto }, inventario);
            }
            catch (InvalidOperationException error)
            {
                return BadRequest(new { message = error.Message });
            }
            catch (DbUpdateException error)
            {
                return ManejarErrorBaseDatos(error, "No se pudo guardar el recurso en inventario.");
            }
            catch (Exception error)
            {
                _logger.LogError(error, "No se pudo guardar el recurso en inventario.");
                return StatusCode(500, new { message = $"No se pudo guardar el recurso en inventario. Detalle: {error.GetBaseException().Message}" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "administracion,maestro")]
        public async Task<ActionResult> Update(int id, [FromBody] TInventario inventario)
        {
            if (id != inventario.IdProducto) return BadRequest();

            try
            {
                await _inventarioLN.ActualizarInventarioAsync(inventario);
                return NoContent();
            }
            catch (InvalidOperationException error)
            {
                return BadRequest(new { message = error.Message });
            }
            catch (DbUpdateException error)
            {
                return ManejarErrorBaseDatos(error, "No se pudo actualizar el recurso en inventario.");
            }
            catch (Exception error)
            {
                _logger.LogError(error, "No se pudo actualizar el recurso en inventario.");
                return StatusCode(500, new { message = $"No se pudo actualizar el recurso en inventario. Detalle: {error.GetBaseException().Message}" });
            }
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

        private ActionResult ManejarErrorBaseDatos(DbUpdateException error, string mensaje)
        {
            var baseError = error.GetBaseException();
            _logger.LogError(error, "{Mensaje}", mensaje);

            if (baseError is PostgresException postgres)
            {
                return postgres.SqlState switch
                {
                    PostgresErrorCodes.UniqueViolation => Conflict(new
                    {
                        message = "Ya existe un recurso con ese codigo interno. Cambia el codigo o edita el recurso existente."
                    }),
                    PostgresErrorCodes.StringDataRightTruncation => BadRequest(new
                    {
                        message = "Una URL o texto del recurso es mas largo que la columna de Neon. Ejecuta el ALTER TABLE para ampliar portada y archivo_url."
                    }),
                    PostgresErrorCodes.CheckViolation => BadRequest(new
                    {
                        message = $"El recurso no cumple una regla de Neon ({postgres.ConstraintName})."
                    }),
                    _ => StatusCode(500, new
                    {
                        message = $"{mensaje} Detalle Neon: {postgres.MessageText}"
                    })
                };
            }

            return StatusCode(500, new { message = $"{mensaje} Detalle: {baseError.Message}" });
        }
    }
}
