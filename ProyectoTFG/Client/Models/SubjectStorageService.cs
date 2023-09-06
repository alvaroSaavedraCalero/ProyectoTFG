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

        private String jwt;
        private Cliente datosCliente;

        public SubjectStorageService()
        {
            jwtSubject.Subscribe<String>((String jwt) => this.jwt = jwt);
            datosClienteSubject.Subscribe<Cliente>((Cliente c) => this.datosCliente = c);
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

        #endregion
    }
}
