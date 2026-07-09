using System;

namespace Biblioteca.Dominio.Entidades
{
    public class CorreoEnviado
    {
        public int IdCorreo { get; set; }
        public int IdUsuario { get; set; }
        public int? IdSancion { get; set; }
        public string CorreoDestino { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Estado { get; set; } = "pendiente";
        public string? ErrorEnvio { get; set; }
        public DateTime? EnviadoEn { get; set; }
        public DateTime CreadoEn { get; set; }

        public Usuario Usuario { get; set; } = null!;
        public Sancion? Sancion { get; set; }
    }
}
