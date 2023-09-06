using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoTFG.Shared
{
    public class ProductoAPI
    {
        #region propiedades de la clase

        public int id { get; set; } = 0;
        public String title { get; set; } = "";
        public Decimal price { get; set; } = 0;
        public String description { get; set; } = "";
        public String category { get; set; } = "";
        public String image { get; set; } = "";
        public Calificacion rating { get; set; } = new Calificacion();

        public class Calificacion
        {
            public Decimal rate { get; set; } = 0;
            public int count { get; set; } = 0;
        }

        #endregion
    }
}
