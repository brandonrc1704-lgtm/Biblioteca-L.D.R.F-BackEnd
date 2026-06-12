using System;

namespace Biblioteca.Dominio.EntidadesTipadas
{
    public class TDescarga
    {
        public int IdDescarga { get; set; }
        public int IdUsuario { get; set; }
        public int IdProducto { get; set; }
        public DateTime FechaDescarga { get; set; }
        public string? IpDescarga { get; set; }
        public string? Dispositivo { get; set; }
        public string? Observaciones { get; set; }

        public TUsuario? Usuario { get; set; }
        public TInventario? Producto { get; set; }
    }
}
