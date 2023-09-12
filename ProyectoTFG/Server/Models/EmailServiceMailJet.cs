using ProyectoTFG.Server.Models.Interfaces;
using System.Net;
using System.Net.Mail;

namespace ProyectoTFG.Server.Models
{
    public class EmailServiceMailJet : IClienteEmail
    {
        #region propiedades de la clase
        public string UserId { get; set; }
        public string Key { get; set; }
        private IConfiguration accesoAppSettings;

        public EmailServiceMailJet(IConfiguration configuracion)
        {
            this.accesoAppSettings = configuracion;
            this.UserId = accesoAppSettings["MailJet:UserId"];
            this.Key = accesoAppSettings["MailJet:Key"];
        }

        #endregion


        #region metodos de la clase
        public bool enviarEmail(string emailCliente, string subject, string body, string? ficheroAdjunto)
        {
            try
            {
                // Con SmtpClient nos conectamos al servidor, MailMessage para crear el email
                SmtpClient clienteSmtp = new SmtpClient("in-v3.mailjet.com");
                clienteSmtp.Credentials = new NetworkCredential(this.UserId, this.Key);

                MailMessage mensaje = new MailMessage("a.saavedra.calero@gmail.com", emailCliente);
                mensaje.Subject = subject;
                mensaje.IsBodyHtml = true;
                mensaje.Body = body;

                if (!String.IsNullOrEmpty(ficheroAdjunto))
                {
                    mensaje.Attachments.Add(new Attachment(ficheroAdjunto, "application/pdf"));
                }
                clienteSmtp.Send(mensaje);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion


    }
}
