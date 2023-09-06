using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoTFG.Shared
{
    public class Direccion
    {
        #region propiedades de la clase

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String IdDireccion { get; set; }

        [BsonElement("nombreEmpresa")]
        public String NombreEmpresa { get; set; } = "";

        [BsonElement("nombreContacto")]
        public String NombreContacto { get; set; } = "";

        [BsonElement("apellidosContacto")]
        public String ApellidosContacto { get; set; } = "";

        [BsonElement("telefonoContacto")]
        public String TelefonoContacto { get; set; } = "";

        [BsonElement("calle")]
        public String Calle { get; set; } = "";

        [BsonElement("numero")]
        public int Numero { get; set; }

        [BsonElement("cp")]
        public int CP { get; set; }

        [BsonElement("provincia")]
        public Provincia ProvDirec { get; set; } = new Provincia();

        [BsonElement("municipio")]
        public Municipio MuniDirecc { get; set; } = new Municipio();

        [BsonElement("pais")]
        public String Pais { get; set; } = "España";

        [BsonElement("esPrincipal")]
        public Boolean EsPrincipal { get; set; } = false;

        [BsonElement("esFacturacion")]
        public Boolean EsFaturacion { get; set; } = false;

        #endregion
    }
}
