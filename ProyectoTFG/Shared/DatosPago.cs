using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoTFG.Shared
{
    public class DatosPago
    {
        //clase q vamos a usar para finalizar el Pedido...
        //va a contener:
        //- PedidoActual
        //- Direccion Principal del cliente, direccion de envio alternativa
        //- datos del destinatario del envio (nombre,apellidos,telefono y email)
        //- datos factura (nombre a quien va dirigida factura, y doc.id)
        //- datos de pago (tarjeta o paypal)

        //datos envio
        public Direccion DireccionPrincipal { get; set; } = new Direccion();
        public Direccion DireccionEnvio { get; set; } = new Direccion();
        public String NombreDestinatario { get; set; } = "";
        public String ApellidosDestinatario { get; set; } = "";
        public String TelefonoDestinatario { get; set; } = "";
        public String EmailDestinatario { get; set; } = "";

        //datos facturacion
        public String NombreFactura { get; set; } = "";
        public String DocFiscalFactura { get; set; } = "";

        //datos pago
        public String MetodoPago { get; set; } = "pagocard";
        public String NumeroTarjeta { get; set; } = "";
        public String NombreBanco { get; set; } = "";
        public int MesCaducidad { get; set; } = 0;
        public int AnioCaducidad { get; set; } = 0;
        public int CVV { get; set; } = 0;
    }
}
