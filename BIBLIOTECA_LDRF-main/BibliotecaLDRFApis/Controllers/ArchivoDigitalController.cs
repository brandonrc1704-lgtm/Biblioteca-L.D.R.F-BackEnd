using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;

namespace BibliotecaLDRFApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administracion,maestro")]
    public class ArchivoDigitalController : ControllerBase
    {
        private const long MaxPdfBytes = 200 * 1024 * 1024;
        private readonly IWebHostEnvironment _environment;

        public ArchivoDigitalController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost("subir")]
        [RequestSizeLimit(MaxPdfBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxPdfBytes)]
        public async Task<IActionResult> Subir([FromForm] IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest("Archivo vacio.");
            }

            if (archivo.Length > MaxPdfBytes)
            {
                return BadRequest("El PDF supera el maximo permitido de 200 MB.");
            }

            if (!EsPdf(archivo))
            {
                return BadRequest("Solo se permiten archivos PDF.");
            }

            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            var carpeta = Path.Combine(webRoot, "uploads", "libros-digitales");
            Directory.CreateDirectory(carpeta);

            var nombreSeguro = CrearNombreSeguro(archivo.FileName);
            var rutaFisica = Path.Combine(carpeta, nombreSeguro);

            await using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return Ok(new
            {
                archivoUrl = $"/uploads/libros-digitales/{nombreSeguro}",
                nombre = nombreSeguro,
                tamanoMb = Math.Round(archivo.Length / 1024d / 1024d, 2)
            });
        }

        private static bool EsPdf(IFormFile archivo)
        {
            return archivo.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
                || Path.GetExtension(archivo.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
        }

        private static string CrearNombreSeguro(string nombreOriginal)
        {
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
                nombreBase = "libro-digital";
            }

            return $"{DateTime.Now:yyyyMMddHHmmss}-{nombreBase}.pdf";
        }
    }
}
