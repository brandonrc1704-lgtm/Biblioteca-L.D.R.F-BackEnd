using BibliotecaLDRFApis.Services;
using Amazon.S3;
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
        private readonly IR2StorageService _storageService;

        public ArchivoDigitalController(IR2StorageService storageService)
        {
            _storageService = storageService;
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

            string archivoUrl;
            var nombreSeguro = CrearNombreSeguro(archivo.FileName);

            try
            {
                archivoUrl = await _storageService.UploadAsync(archivo, "libros-digitales", nombreSeguro, HttpContext.RequestAborted);
            }
            catch (AmazonS3Exception error)
            {
                return StatusCode(502, $"Cloudflare R2 rechazo el PDF: {error.Message}");
            }
            catch (InvalidOperationException error)
            {
                return StatusCode(500, error.Message);
            }

            return Ok(new
            {
                archivoUrl,
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
