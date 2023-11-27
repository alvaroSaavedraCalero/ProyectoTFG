using ProyectoTFG.Client.Models.Interfaces;
using ProyectoTFG.Shared;
using System.Reactive.Subjects;

namespace ProyectoTFG.Client.Models
{
    public class SubjectStorageService : IStorageService
    {
        #region propiedades de la clase

        private BehaviorSubject<String> jwtSubject = new BehaviorSubject<String>("");
        private BehaviorSubject<Cliente> datosClienteSubject = new BehaviorSubject<Cliente>(new Cliente());
        private BehaviorSubject<List<ProductoAPI>> listaDeseosSubject = new BehaviorSubject<List<ProductoAPI>>(new List<ProductoAPI>());

        private String jwt;
        private Cliente datosCliente;
        private List<ProductoAPI> listaDeseos;

        public SubjectStorageService()
        {
            jwtSubject.Subscribe<String>((String jwt) => this.jwt = jwt);
            datosClienteSubject.Subscribe<Cliente>((Cliente c) => this.datosCliente = c);
            listaDeseosSubject.Subscribe<List<ProductoAPI>>((List<ProductoAPI> l) => this.listaDeseos = l);
        }

        #endregion


        #region metodos sincronos 

        /// <summary>
        /// Almacena los datos del cliente en el subject
        /// </summary>
        /// <param name="datosCliente">Datos del cliente</param>
        public void AlmacenarDatosCliente(Cliente datosCliente)
        {
            this.datosClienteSubject.OnNext(datosCliente);
        }

        /// <summary>
        /// Almacena el jwt en el subject
        /// </summary>
        /// <param name="jwt">String con el jwt</param>
        public void AlmacenarJWT(string jwt)
        {
            this.jwtSubject.OnNext(jwt);
        }

        /// <summary>
        /// Recupera los datos del cliente del subject
        /// </summary>
        /// <returns>Cliente recuperado</returns>
        public Cliente RecuperarDatosCliente()
        {
            return this.datosCliente;
        }

        /// <summary>
        /// Recupera el jwt del subject
        /// </summary>
        /// <returns>JWT recuperado</returns>
        public string RecuperarJWT()
        {
            return this.jwt;
        }

        /// <summary>
        /// Recupera la lista de deseos del subject
        /// </summary>
        /// <returns>La lista de deseos recuperada</returns>
        public List<ProductoAPI> RecuperarListaDeseos()
        {
            return this.listaDeseos;
        }

        /// <summary>
        /// Almacena la lista de deseos en el subject
        /// </summary>
        /// <param name="listaDeseos">Lista de deseos</param>
        public void AlmacenarListaDeseos(List<ProductoAPI> listaDeseos)
        {
            this.listaDeseosSubject.OnNext(listaDeseos);
        }

        /// <summary>
        /// Reinicia los subjects de forma que ya no habra datos del cliente
        /// </summary>
        public void CerrarSesion()
        {
            this.jwt = "";
            this.datosCliente = new Cliente();
            this.listaDeseos = new List<ProductoAPI>();
        }

        #endregion


        #region metodos asincronos (sin implementar)

        public Task AlmacenarDatosClienteAsync(Cliente datosCliente)
        {
            throw new NotImplementedException();
        }

        public Task AlmacenarJWTAsync(string jwt)
        {
            throw new NotImplementedException();
        }

        public Task<Cliente> RecuperarDatosClienteAsync()
        {
            throw new NotImplementedException();
        }

        public Task<string> RecuperarJWTAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<ProductoAPI>> RecuperarListaDeseosAsync()
        {
            throw new NotImplementedException();
        }

        public Task AlmacenarListaDeseosAsync(List<ProductoAPI> listaDeseos)
        {
            throw new NotImplementedException();
        }

        public Task CerrarSesionAsync()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
