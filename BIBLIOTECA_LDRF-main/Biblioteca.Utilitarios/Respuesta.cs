using System;
using System.Collections.Generic;
using System.Text;
namespace Biblioteca.Utilitarios
{
    public partial class Respuesta<T>
    {
        public bool IndicadorTransaccion { get; set; } = true;
        public byte TipoMensaje { get; set; } = (byte)Enumeradores.TipoMensaje.Informativo;
        public string TituloRespuesta { get; set; } = "Transacción Exitosa";
        public string MensajeRespuesta { get; set; } = "Transacción realizada correctamente";
        public string? MetodoError { get; set; }
        public string? ClaseError { get; set; }
        public string? CantidadItems { get; set; }
        public string? Faltante { get; set; }
        public T? ValorRetorno { get; set; }

        public void Validacion(string? mensaje = null)
        {
            IndicadorTransaccion = false;
            TipoMensaje = (byte)Enumeradores.TipoMensaje.Validacion;

            MensajeRespuesta = string.IsNullOrEmpty(mensaje)
                ? "Se presentó un error inesperado, favor comunicarlo al encargado"
                : mensaje;
        }

        public void Error(string? mensaje = null)
        {
            IndicadorTransaccion = false;
            TipoMensaje = (byte)Enumeradores.TipoMensaje.Error;

            MensajeRespuesta = string.IsNullOrEmpty(mensaje)
                ? "Se presentó un error inesperado, favor comunicarlo al encargado"
                : mensaje;
        }
    }

    public class Enumeradores
    {
        public enum TipoMensaje
        {
            Satisfactorio = 1,
            Informativo = 2,
            Validacion = 3,
            Error = 4
        }
    }
}