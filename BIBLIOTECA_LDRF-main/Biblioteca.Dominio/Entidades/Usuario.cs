using System;
using System.Collections.Generic;

namespace Biblioteca.Dominio.Entidades
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Credencial { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public DateOnly? FechaNacimiento { get; set; }
        public string? Seccion { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Rol { get; set; } = "estudiante";
        public string Estado { get; set; } = "activo";
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }
        public string? TokenActivo { get; set; }

        public ICollection<Descarga> Descargas { get; set; } = new List<Descarga>();
        public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
        public ICollection<Prestamo> PrestamosEntregados { get; set; } = new List<Prestamo>();
        public ICollection<Prestamo> PrestamosRecibidos { get; set; } = new List<Prestamo>();
        public ICollection<Sancion> Sanciones { get; set; } = new List<Sancion>();
    }
}
