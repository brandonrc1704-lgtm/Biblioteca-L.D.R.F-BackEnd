using System;

namespace Biblioteca.Dominio.EntidadesTipadas
{
    public class TSancionEscapado
    {
        public int IdEscapado { get; set; }
        public int IdUsuario { get; set; }
        public int? IdHorario { get; set; }
        public DateOnly Fecha { get; set; }
        public TimeOnly Hora { get; set; }
        public string DiaSemana { get; set; } = string.Empty;
        public string Seccion { get; set; } = string.Empty;
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public string? Materia { get; set; }
        public string? Docente { get; set; }
        public string? Aula { get; set; }
        public string Estado { get; set; } = "pendiente";
        public string? Observaciones { get; set; }
        public int? IdSancion { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }
    }
}
