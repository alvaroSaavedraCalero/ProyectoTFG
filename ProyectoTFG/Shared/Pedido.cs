using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoTFG.Shared
{
    public class Pedido
    {
        #region propiedades clase pedido

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public String IdPedido { get; set; } = "";

        [BsonElement("idCliente")]
        public String IdCliente { get; set; } = "";

        [BsonElement("itemsPedido")]
        public List<ItemPedido> ItemsPedido { get; set; } = new List<ItemPedido>();

        [BsonElement("subtotal")]
        public Decimal SubTotal { get; set; } 

        [BsonElement("gastosEnvio")]
        public Decimal GastosEnvio { get; set; } = 2;

        [BsonElement("total")]
        public Decimal Total { get; set; } 

        [BsonElement("direccionEnvio")]
        public Direccion DireccionEnvio { get; set; } = new Direccion();

        [BsonElement("direccionFacturacion")]
        public Direccion DireccionFacturacion { get; set; } = new Direccion();

        [BsonElement("fechaPedido")]
        public DateTime FechaPedido { get; set; } = DateTime.Now;

        #endregion
    }
}
