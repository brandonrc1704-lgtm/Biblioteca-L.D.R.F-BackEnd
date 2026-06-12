using System;

namespace Biblioteca.Dominio.Entidades
{
    public class Descarga
    {
        public int IdDescarga { get; set; }
        public int IdUsuario { get; set; }
        public int IdProducto { get; set; }
        public DateTime FechaDescarga { get; set; }
        public string? IpDescarga { get; set; }
        public string? Dispositivo { get; set; }
        public string? Observaciones { get; set; }

        public Usuario Usuario { get; set; } = null!;
        public Inventario Producto { get; set; } = null!;
    }
}
