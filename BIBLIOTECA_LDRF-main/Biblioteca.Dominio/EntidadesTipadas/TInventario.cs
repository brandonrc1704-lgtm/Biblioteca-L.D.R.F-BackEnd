using System;
using System.Collections.Generic;

namespace Biblioteca.Dominio.EntidadesTipadas
{
    public class TInventario
    {
        public int IdProducto { get; set; }
        public string? CodigoInterno { get; set; }
        public string TipoObjeto { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Autor { get; set; }
        public string? Editorial { get; set; }
        public string? NumeroSaga { get; set; }
        public string? Edicion { get; set; }
        public string? Categoria { get; set; }
        public string? Ubicacion { get; set; }
        public string? Portada { get; set; }
        public string? ArchivoUrl { get; set; }
        public string? TipoArchivo { get; set; }
        public decimal? TamanoMb { get; set; }
        public int StockFisico { get; set; }
        public bool PermitePrestamo { get; set; } = true;
        public bool PermiteDescarga { get; set; }
        public string Visibilidad { get; set; } = "todos";
        public string Estado { get; set; } = "disponible";
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }

        public ICollection<TDescarga> Descargas { get; set; } = new List<TDescarga>();
        public ICollection<TPrestamo> Prestamos { get; set; } = new List<TPrestamo>();
    }
}
