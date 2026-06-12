using System;

namespace Biblioteca.Dominio.Entidades
{
    public class HorarioSeccion
    {
        public int IdHorario { get; set; }
        public string Seccion { get; set; } = string.Empty;
        public string DiaSemana { get; set; } = string.Empty;
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public string? Materia { get; set; }
        public string? Docente { get; set; }
        public string? Aula { get; set; }
        public string Estado { get; set; } = "clase";
        public string? Observaciones { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }
    }
}
