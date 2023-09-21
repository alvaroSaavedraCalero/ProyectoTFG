using ProyectoTFG.Client.Models.Interfaces;
using ProyectoTFG.Shared;
using System.Net.Http.Json;
using System.Text.Json;

namespace ProyectoTFG.Client.Models
{
    public class RestService : IRestService
    {
        #region propiedades de la clase

        private HttpClient clienteHttp; // para hacer peticiones al exterior

        public RestService(HttpClient clienteHttp)
        {
            this.clienteHttp = clienteHttp;
        }

        #endregion


        #region zona Cliente

        /// <summary>
        /// Realiza una petición a la API_REST de Cliente para recuperar un cliente de la base de datos.
        /// </summary>
        /// <param name="cuenta">Cuenta con los datos del cliente</param>
        /// <returns>La respuesta del servidor en forma de RestMessage</returns>
        public async Task<RestMessage> Login(Cuenta cuenta)
        {
            // Pasamos siempre la info al servidor a traves de un diccionario
            Dictionary<String, String> datos = new Dictionary<String, String>()
            {
                {"cuenta", JsonSerializer.Serialize<Cuenta>(cuenta)}
            };

            // Almacenamos la respuesta del servicio, en el cuerpo esta el RestMessage
            HttpResponseMessage respuesta = await this.clienteHttp.PostAsJsonAsync<Dictionary<String, String>>("api/Cliente/Login", datos);
            return await respuesta.Content.ReadFromJsonAsync<RestMessage>();
        }

        /// <summary>
        /// Realiza una peticion a la API_REST de Cliente para recuperar un cliente de google de la base de datos
        /// </summary>
        /// <param name="idGoogle">Id de google del cliente</param>
        /// <returns>La respuesta del servidor en forma de RestMessage</returns>
        public async Task<RestMessage> ObtenerClienteGoogle(string idGoogle)
        {
            Dictionary<String, String> datos = new Dictionary<String, String>()
            {
                {"idGoogle",idGoogle }
            };
            HttpResponseMessage respuesta = await this.clienteHttp.PostAsJsonAsync<Dictionary<String, String>>("api/Cliente/ObtenerClienteGoogle", datos);
            return await respuesta.Content.ReadFromJsonAsync<RestMessage>();
        }

        /// <summary>
        /// Realiza una petición a la API_REST de Cliente para introducir un cliente en la base de datos.
        /// </summary>
        /// <param name="cliente">Datos del cliente</param>
        /// <returns>La respuesta del servidor en forma de RestMessage</returns>
        public async Task<RestMessage> Registro(Cliente cliente)
        {
            Dictionary<String, String> datos = new Dictionary<String, String>()
            {
                {"cliente", JsonSerializer.Serialize<Cliente>(cliente)}
            };

            HttpResponseMessage respuesta = await this.clienteHttp.PostAsJsonAsync<Dictionary<String, String>>("api/Cliente/Registro", datos);
            return await respuesta.Content.ReadFromJsonAsync<RestMessage>();
        }

        /// <summary>
        /// Realiza una petición a la API_REST de Cliente para introducir una direccion en la base de datos.
        /// </summary>
        /// <param name="direc">Direccion a introducir</param>
        /// <param name="cliente">Cliente del cual es la direccion</param>
        /// <param name="jwt">JWT del cliente</param>
        /// <returns>La respuesta del servidor en forma de RestMessage</returns>
        public async Task<RestMessage> IntroducirDireccion(Direccion direc, Cliente cliente, String jwt)
        {
            Dictionary<String, String> datos = new Dictionary<String, String>()
            {
                {"cliente", JsonSerializer.Serialize<Cliente>(cliente) },
                {"direccion", JsonSerializer.Serialize<Direccion>(direc)},
                {"jwt", jwt }
            };

            HttpResponseMessage respuesta = await this.clienteHttp.PostAsJsonAsync<Dictionary<String, String>>("api/Cliente/IntroducirDireccion", datos);
            return await respuesta.Content.ReadFromJsonAsync<RestMessage>();
        }

        /// <summary>
        /// Realiza una petición a la API_REST de Cliente para recuperar las provincias.
        /// </summary>
        /// <returns>Una lista de Provincias</returns>
        public async Task<List<Provincia>> RecuperarProvincias()
        {
            return await this.clienteHttp.GetFromJsonAsync<List<Provincia>>("/api/Cliente/RecuperarProvincias");
        }

