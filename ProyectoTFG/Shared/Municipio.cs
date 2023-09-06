using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoTFG.Shared
{
    public class Municipio
    {
        #region propiedades clase Municipio

        //codigo provincia, codigo municipio, nombre municipio

        [BsonElement("CPRO")]
        public String CPRO { get; set; } = "";

        [BsonElement("CMUM")]
        public String CMUM { get; set; } = "";

        [BsonElement("CUN")]
        public String CUN { get; set; } = "";

        [BsonElement("DMUN50")]
        public String DMUN50 { get; set; } = "";

        #endregion
    }
}
