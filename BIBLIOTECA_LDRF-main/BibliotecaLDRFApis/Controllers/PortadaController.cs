using BibliotecaLDRFApis.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;

namespace BibliotecaLDRFApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administracion,maestro")]
    public class PortadaController : ControllerBase
    {
        private const long MaxImagenBytes = 10 * 1024 * 1024;
        private static readonly HashSet<string> ExtensionesPermitidas = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
            ".gif"
        };

        private readonly IR2StorageService _storageService;

        public PortadaController(IR2StorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpPost("subir")]
        [RequestSizeLimit(MaxImagenBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxImagenBytes)]
        public async Task<IActionResult> Subir([FromForm] IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest("Archivo vacio.");
            }

            if (archivo.Length > MaxImagenBytes)
            {
                return BadRequest("La imagen supera el maximo permitido de 10 MB.");
            }

            if (!EsImagenPermitida(archivo))
            {
                return BadRequest("Solo se permiten imagenes JPG, PNG, WEBP o GIF.");
            }

            var nombreSeguro = CrearNombreSeguro(archivo.FileName);
            var portadaUrl = await _storageService.UploadAsync(archivo, "portadas", nombreSeguro, HttpContext.RequestAborted);

            return Ok(new
            {
                portadaUrl,
                nombre = nombreSeguro,
                tamanoKb = Math.Round(archivo.Length / 1024d, 2)
            });
        }

        private static bool EsImagenPermitida(IFormFile archivo)
        {
            var extension = Path.GetExtension(archivo.FileName);
            return archivo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                && ExtensionesPermitidas.Contains(extension);
        }

        private static string CrearNombreSeguro(string nombreOriginal)
        {
            var extension = Path.GetExtension(nombreOriginal).ToLowerInvariant();
            var nombreSinExtension = Path.GetFileNameWithoutExtension(nombreOriginal);
            var normalizado = nombreSinExtension
                .Normalize(NormalizationForm.FormD)
                .ToLowerInvariant();

            var builder = new StringBuilder();
            foreach (var caracter in normalizado)
            {
                var categoria = CharUnicodeInfo.GetUnicodeCategory(caracter);
                if (categoria == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                builder.Append(char.IsLetterOrDigit(caracter) ? caracter : '-');
            }

            var nombreBase = string.Join('-', builder
                .ToString()
                .Split('-', StringSplitOptions.RemoveEmptyEntries));

            if (string.IsNullOrWhiteSpace(nombreBase))
            {
                nombreBase = "portada";
            }

            return $"{DateTime.Now:yyyyMMddHHmmss}-{nombreBase}{extension}";
        }
    }
}
