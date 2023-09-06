using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoTFG.Shared
{
    public class Cuenta
    {
        #region propiedades de la clase 

        [Required(ErrorMessage = "*Login obligatorio")]
        [MaxLength(50, ErrorMessage = "*Maximo numero de caracteres permitidos es de 50")]
        [BsonElement("login")]
        public String Login { get; set; } = ""; 

        [Required(ErrorMessage = "*Email obligatorio")]
        [EmailAddress(ErrorMessage = "*Formato de email incorrecto")]
        [BsonElement("email")]
        public String Email { get; set; } = "";

        [Required(ErrorMessage = "*Contraseña obligatoria")]
        [MinLength(6, ErrorMessage = "*El tamaño minimo de la contraseña debe ser de 6 caracteres")]
        [MaxLength(50, ErrorMessage = "*El tamaño maximo de la contraseña es de 50 caracteres")]
        [BsonElement("password")]
        public String Password { get; set; } = ""; 

        [BsonElement("cuentaActiva")]
        public Boolean CuentaActivada { get; set; } = false;

        [BsonElement("imagenAvatarBASE64")]
        public String ImagenAvatarBASE64 { get; set; } = "";

        #endregion
    }
}
