using Microsoft.AspNetCore.Identity;

namespace ProyectoTFG.Server.Models
{
    public class ClienteIdentity : IdentityUser
    {
        public String Nombre { get; set; }
        public String Apellidos { get; set; }
        public String NIF { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public String Genero { get; set; } = "";
        public String Descripcion { get; set; } = "";
        public String ImagenAvatar { get; set; } = "";
        public String ImagenAvatarBASE64 { get; set; } = "";
    }
}
