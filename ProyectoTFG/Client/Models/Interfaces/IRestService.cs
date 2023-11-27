using ProyectoTFG.Shared;

namespace ProyectoTFG.Client.Models.Interfaces
{
    public interface IRestService
    {
        #region zona Cliente

        Task<RestMessage> Login(Cuenta cuenta);
        Task<RestMessage> ObtenerClienteGoogle(String idGoogle);
        Task<RestMessage> ObtenerClienteId(String idCliente);
        Task<RestMessage> Registro(Cliente cliente);
        Task<RestMessage> ModificarCliente(Cliente cliente, Boolean cambioPass, String jwt);
        Task<RestMessage> ModificarImagenCliente(Cliente cliente, String imagenB64, String jwt);
        Task<RestMessage> IntroducirDireccion(Direccion direc, Cliente cliente, String jwt);
        Task<List<Provincia>> RecuperarProvincias();
        Task<List<Municipio>> RecuperarMunicipios(String codprov);
        Task<String> LoginGoogle();
        Task<RestMessage> ModificarDireccion(Direccion direccion, Cliente cliente, String jwt);
        Task<RestMessage> EliminarDireccion(Direccion direccion, Cliente cliente, String jwt);

        #endregion

        #region zona Tienda

        Task<List<ProductoAPI>> RecuperarProductosAleatorios();
        Task<List<ProductoAPI>> RecuperarProductosCategoria(String categoria);
        Task<ProductoAPI> RecuperarProducto(String idProducto);
        Task<RestMessage> AlmacenarPedido(Pedido pedido, Cliente cliente, String jwt);
        Task<RestMessage> RealizarPagoTarjeta(Cliente cliente, DatosPago datosPago, String jwt);
        Task<String> RealizarPagoPayPal(Cliente cliente, DatosPago datosPago, String jwt);
        Task<RestMessage> DesearProducto(Cliente clienteActual, ProductoAPI producto, String jwt);
        Task<RestMessage> DesDesearProd(Cliente clienteActual, ProductoAPI producto, String jwt);
        Task<RestMessage> AlmacenarComentario(String jwt, String comentario, String idCliente, String nombreCliente, String idProducto, String imagenCliente);
        Task<List<ComentarioCli>> RecuperarComentariosProd(ProductoAPI producto);
        Task<Pedido> RecuperarPedidoPorId(String idPedido);

        #endregion

        #region zona Administracion

        Task<List<Cliente>> GetClientes(String jwt);

        #endregion
    }
}
