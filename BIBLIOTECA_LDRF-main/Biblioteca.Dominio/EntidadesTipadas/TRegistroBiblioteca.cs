using System;

namespace Biblioteca.Dominio.EntidadesTipadas
{
    public class TRegistroBiblioteca
    {
        public int IdRegistro { get; set; }
        public int IdUsuario { get; set; }
        public string TipoMovimiento { get; set; } = "entrada";
        public DateTime FechaHora { get; set; }
        public int? RegistradoPor { get; set; }
        public string? Observaciones { get; set; }
        public DateTime CreadoEn { get; set; }

        public TUsuario? Usuario { get; set; }
        public TUsuario? UsuarioRegistrador { get; set; }
    }
}
