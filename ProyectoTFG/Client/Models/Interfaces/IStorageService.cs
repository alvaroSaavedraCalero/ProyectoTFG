using ProyectoTFG.Shared;

namespace ProyectoTFG.Client.Models.Interfaces
{
    public interface IStorageService
    {
        #region metodos sincronos de acceso a datos (para subjects)

        String RecuperarJWT();
        Cliente RecuperarDatosCliente();
        List<ProductoAPI> RecuperarListaDeseos();
        void AlmacenarJWT(String jwt);
        void AlmacenarDatosCliente(Cliente datosCliente);
        void AlmacenarListaDeseos(List<ProductoAPI> listaDeseos);

        #endregion


        #region metodos asincronos para acceso a datos (para indexedDB y local/sessionStorage)

        Task<String> RecuperarJWTAsync();
        Task<Cliente> RecuperarDatosClienteAsync();
        Task<List<ProductoAPI>> RecuperarListaDeseosAsync();
        Task AlmacenarJWTAsync(String jwt);
        Task AlmacenarDatosClienteAsync(Cliente datosCliente);
        Task AlmacenarListaDeseosAsync(List<ProductoAPI> listaDeseos);

        #endregion
    }
}
