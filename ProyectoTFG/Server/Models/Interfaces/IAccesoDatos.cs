using ProyectoTFG.Shared;

namespace ProyectoTFG.Server.Models.Interfaces
{
    public interface IAccesoDatos
    {
        #region metodos Cliente

        Task<Cliente> ComprobarCredenciales(String email, String password);
        Task<Cliente> ObtenerClienteId(String id);
        Task<Boolean> GuardarCredencialesGoogle(String idGoogle, String idCliente);
        Task<Cliente> ObtenerClienteIdGoogle(String idGoogle);
        Task<Cliente> RegistrarClienteGoogle(Cliente cliente);
        Task<Boolean> ActivarCuenta(String idCliente);
        Task<Boolean> RegistrarCliente(Cliente cliente);
        Task<Boolean> RegistrarDireccion(Direccion direccion, String idCliente);
        Task<Cliente> ModificarCliente(Cliente cliente, Boolean cambioPass);
        Task<Boolean> ModificarImagen(Cliente cliente, String imagen);
        Task<Direccion> ModificarDireccion(Direccion direcccion, String idCliente);
        Task<Boolean> EliminarDireccion(Direccion direcccion, String idCliente);

        #endregion

        #region metodos Tienda

        Task<Pedido> RegistrarPedido(Pedido pedido, Cliente cliente);
        Task<Boolean> IntroducirDatosPayPal(PaypalPedidoInfo datosPedido);
        Task<PaypalPedidoInfo> RecuperarDatosPayPal(String idPedido);
        Task<Boolean> DesearProducto(String idProducto, String idCliente);
        Task<Boolean> DesDesearProducto(String idProducto, String idCliente);
        Task<Boolean> SubirComentario(String idCliente, String comentario, String nombreCliente, String idProducto, String imagenCliente);
        Task<List<ComentarioCli>> RecuperarComentariosProducto(String idProducto);

        #endregion

        #region metodos Administracion

        Task<List<Cliente>> GetClientes();

        #endregion
    }
}