        /// <summary>
        /// Realiza una petición a la API_REST de Cliente para recuperar los municipios de una provincia.
        /// </summary>
        /// <param name="codprov">Codigo de la provincia</param>
        /// <returns>Una lista de Municipios</returns>
        public async Task<List<Municipio>> RecuperarMunicipios(string codprov)
        {
            return await this.clienteHttp.GetFromJsonAsync<List<Municipio>>($"/api/Cliente/RecuperarMunicipios?codprov={codprov}");
        }

        /// <summary>
        /// Realiza una peticion a la API_REST de Cliente para hacer login a traves de google.
        /// </summary>
        /// <returns>String con la url destino</returns>
        public async Task<String> LoginGoogle()
        {
            return await this.clienteHttp.GetStringAsync("api/Cliente/LoginGoogle");
        }

        /// <summary>
        /// Realiza una petición a la API_REST de Cliente para modificar los datos del cliente.
        /// </summary>
        /// <param name="cliente">Datos nuevos del Cliente</param>
        /// <param name="cambioPass">True en caso de que se quiera cambiar la contraseña</param>
        /// <param name="jwt">JWT del cliente</param>
        /// <returns>Respuesta del servidor en forma de RestMessage</returns>
        public async Task<RestMessage> ModificarCliente(Cliente cliente, Boolean cambioPass, String jwt)
        {
            Dictionary<String, String> datos = new Dictionary<String, String>
            {
                {"cliente", JsonSerializer.Serialize<Cliente>(cliente) },
                {"cambioPass", cambioPass.ToString() },
                {"jwt", jwt }
            };

            HttpResponseMessage respuesta = await this.clienteHttp.PostAsJsonAsync<Dictionary<String, String>>("api/Cliente/ModificarCliente", datos);
            return await respuesta.Content.ReadFromJsonAsync<RestMessage>();
        }

        /// <summary>
        /// Realiza una peticion a la API_REST de Cliente para subir la imagen del cliente
        /// </summary>
        /// <param name="cliente">Cliente en cuestion</param>
        /// <param name="imagenB64">Imagen en BASE 64</param>
        /// <param name="jwt">JWT del cliente</param>
        /// <returns>La respuesta del servidor en forma de RestMessage</returns>
        public async Task<RestMessage> ModificarImagenCliente(Cliente cliente, string imagenB64, string jwt)
        {
            Dictionary<String, String> datos = new Dictionary<String, String>
            {
                {"cliente", JsonSerializer.Serialize<Cliente>(cliente) },
                {"imagen", imagenB64 },
                {"jwt", jwt }
            };

            HttpResponseMessage respuesta = await this.clienteHttp.PostAsJsonAsync<Dictionary<String, String>>("api/Cliente/ModificarImagen", datos);
            return await respuesta.Content.ReadFromJsonAsync<RestMessage>();
        }

        /// <summary>
        /// Realiza una petición a la API_REST de Cliente para modificar los datos de una direccion.
        /// </summary>
        /// <param name="direccion">Datos nuevos de la Direccion</param>
        /// <param name="cliente">Cliente al que pertenecen las direcciones</param>
        /// <param name="jwt">JWT del cliente</param>
        /// <returns>Respuesta del servidor en forma de RestMessage</returns>
        public async Task<RestMessage> ModificarDireccion(Direccion direccion, Cliente cliente, String jwt)
        {
            Dictionary<String, String> datos = new Dictionary<String, String>
            {
                {"direccion", JsonSerializer.Serialize<Direccion>(direccion) },
                {"cliente", JsonSerializer.Serialize<Cliente>(cliente) },
                {"jwt", jwt }
            };

            HttpResponseMessage respuesta = await this.clienteHttp.PostAsJsonAsync<Dictionary<String, String>>("api/Cliente/ModificarDireccion", datos);
            return await respuesta.Content.ReadFromJsonAsync<RestMessage>();
        }

        /// <summary>
        /// Realiza una petición a la API_REST de Cliente para eliminar uan direccion de la base de datos.
        /// </summary>
        /// <param name="direccion">Direccion a eliminar</param>
        /// <param name="cliente">Cliente al que pertenece la direccion</param>
        /// <param name="jwt">JWT del cliente</param>
        /// <returns>Respuesta del servidor en forma de RestMessage</returns>
        public async Task<RestMessage> EliminarDireccion(Direccion direccion, Cliente cliente, String jwt)
        {
            Dictionary<String, String> datos = new Dictionary<String, String>
            {
                {"direccion", JsonSerializer.Serialize<Direccion>(direccion) },
                {"cliente", JsonSerializer.Serialize<Cliente>(cliente) },
                {"jwt", jwt }
            };

            HttpResponseMessage respuesta = await this.clienteHttp.PostAsJsonAsync<Dictionary<String, String>>("api/Cliente/EliminarDireccion", datos);
            return await respuesta.Content.ReadFromJsonAsync<RestMessage>();
        }


