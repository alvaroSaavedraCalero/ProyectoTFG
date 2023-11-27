using ProyectoTFG.Server.Models.Interfaces;
using ProyectoTFG.Shared;

namespace ProyectoTFG.Server.Models
{
    public class SqlServerService : IAccesoDatos
    {
        #region propiedades de la clase

        public String cadenaConexion { get; set; }
        private IConfiguration accesoAppSettings;

        public SqlServerService(IConfiguration accesoAppSettings)
        {
            this.accesoAppSettings = accesoAppSettings;
            this.cadenaConexion = this.accesoAppSettings["SqlServer:cadenaConexion"];
        }

        #endregion

        #region metodos de la clase

        #region metodos Cliente

        public Task<Cliente> ComprobarCredenciales(string email, string password)
        {
            throw new NotImplementedException();
        }

        public Task<Cliente> ObtenerClienteId(string id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GuardarCredencialesGoogle(string idGoogle, string idCliente)
        {
            throw new NotImplementedException();
        }

        public Task<Cliente> ObtenerClienteIdGoogle(string idGoogle)
        {
            throw new NotImplementedException();
        }

        public Task<Cliente> RegistrarClienteGoogle(Cliente cliente)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ActivarCuenta(string idCliente)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RegistrarCliente(Cliente cliente)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RegistrarDireccion(Direccion direccion, string idCliente)
        {
            throw new NotImplementedException();
        }

        public Task<Cliente> ModificarCliente(Cliente cliente, bool cambioPass)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ModificarImagen(Cliente cliente, string imagen)
        {
            throw new NotImplementedException();
        }

        public Task<Direccion> ModificarDireccion(Direccion direcccion, string idCliente)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EliminarDireccion(Direccion direcccion, string idCliente)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region metodos Tienda

        public Task<Pedido> RegistrarPedido(Pedido pedido, Cliente cliente)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IntroducirDatosPayPal(PaypalPedidoInfo datosPedido)
        {
            throw new NotImplementedException();
        }

        public Task<PaypalPedidoInfo> RecuperarDatosPayPal(string idPedido)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DesearProducto(string idProducto, string idCliente)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DesDesearProducto(string idProducto, string idCliente)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SubirComentario(string idCliente, string comentario, string nombreCliente, string idProducto, string imagenCliente)
        {
            throw new NotImplementedException();
        }

        public Task<List<ComentarioCli>> RecuperarComentariosProducto(string idProducto)
        {
            throw new NotImplementedException();
        }

        public Task<Pedido> ObtenerPedidoId(string idPedido)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region metodos Administracion

        public Task<List<Cliente>> GetClientes()
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        
    }
}
