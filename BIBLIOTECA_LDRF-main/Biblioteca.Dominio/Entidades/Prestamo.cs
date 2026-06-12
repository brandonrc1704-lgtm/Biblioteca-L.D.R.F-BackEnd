using System;
using System.Collections.Generic;

namespace Biblioteca.Dominio.Entidades
{
    public class Prestamo
    {
        public int IdPrestamo { get; set; }
        public int IdUsuario { get; set; }
        public int IdProducto { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateOnly FechaLimite { get; set; }
        public DateOnly? FechaDevolucion { get; set; }
        public int Renovaciones { get; set; }
        public string Estado { get; set; } = "activo";
        public int? EntregadoPor { get; set; }
        public int? RecibidoPor { get; set; }
        public string? Observaciones { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }

        public Usuario Usuario { get; set; } = null!;
        public Inventario Producto { get; set; } = null!;
        public Usuario? UsuarioEntrega { get; set; }
        public Usuario? UsuarioRecibe { get; set; }
        public ICollection<Sancion> Sanciones { get; set; } = new List<Sancion>();
    }
}
