using ProyectoTFG.Shared;

namespace ProyectoTFG.Client.Models.Interfaces
{
    public interface IStorageService
    {
        #region metodos sincronos de acceso a datos (para subjects)

        String RecuperarJWT();
        Cliente RecuperarDatosCliente();
        void AlmacenarJWT(String jwt);
        void AlmacenarDatosCliente(Cliente datosCliente);

        #endregion


        #region metodos asincronos para acceso a datos (para indexedDB y local/sessionStorage)

        Task<String> RecuperarJWTAsync();
        Task<Cliente> RecuperarDatosClienteAsync();
        Task AlmacenarJWTAsync(String jwt);
        Task AlmacenarDatosClienteAsync(Cliente datosCliente);

        #endregion
    }
}
