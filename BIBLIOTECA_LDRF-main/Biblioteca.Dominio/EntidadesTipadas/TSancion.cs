using System;

namespace Biblioteca.Dominio.EntidadesTipadas
{
    public class TSancion
    {
        public int IdSancion { get; set; }
        public int IdUsuario { get; set; }
        public int? IdPrestamo { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public int DiasSancion { get; set; } = 1;
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public string Estado { get; set; } = "activa";
        public string? Observaciones { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }

        public TUsuario? Usuario { get; set; }
        public TPrestamo? Prestamo { get; set; }
    }
}
