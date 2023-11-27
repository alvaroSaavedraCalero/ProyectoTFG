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

        #region metodos de la clase

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            ProductoAPI productoAPI = (ProductoAPI)obj;

            return productoAPI.id == this.id;
        }

        public override int GetHashCode()
        {
            return this.id.GetHashCode();
        }

        #endregion
    }
}
