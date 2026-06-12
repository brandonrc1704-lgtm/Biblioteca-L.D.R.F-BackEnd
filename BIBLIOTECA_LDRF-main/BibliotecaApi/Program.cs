using Microsoft.EntityFrameworkCore;
using NLog.Web;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Biblioteca.Dominio.InterfaceLN;
using Biblioteca.Dominio.InterfacesAD;
using Biblioteca.LogicaNegocio;
using TiendaBatarazo.AccesoDatos.Context;
using TiendaBatarazo.AccesoDatos.Implementaciones;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ============================
        // LOGGING CON NLOG
        // ============================

        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Host.UseNLog();

        // ============================
        // CONTROLADORES
        // ============================

        builder.Services.AddControllers();

        // ============================
        // CONFIGURACION JSON
        // ============================

        builder.Services.AddMvc().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.MaxDepth = 0;
        });

        // ============================
        // SWAGGER / OPENAPI
        // ============================

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // ============================
        // ENTITY FRAMEWORK
        // ============================

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("DefaultConnection")
            )
        );
        builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // ============================
        // UNIDAD DE TRABAJO
        // ============================

        builder.Services.AddScoped<IUnidadTrabajoEF, UnidadTrabajoEF>();

        // ============================
        // LOGICA DE NEGOCIO
        // ============================

        builder.Services.AddScoped<IUsuarioLN, UsuarioLN>();
        builder.Services.AddScoped<IInventarioLN, InventarioLN>();
        builder.Services.AddScoped<IDescargaLN, DescargaLN>();
        builder.Services.AddScoped<IPrestamoLN, PrestamoLN>();
        builder.Services.AddScoped<ISancionLN, SancionLN>();
        builder.Services.AddScoped<IHorarioSeccionLN, HorarioSeccionLN>();

        // ============================
        // CORS (para Angular / React)
        // ============================

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
        });

        // ============================
        // AUTORIZACION
        // ============================

        builder.Services.AddAuthorization();

        // ============================
        // BUILD APP
        // ============================

        var app = builder.Build();

        // ============================
        // MIDDLEWARE
        // ============================

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors("AllowAll");

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
