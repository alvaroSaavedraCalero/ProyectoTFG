namespace ProyectoTFG.Server.Models.Interfaces
{
    public interface IClienteEmail
    {
        #region propiedades de la interface 
        public String UserId { get; set; }
        public String Key { get; set; }

        #endregion

        #region metodos de la interface
        public Boolean enviarEmail(String emailCliente, String subject, String body, String? ficheroAdjunto);

        #endregion
    }
}
