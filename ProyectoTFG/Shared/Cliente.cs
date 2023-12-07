using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoTFG.Shared
{
    public class Cliente
    {
        #region propiedades de la clase

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String IdCliente { get; set; }

        [BsonElement("rol")]
        public String Rol { get; set; } = "Cliente";


        [Required(ErrorMessage = "*Nombre obligatortio")]
        [MaxLength(50, ErrorMessage = "No se admiten mas de 50 caracteres")]
        [BsonElement("nombre")]
        public String Nombre { get; set; } = "";



        [Required(ErrorMessage = "*Apellidos obligatortios")]
        [MaxLength(50, ErrorMessage = "No se admiten mas de 300 caracteres")]
        [BsonElement("apellidos")]
        public String Apellidos { get; set; } = "";




        [Required(ErrorMessage = "*Telefono de contacto obligatorio")]
        [RegularExpression(@"^[9|6|7][0-9]{8}$", ErrorMessage = "*Formato invalido de tlfno, es asi: 666 11 22 33")]
        [Phone(ErrorMessage = "*Formato invalido de telefono")]
        [BsonElement("telefono")]
        public String Telefono { get; set; } = "000 00 00 00";



        [BsonElement("fechaNacimiento")]
        public DateTime FechaNacimiento { get; set; } = DateTime.Now;


        [BsonElement("genero")]
        public String Genero { get; set; } = "Hombre";



        [BsonElement("nif")]
        public String NIF { get; set; } = "00000000A";



        [BsonElement("cuenta")]
        public Cuenta cuenta { get; set; } = new Cuenta();



        [BsonElement("direcciones")]
        public List<Direccion> MisDirecciones { get; set; } = new List<Direccion>();



        [BsonElement("pedidos")]
        public List<Pedido> MisPedidos { get; set; } = new List<Pedido>();


        [BsonElement("listaDeseos")]
        public List<ProductoAPI> ListaDeseos { get; set; } = new List<ProductoAPI>();


        [BsonIgnore]
        public Pedido PedidoActual { get; set; } = new Pedido();

        #endregion

    }
}
