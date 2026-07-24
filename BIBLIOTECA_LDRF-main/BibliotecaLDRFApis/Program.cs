using Biblioteca.Dominio.InterfaceLN;
using Biblioteca.Dominio.InterfacesAD;
using BibliotecaLDRFApis.Services;
using Biblioteca.LogicaNegocio;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.FileProviders;
using Npgsql;
using System.Text;
using TiendaBatarazo.AccesoDatos.Context;
using TiendaBatarazo.AccesoDatos.Implementaciones;

var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 200 * 1024 * 1024;
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se configuro la cadena de conexion DefaultConnection.");
var npgsqlConnection = new NpgsqlConnectionStringBuilder(connectionString)
{
    GssEncryptionMode = GssEncryptionMode.Disable
};

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(npgsqlConnection.ConnectionString));
builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<IUnidadTrabajoEF, UnidadTrabajoEF>();
builder.Services.AddScoped<IUsuarioLN, UsuarioLN>();
builder.Services.AddScoped<IInventarioLN, InventarioLN>();
builder.Services.AddScoped<IDescargaLN, DescargaLN>();
builder.Services.AddScoped<IPrestamoLN, PrestamoLN>();
builder.Services.AddScoped<ISancionLN, SancionLN>();
builder.Services.AddScoped<IHorarioSeccionLN, HorarioSeccionLN>();
builder.Services.AddScoped<IRegistroBibliotecaLN, RegistroBibliotecaLN>();
builder.Services.AddScoped<IRegistroSeccionBibliotecaLN, RegistroSeccionBibliotecaLN>();
builder.Services.AddScoped<INoticiaLN, NoticiaLN>();
builder.Services.AddScoped<ICorreoService, SmtpCorreoService>();
builder.Services.AddScoped<IR2StorageService, R2StorageService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendOnly", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:4200", "http://localhost:8100", "https://localhost:4200"];

        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-only-change-this-secret-key-before-production";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "BibliotecaLDRF";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "BibliotecaLDRF";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors("FrontendOnly");

var webRootPath = app.Environment.WebRootPath;
if (string.IsNullOrWhiteSpace(webRootPath))
{
    webRootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
    app.Environment.WebRootPath = webRootPath;
}

Directory.CreateDirectory(webRootPath);
Directory.CreateDirectory(Path.Combine(webRootPath, "uploads", "portadas"));
Directory.CreateDirectory(Path.Combine(webRootPath, "uploads", "libros-digitales"));

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(webRootPath),
    RequestPath = ""
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
