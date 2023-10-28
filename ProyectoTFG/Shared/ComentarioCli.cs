using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoTFG.Shared
{
    public class ComentarioCli
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String IdComentario { get; set; }

        [BsonElement("idCliente")]
        public String IdCliente { get; set; } = "";

        [BsonElement("comentario")]
        public String Comentario { get; set; } = "";

        [BsonElement("nombreCliente")]
        public String NombreCliente { get; set; } = "";

        [BsonElement("idProducto")]
        public String IdProducto { get; set; } = "";

        [BsonElement("imagenCliente")]
        public String ImagenCliente { get; set; } = "";
    }
}