        #endregion


        #region zona Tienda

        /// <summary>
        /// Realiza una petición a la API_REST de Tienda para recuperar los productos de alguna categoria.
        /// </summary>
        /// <param name="categoria">Categoria de los productos</param>
        /// <returns>Una lista de productos</returns>
        public async Task<List<ProductoAPI>> RecuperarProductosCategoria(string categoria)
        {
            HttpResponseMessage respuesta = await this.clienteHttp.GetAsync($"https://fakestoreapi.com/products/category/{categoria}");
            String respuestaString = await respuesta.Content.ReadAsStringAsync();
            List<ProductoAPI> productos = JsonSerializer.Deserialize<List<ProductoAPI>>(respuestaString);
            return productos;
        }

        /// <summary>
        /// Realiza uan petición a la API de fakestoreapi para recuperar productos aleatorios.
        /// </summary>
        /// <returns>Una lista de productos</returns>
        public async Task<List<ProductoAPI>> RecuperarProductosAleatorios()
        {
            HttpResponseMessage respuesta = await this.clienteHttp.GetAsync("https://fakestoreapi.com/products");
            String respuestaString = await respuesta.Content.ReadAsStringAsync();
            List<ProductoAPI> productos = JsonSerializer.Deserialize<List<ProductoAPI>>(respuestaString);
            List<ProductoAPI> productosAleatorios = Shuffle<ProductoAPI>(productos);
            return productosAleatorios;
        }

        /// <summary>
        /// Realiza una petición a la API_REST de Tienda para recuperar un producto de la base de datos.
        /// </summary>
        /// <param name="idProducto"></param>
        /// <returns>Un producto</returns>
        public async Task<ProductoAPI> RecuperarProducto(string idProducto)
        {
            HttpResponseMessage respuesta = await this.clienteHttp.GetAsync($"https://fakestoreapi.com/products/{idProducto}");
            String contenidoString = await respuesta.Content.ReadAsStringAsync();
            ProductoAPI producto = JsonSerializer.Deserialize<ProductoAPI>(contenidoString);
            return producto;
        }

        /// <summary>
        /// Realiza una petición a la API_REST de Tienda para almacenar un pedido de la base de datos.
        /// </summary>
        /// <param name="pedido">Pedido a almacenar</param>
        /// <param name="cliente">Cliente que ha realizado el pedido</param>
        /// <param name="jwt">JWT del cliente</param>
        /// <returns>La respuesta del servidor en forma de RestMessage</returns>
        public async Task<RestMessage> AlmacenarPedido(Pedido pedido, Cliente cliente, String jwt)
        {
            Dictionary<String, String> datos = new Dictionary<String, String>
            {
                {"pedido", JsonSerializer.Serialize<Pedido>(pedido) },
                {"cliente", JsonSerializer.Serialize<Cliente>(cliente) },
                {"jwt", jwt }
            };

            HttpResponseMessage respuesta = await this.clienteHttp.PostAsJsonAsync<Dictionary<String, String>>("api/Tienda/AlmacenarPedido", datos);
            return await respuesta.Content.ReadFromJsonAsync<RestMessage>();
        }

        /// <summary>
        /// Realiza una petición a la API_REST de Tienda para realizar el pago con tarjeta.
        /// </summary>
        /// <param name="cliente">Cliente que realiza el pedido</param>
        /// <param name="datosPago">Objeto con los datos del pago y cliente</param>
        /// <param name="jwt">JWT del cliente</param>
        /// <returns>La respuesta del servidor en forma de RestMessage</returns>
        public async Task<RestMessage> RealizarPagoTarjeta(Cliente cliente, DatosPago datosPago, String jwt)
        {
            Dictionary<String, String> datos = new Dictionary<String, String>
            {
                {"cliente", JsonSerializer.Serialize<Cliente>(cliente) },
                {"datosPago", JsonSerializer.Serialize<DatosPago>(datosPago) },
                {"jwt", jwt }
            };

            HttpResponseMessage respuesta = await this.clienteHttp.PostAsJsonAsync<Dictionary<String, String>>("api/Tienda/RealizarPagoTarjeta", datos);
            return await respuesta.Content.ReadFromJsonAsync<RestMessage>();
        }

