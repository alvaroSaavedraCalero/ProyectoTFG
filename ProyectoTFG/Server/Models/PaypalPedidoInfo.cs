using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ProyectoTFG.Shared;

namespace ProyectoTFG.Server.Models
{
    public class PaypalPedidoInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String id { get; set; }

        [BsonElement("idCliente")]
        public String IdCliente { get; set; }

        [BsonElement("idCargo")]
        public String IdCargo { get; set; }

        [BsonElement("paypalContextClient")]
        public String PayPalContextClient { get; set; }

        [BsonElement("pedido")]
        public Pedido Pedido { get; set; }
    }
}
