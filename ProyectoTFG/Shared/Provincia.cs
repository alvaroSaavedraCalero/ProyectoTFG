using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoTFG.Shared
{
    public class Provincia
    {
        #region propiedades de clase Provincia

        [BsonElement("CCOM")]
        public String CCOM { get; set; } = "";

        [BsonElement("CPRO")]
        public String CPRO { get; set; } = "";

        [BsonElement("PRO")]
        public String PRO { get; set; } = "";

        #endregion
    }
}