        /// <summary>
        /// Realiza una petición a la API_REST de Tienda para desear un producto.
        /// </summary>
        /// <param name="clienteActual">Cliente que ha deseado un producto</param>
        /// <param name="producto">Producto deseado</param>
        /// <param name="jwt">JWT del cliente</param>
        /// <returns>La respuesta del servidor en forma de RestMessage</returns>
        public async Task<RestMessage> DesearProducto(Cliente clienteActual, ProductoAPI producto, string jwt)
        {
            Dictionary<String, String> datos = new Dictionary<String, String>
            {
                {"idCliente", clienteActual.IdCliente },
                {"idProducto",  producto.id.ToString()},
                {"jwt", jwt }
            };

            HttpResponseMessage respuesta = await this.clienteHttp.PostAsJsonAsync<Dictionary<String, String>>("api/Tienda/DesearProducto", datos);
            return await respuesta.Content.ReadFromJsonAsync<RestMessage>();
        }

        /// <summary>
        /// Realiza una petición a la API_REST de Tienda para dejar de desear un producto.
        /// </summary>
        /// <param name="clienteActual">Cliente que ha dejado de desear el producto</param>
        /// <param name="producto">Producto dejado de desear</param>
        /// <param name="jwt">JWT del cliente</param>
        /// <returns>La respuesta del servidor en forma de RestMessage</returns>
        public async Task<RestMessage> DesDesearProd(Cliente clienteActual, ProductoAPI producto, string jwt)
        {
            Dictionary<String, String> datos = new Dictionary<String, String>
            {
                {"idCliente", clienteActual.IdCliente },
                {"idProducto",  producto.id.ToString()},
                {"jwt", jwt }
            };

            HttpResponseMessage respuesta = await this.clienteHttp.PostAsJsonAsync<Dictionary<String, String>>("api/Tienda/DesDesearProducto", datos);
            return await respuesta.Content.ReadFromJsonAsync<RestMessage>();
        }

        /// <summary>
        /// Realiza una petición a la API_REST de Tienda para almacenar un comentario en un producto
        /// </summary>
        /// <param name="jwt">JWT del cliente</param>
        /// <param name="comentario">Comentario del producto</param>
        /// <param name="idCliente">Id del cliente que ha realizado el comentario</param>
        /// <param name="nombreCliente">Nombre del cliente que ha realizado el comentario</param>
        /// <param name="idProducto">Id del producto</param>
        /// <returns>La respuesta del servidor en forma de RestMessage</returns>
        public async Task<RestMessage> AlmacenarComentario(string jwt, string comentario, string idCliente, string nombreCliente, string idProducto)
        {
            Dictionary<String, String> datos = new Dictionary<String, String>
            {
                {"jwt", jwt },
                {"comentario", comentario},
                {"idCliente", idCliente},
                {"nombreCliente", nombreCliente },
                {"idProducto", idProducto }
            };

            HttpResponseMessage respuesta = await this.clienteHttp.PostAsJsonAsync<Dictionary<String, String>>("api/Tienda/AlmacenarComentario", datos);
            return await respuesta.Content.ReadFromJsonAsync<RestMessage>();
        }

        /// <summary>
        /// Realiza una peticion a la API_REST de Tienda para recuperar los comentarios de los productos
        /// </summary>
        /// <param name="producto">Producto para obtener los comentarios</param>
        /// <returns>La respuesta del servidor en forma de lista de conjuntos de clave-valor</returns>
        public async Task<List<ComentarioCli>> RecuperarComentariosProd(ProductoAPI producto)
        {
            Dictionary<String, String> datos = new Dictionary<String, String>
            {
                {"idProducto", producto.id.ToString() },
            };
            HttpResponseMessage respuesta = await this.clienteHttp.PostAsJsonAsync<Dictionary<String, String>>("api/Tienda/RecuperarComentariosProducto", datos);
            return await respuesta.Content.ReadFromJsonAsync<List<ComentarioCli>>();
        }


        #endregion

        #region metodos especiales

        public static List<T> Shuffle<T>(List<T> lista)
        {
            Random random = new Random();

            // Realizar la permutación aleatoria utilizando el algoritmo de Fisher-Yates
            for (int i = lista.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T temp = lista[i];
                lista[i] = lista[j];
                lista[j] = temp;
            }

            return lista;
        }


        #endregion

    }
}

