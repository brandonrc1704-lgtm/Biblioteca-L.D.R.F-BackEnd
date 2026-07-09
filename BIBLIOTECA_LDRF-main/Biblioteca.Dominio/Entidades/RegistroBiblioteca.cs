using System;

namespace Biblioteca.Dominio.Entidades
{
    public class RegistroBiblioteca
    {
        public int IdRegistro { get; set; }
        public int IdUsuario { get; set; }
        public string TipoMovimiento { get; set; } = "entrada";
        public DateTime FechaHora { get; set; }
        public int? RegistradoPor { get; set; }
        public string? Observaciones { get; set; }
        public DateTime CreadoEn { get; set; }

        public Usuario Usuario { get; set; } = null!;
        public Usuario? UsuarioRegistrador { get; set; }
    }
}
