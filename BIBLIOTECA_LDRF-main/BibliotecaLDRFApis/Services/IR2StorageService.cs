namespace BibliotecaLDRFApis.Services;

public interface IR2StorageService
{
    Task<string> UploadAsync(IFormFile archivo, string carpeta, string nombreArchivo, CancellationToken cancellationToken = default);
}
