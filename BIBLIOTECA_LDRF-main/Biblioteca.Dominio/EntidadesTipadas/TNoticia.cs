using System;

namespace Biblioteca.Dominio.EntidadesTipadas
{
    public class TNoticia
    {
        public int IdNoticia { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Tipo { get; set; } = "general";
        public string Estado { get; set; } = "publicada";
        public int CreadoPor { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }

        public TUsuario? UsuarioCreador { get; set; }
    }
}
