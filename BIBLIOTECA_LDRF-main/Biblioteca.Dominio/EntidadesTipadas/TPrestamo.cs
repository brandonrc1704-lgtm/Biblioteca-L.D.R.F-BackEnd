using System;
using System.Collections.Generic;

namespace Biblioteca.Dominio.EntidadesTipadas
{
    public class TPrestamo
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

        public TUsuario? Usuario { get; set; }
        public TInventario? Producto { get; set; }
        public TUsuario? UsuarioEntrega { get; set; }
        public TUsuario? UsuarioRecibe { get; set; }
        public ICollection<TSancion> Sanciones { get; set; } = new List<TSancion>();
    }
}
