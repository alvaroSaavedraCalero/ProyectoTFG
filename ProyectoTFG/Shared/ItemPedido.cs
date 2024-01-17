using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoTFG.Shared
{
    public class ItemPedido
    {
        #region propiedades de la clase itempedido

        [BsonElement("idItemPedido")]
        public String IdItemPedido { get; set; } = "";

        [BsonElement("idPedido")]
        public String IdPedido { get; set; } = "";

        [BsonElement("producto")]
        public ProductoAPI ProductoItem { get; set; } = new ProductoAPI();

        [BsonElement("cantidadItem")]
        public int CatidadItem { get; set; } = 0;

        #endregion
    }
}
