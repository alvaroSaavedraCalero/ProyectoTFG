using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoTFG.Shared
{
    public class RestMessage
    {
        #region propiedades clase RestMesaage
        public int Codigo { get; set; }
        public String? Mensaje { get; set; }
        public String? Error { get; set; }
        public String? TokenSesion { get; set; }
        public Cliente? DatosCliente { get; set; }
        public Object? OtrosDatos { get; set; }

        #endregion
    }
}
