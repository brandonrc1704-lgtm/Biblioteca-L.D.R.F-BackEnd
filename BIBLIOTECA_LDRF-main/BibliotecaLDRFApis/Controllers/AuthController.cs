using Biblioteca.Dominio.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TiendaBatarazo.AccesoDatos.Context;

namespace BibliotecaLDRFApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const string HashPrefix = "sha256$";
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var correo = request.Correo.Trim().ToLowerInvariant();
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Correo.ToLower() == correo);

            if (usuario is null ||
                usuario.Estado != "activo" ||
                !ClaveCoincide(request.Clave, usuario.Clave))
            {
                return Unauthorized(new { message = "Credenciales invalidas." });
            }

            var expiresAt = DateTime.UtcNow.AddHours(8);
            var token = CrearToken(usuario, expiresAt);

            return Ok(new LoginResponse(token, expiresAt, UsuarioPublico.Desde(usuario)));
        }

        private string CrearToken(Usuario usuario, DateTime expiresAt)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "dev-only-change-this-secret-key-before-production";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "BibliotecaLDRF";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "BibliotecaLDRF";
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("rol", usuario.Rol)
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static bool ClaveCoincide(string claveIngresada, string claveGuardada)
        {
            var guardada = claveGuardada.Trim();
            if (string.IsNullOrWhiteSpace(guardada))
            {
                return false;
            }

            if (!guardada.StartsWith(HashPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return guardada == claveIngresada;
            }

            return guardada == ProtegerClave(claveIngresada);
        }

        private static string ProtegerClave(string clave)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(clave.Trim()));
            return $"{HashPrefix}{Convert.ToHexString(bytes).ToLowerInvariant()}";
        }
    }

    public record LoginRequest(string Correo, string Clave);

    public record LoginResponse(string Token, DateTime ExpiresAt, UsuarioPublico Usuario);

    public record UsuarioPublico(
        int IdUsuario,
        string Credencial,
        string Nombres,
        string Apellidos,
        string Cedula,
        string Correo,
        string? Telefono,
        DateOnly? FechaNacimiento,
        string? Seccion,
        string Rol,
        string Estado,
        DateTime CreadoEn,
        DateTime ActualizadoEn)
    {
        public static UsuarioPublico Desde(Usuario usuario) => new(
            usuario.IdUsuario,
            usuario.Credencial,
            usuario.Nombres,
            usuario.Apellidos,
            usuario.Cedula,
            usuario.Correo,
            usuario.Telefono,
            usuario.FechaNacimiento,
            usuario.Seccion,
            usuario.Rol,
            usuario.Estado,
            usuario.CreadoEn,
            usuario.ActualizadoEn);
    }
}
