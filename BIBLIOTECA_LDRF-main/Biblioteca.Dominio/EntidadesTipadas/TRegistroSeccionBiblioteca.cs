using System;

namespace Biblioteca.Dominio.EntidadesTipadas
{
    public class TRegistroSeccionBiblioteca
    {
        public int IdRegistroSeccion { get; set; }
        public string Grado { get; set; } = string.Empty;
        public string Seccion { get; set; } = string.Empty;
        public DateOnly FechaUso { get; set; }
        public string UsoBiblioteca { get; set; } = string.Empty;
        public string ReservadoPor { get; set; } = string.Empty;
        public int? RegistradoPor { get; set; }
        public string? Observaciones { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }

        public TUsuario? UsuarioRegistrador { get; set; }
    }
}
