using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace BibliotecaLDRFApis.Services;

public sealed class R2StorageService : IR2StorageService
{
    private readonly R2StorageOptions _options;
    private readonly IAmazonS3 _s3Client;

    public R2StorageService(IConfiguration configuration)
    {
        _options = configuration.GetSection("R2").Get<R2StorageOptions>() ?? new R2StorageOptions();
        ValidarConfiguracion(_options);

        var config = new AmazonS3Config
        {
            ServiceURL = _options.ServiceUrl,
            ForcePathStyle = true,
            AuthenticationRegion = "auto"
        };

        _s3Client = new AmazonS3Client(
            new BasicAWSCredentials(_options.AccessKeyId, _options.SecretAccessKey),
            config
        );
    }

    public async Task<string> UploadAsync(
        IFormFile archivo,
        string carpeta,
        string nombreArchivo,
        CancellationToken cancellationToken = default)
    {
        var key = CrearKey(carpeta, nombreArchivo);

        await using var stream = archivo.OpenReadStream();
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = stream,
            ContentType = archivo.ContentType,
            AutoCloseStream = false,
            DisablePayloadSigning = true
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);

        return CrearUrlPublica(key);
    }

    private static string CrearKey(string carpeta, string nombreArchivo)
    {
        var carpetaNormalizada = carpeta.Trim().Trim('/').Replace("\\", "/");
        var nombreNormalizado = Path.GetFileName(nombreArchivo);

        return string.IsNullOrWhiteSpace(carpetaNormalizada)
            ? nombreNormalizado
            : $"{carpetaNormalizada}/{nombreNormalizado}";
    }

    private string CrearUrlPublica(string key)
    {
        var baseUrl = _options.PublicBaseUrl.TrimEnd('/');
        var keyCodificada = string.Join("/", key.Split('/').Select(Uri.EscapeDataString));

        return $"{baseUrl}/{keyCodificada}";
    }

    private static void ValidarConfiguracion(R2StorageOptions options)
    {
        var faltantes = new List<string>();

        if (string.IsNullOrWhiteSpace(options.AccountId)) faltantes.Add("R2__AccountId");
        if (string.IsNullOrWhiteSpace(options.AccessKeyId)) faltantes.Add("R2__AccessKeyId");
        if (string.IsNullOrWhiteSpace(options.SecretAccessKey)) faltantes.Add("R2__SecretAccessKey");
        if (string.IsNullOrWhiteSpace(options.BucketName)) faltantes.Add("R2__BucketName");
        if (string.IsNullOrWhiteSpace(options.PublicBaseUrl)) faltantes.Add("R2__PublicBaseUrl");

        if (faltantes.Count > 0)
        {
            throw new InvalidOperationException($"Faltan variables de entorno para Cloudflare R2: {string.Join(", ", faltantes)}.");
        }
    }
}
