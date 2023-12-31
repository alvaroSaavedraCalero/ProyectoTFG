﻿using MongoDB.Bson;
using MongoDB.Driver;
using ProyectoTFG.Server.Models.Interfaces;
using ProyectoTFG.Shared;
using System.Text.Json;

namespace ProyectoTFG.Server.Models
{
    public class MongoDBService : IAccesoDatos
    {
        #region propiedades de la clase

        private IConfiguration accesoAppSettings;
        private MongoClient clienteMongo;
        private IMongoDatabase bdCollection;

        public MongoDBService(IConfiguration accesoAppSettings)
        {
            this.accesoAppSettings = accesoAppSettings;
            this.clienteMongo = new MongoClient(this.accesoAppSettings["MongoDB:ConnectionString"]);
            this.bdCollection = this.clienteMongo.GetDatabase(this.accesoAppSettings["MongoDB:Database"]);
        }

        #endregion


        #region metodos de la clase

        #region metodos Cliente

        /// <summary>
        /// Comprueba si existe un Cliente en la coleccion "clientes"
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>El cliente encontrado en la base de datos, null en caso contrario</returns>
        public async Task<Cliente> ComprobarCredenciales(String email, String password)
        {
            try
            {
                Cliente cliente = new Cliente();
                // recuperamos de la coleccion cuentas, el documento asociado al email
                FilterDefinition<BsonDocument> filtro = Builders<BsonDocument>.Filter.Eq("cuenta.email", email);
                BsonDocument clienteBson = this.bdCollection.GetCollection<BsonDocument>("clientes").Find(filtro).FirstOrDefaultAsync().Result;

                String hashedPassword = "";

                if (clienteBson != null)
                {
                    cliente.IdCliente = clienteBson.GetElement("_id").Value.ToString();
                    cliente.Nombre = clienteBson.GetElement("nombre").Value.ToString();
                    cliente.Apellidos = clienteBson.GetElement("apellidos").Value.ToString();
                    cliente.Telefono = clienteBson.GetElement("telefono").Value.ToString();
                    cliente.FechaNacimiento = System.Convert.ToDateTime(clienteBson.GetElement("fechaNacimiento").Value);
                    cliente.Genero = clienteBson.GetElement("genero").Value.ToString();
                    cliente.NIF = clienteBson.GetElement("nif").Value.ToString();
                    cliente.Rol = clienteBson.GetElement("rol").Value.ToString();
                    cliente.cuenta = new Cuenta
                    {
                        Email = clienteBson.GetElement("cuenta").Value.ToBsonDocument().GetElement("email").Value.ToString(),
                        Login = clienteBson.GetElement("cuenta").Value.ToBsonDocument().GetElement("login").Value.ToString(),
                        CuentaActivada = System.Convert.ToBoolean(clienteBson.GetElement("cuenta").Value.ToBsonDocument().GetElement("cuentaActiva").Value),
                        ImagenAvatarBASE64 = clienteBson.GetElement("cuenta").Value.ToBsonDocument().GetElement("imagenAvatarBASE64").Value.ToString()
                    };

                    hashedPassword = clienteBson.GetElement("cuenta").Value.ToBsonDocument().GetElement("password").Value.ToString();

                    List<int> listaProductosDeseados = clienteBson.GetElement("listaDeseos").Value.AsBsonArray.Select(s => s.ToInt32()).ToList() ?? new List<int>();
                    if (listaProductosDeseados.Count > 0)
                    {
                        HttpClient clienteHttp = new HttpClient();
                        foreach (int i in listaProductosDeseados)
                        {
                            HttpResponseMessage respuesta = await clienteHttp.GetAsync($"https://fakestoreapi.com/products/{i}");
                            String respuestaString = await respuesta.Content.ReadAsStringAsync();
                            ProductoAPI productoApi = JsonSerializer.Deserialize<ProductoAPI>(respuestaString);
                            cliente.ListaDeseos.Add(productoApi);
                        }
                    }


                    // comprobamos si el hash de la cuenta recuperada coincide con el hash de la password que me pasan
                    if (BCrypt.Net.BCrypt.Verify(password, hashedPassword))
                    {
                        // recuperamos documento cliente de coleccion "clientes"
                        /*
                        Para poder mapear el documento cliente recupera de la coleccion "clientes" contra un 
                        objeto de la clase Cliente.cs, la propiedad "cuenta" debe ser un objeto Cuenta.cs, y 
                        en el doc.cliente es un ObjectId... ¿solucion? hay que hacer un JOIN de las colecciones
                        "clientes" y "cuentas" usando el ObjectId de cuenta:


                        en Mngodb se llama AGGREGATE-PIPELINE al conjunto de operaciones que se hacen en cascada sobre una query en uan coleccion:

                            db.clientes.aggregate(
                                {$operador: {parametros}}, // operacion1 o stage-1 --> filtrado por  el documento cliente que intento recuperar
                                {$operador: {parametros}}, // operacion2 o stage-3, actua sobre el resultado de la transformacion de los docs 
                                                            del stage1 --> join de coleccione clientes y cuentas
                                {$operador: {parametros}}, // op3 o stage-3, """ stage1,2 -->UNWIND el array tipo cuenta resultante del join convertirlo a un unico objeto
                                ...
                            )

                            db.clientes.aggregate( 
                                { $match: {nombre:'PABLO' } },
                                { $lookup: { from:'cuentas', foreignField:'_id', localField:'cuenta', as:'cuenta'} },
                                { $unwind: '$cuenta' }
                                ) 

                         */
                        // metodo 1, usando BsonDocument para generar los stages del agregate direcctamente
                        // como si fuera mongodb


                        // Recupero las direcciones asociadas
                        List<String> listaIdsDirecciones = clienteBson.GetElement("direcciones").Value.AsBsonArray.Select(s => s.AsString).ToList() ?? new List<String>();

                        if (listaIdsDirecciones != null || listaIdsDirecciones.Count != 0)
                        {
                            List<Direccion> listaDirecciones = new List<Direccion>();

                            foreach (String s in listaIdsDirecciones)
                            {
                                Direccion dir = this.bdCollection.GetCollection<Direccion>("direcciones").AsQueryable<Direccion>()
                                    .Where((Direccion d) => d.IdDireccion == s).Single<Direccion>();
                                if (dir != null)
                                {
                                    listaDirecciones.Add(dir);
                                }
                            }

                            // Y las añadimos a las direcciones del cliente
                            cliente.MisDirecciones = listaDirecciones;
                        }

                        // Recuperamos los pedidos asociados
                        List<String> listaIdsPedidos = clienteBson.GetElement("pedidos").Value.AsBsonArray.Select(s => s.AsString).ToList() ?? new List<String>();

                        if (listaIdsPedidos != null || listaIdsPedidos.Count != 0)
                        {
                            List<Pedido> listaPedidos = new List<Pedido>();
                            foreach (String s in listaIdsPedidos)
                            {
                                Pedido ped = this.bdCollection.GetCollection<Pedido>("pedidos").AsQueryable<Pedido>()
                                    .Where((Pedido p) => p.IdPedido == s).Single<Pedido>();
                                if (ped != null)
                                {
                                    listaPedidos.Add(ped);
                                }
                            }
                            cliente.MisPedidos = listaPedidos;
                        }

                        return cliente;
                    }
                    else { throw new Exception("password invalida"); }


                }
                else { return null; }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Recupera un cliente en base a su id
        /// </summary>
        /// <param name="id">Id del cliente</param>
        /// <returns>El cliente recuperado, null en caso de queno lo encuentre</returns>
        public async Task<Cliente> ObtenerClienteId(String id)
        {
            FilterDefinition<BsonDocument> filtro = Builders<BsonDocument>.Filter.Eq("_id", BsonObjectId.Parse(id));
            BsonDocument clienteBson = this.bdCollection.GetCollection<BsonDocument>("clientes").Find(filtro).FirstOrDefaultAsync().Result;
            Cliente cliente = new Cliente();

            if (clienteBson != null)
            {
                cliente.IdCliente = clienteBson.GetElement("_id").Value.ToString();
                cliente.Nombre = clienteBson.GetElement("nombre").Value.ToString();
                cliente.Apellidos = clienteBson.GetElement("apellidos").Value.ToString();
                cliente.Telefono = clienteBson.GetElement("telefono").Value.ToString();
                cliente.FechaNacimiento = System.Convert.ToDateTime(clienteBson.GetElement("fechaNacimiento").Value);
                cliente.Genero = clienteBson.GetElement("genero").Value.ToString();
                cliente.NIF = clienteBson.GetElement("nif").Value.ToString();
                cliente.cuenta = new Cuenta
                {
                    Email = clienteBson.GetElement("cuenta").Value.ToBsonDocument().GetElement("email").Value.ToString(),
                    Login = clienteBson.GetElement("cuenta").Value.ToBsonDocument().GetElement("login").Value.ToString(),
                    Password = clienteBson.GetElement("cuenta").Value.ToBsonDocument().GetElement("password").Value.ToString(),
                    CuentaActivada = System.Convert.ToBoolean(clienteBson.GetElement("cuenta").Value.ToBsonDocument().GetElement("cuentaActiva").Value),
                    ImagenAvatarBASE64 = clienteBson.GetElement("cuenta").Value.ToBsonDocument().GetElement("imagenAvatarBASE64").Value.ToString()
                };

                List<int> listaProductosDeseados = clienteBson.GetElement("listaDeseos").Value.AsBsonArray.Select(s => s.ToInt32()).ToList() ?? new List<int>();
                if (listaProductosDeseados.Count > 0)
                {
                    HttpClient clienteHttp = new HttpClient();
                    foreach (int i in listaProductosDeseados)
                    {
                        HttpResponseMessage respuesta = await clienteHttp.GetAsync($"https://fakestoreapi.com/products/{i}");
                        String respuestaString = await respuesta.Content.ReadAsStringAsync();
                        ProductoAPI productoApi = JsonSerializer.Deserialize<ProductoAPI>(respuestaString);
                        cliente.ListaDeseos.Add(productoApi);
                    }
                }

                // Recupero las direcciones asociadas
                List<String> listaIdsDirecciones = clienteBson.GetElement("direcciones").Value.AsBsonArray.Select(s => s.AsString).ToList() ?? new List<String>();

                if (listaIdsDirecciones != null || listaIdsDirecciones.Count != 0)
                {
                    List<Direccion> listaDirecciones = new List<Direccion>();

                    foreach (String s in listaIdsDirecciones)
                    {
                        Direccion dir = this.bdCollection.GetCollection<Direccion>("direcciones").AsQueryable<Direccion>()
                            .Where((Direccion d) => d.IdDireccion == s).Single<Direccion>();
                        if (dir != null)
                        {
                            listaDirecciones.Add(dir);
                        }
                    }

                    // Y las añadimos a las direcciones del cliente
                    cliente.MisDirecciones = listaDirecciones;
                }

                // Recuperamos los pedidos asociados
                List<String> listaIdsPedidos = clienteBson.GetElement("pedidos").Value.AsBsonArray.Select(s => s.AsString).ToList() ?? new List<String>();

                if (listaIdsPedidos != null || listaIdsPedidos.Count != 0)
                {
                    List<Pedido> listaPedidos = new List<Pedido>();
                    foreach (String s in listaIdsPedidos)
                    {
                        Pedido ped = this.bdCollection.GetCollection<Pedido>("pedidos").AsQueryable<Pedido>()
                            .Where((Pedido p) => p.IdPedido == s).Single<Pedido>();
                        if (ped != null)
                        {
                            listaPedidos.Add(ped);
                        }
                    }
                    cliente.MisPedidos = listaPedidos;
                }

                return cliente;
            }
            else { return null; }
        }

        /// <summary>
        /// Obtiene un cliente de la base de datos en base al id de google
        /// </summary>
        /// <param name="idGoogle">Id de google</param>
        /// <returns>El cliente recuperado, null en caso contrario</returns>
        public async Task<Cliente> ObtenerClienteIdGoogle(string idGoogle)
        {
            try
            {
                FilterDefinition<BsonDocument> filtro = Builders<BsonDocument>.Filter.Eq("idGoogle", idGoogle);
                BsonDocument clienteBson = this.bdCollection.GetCollection<BsonDocument>("googleSesions").Find(filtro).FirstOrDefaultAsync().Result;

                if (clienteBson != null)
                {
                    String idCliente = clienteBson.GetElement("idCliente").Value.ToString();
                    Cliente cliente = await this.ObtenerClienteId(idCliente);

                    if (cliente != null)
                    {
                        return cliente;
                    }
                    else { return null; }
                }
                else { return null; }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Guardado de credenciales de google
        /// </summary>
        /// <param name="idGoogle">El id del user de google</param>
        /// <param name="idCliente">Id del cliente</param>
        /// <returns>True en caso de que el guardado sea satisfactorio, false en caso contrario</returns>
        public async Task<bool> GuardarCredencialesGoogle(String idGoogle, String idCliente)
        {
            try
            {
                BsonDocument docBson = new BsonDocument
                {
                    {"idGoogle", idGoogle},
                    {"idCliente", idCliente }
                };

                await this.bdCollection.GetCollection<BsonDocument>("googleSesions").InsertOneAsync(docBson);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Registra un cliente que se ha logueado a traves de google
        /// </summary>
        /// <param name="cliente">El cliente de google</param>
        /// <returns>El cliente introducido en la base de datos</returns>
        public async Task<Cliente> RegistrarClienteGoogle(Cliente cliente)
        {
            try
            {
                Boolean insertCli = await RegistrarCliente(cliente);
                if (insertCli)
                {
                    Cliente clienteAlmac = this.bdCollection.GetCollection<Cliente>("clientes").AsQueryable<Cliente>()
                    .Where((Cliente c) => c.cuenta.Email == cliente.cuenta.Email && c.cuenta.CuentaActivada == true).Single<Cliente>();

                    if (clienteAlmac != null) { return clienteAlmac; } else { return null; }
                }
                else { return null; }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Activa la cuenta del cliente
        /// </summary>
        /// <param name="idCliente">id del cliente</param>
        /// <returns>True en caso de que no haya ningun error, false en caso contrario</returns>
        public async Task<bool> ActivarCuenta(string idCliente)
        {
            try
            {
                UpdateDefinition<Cliente> update = Builders<Cliente>.Update.Set("cuenta.cuentaActiva", true);
                UpdateResult resultadoUpdate = await this.bdCollection.GetCollection<Cliente>("clientes")
                    .UpdateOneAsync<Cliente>(c => c.IdCliente == idCliente, update);

                if (resultadoUpdate.ModifiedCount > 0)
                {
                    return true;
                }
                else { return false; }

            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Introduce un Cliente en la coleccion "clientes", con su respectiva Cuenta
        /// </summary>
        /// <param name="cliente"></param>
        /// <returns>True en caso de que el registro haya funcionado</returns>
        public async Task<bool> RegistrarCliente(Cliente cliente)
        {
            try
            {
                Cliente registro = new Cliente
                {
                    Nombre = cliente.Nombre,
                    Apellidos = cliente.Apellidos,
                    Telefono = cliente.Telefono,
                    FechaNacimiento = cliente.FechaNacimiento,
                    Genero = cliente.Genero,
                    NIF = cliente.NIF,
                    Rol = "cliente",
                    cuenta = new Cuenta
                    {
                        Login = cliente.cuenta.Login,
                        Email = cliente.cuenta.Email,
                        Password = BCrypt.Net.BCrypt.HashPassword(cliente.cuenta.Password),
                        CuentaActivada = cliente.cuenta.CuentaActivada,
                        ImagenAvatarBASE64 = cliente.cuenta.ImagenAvatarBASE64
                    },
                    MisDirecciones = cliente.MisDirecciones,
                    MisPedidos = cliente.MisPedidos,
                    ListaDeseos = cliente.ListaDeseos,
                    PedidoActual = cliente.PedidoActual
                };

                await this.bdCollection.GetCollection<Cliente>("clientes").InsertOneAsync(registro);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Modifica el cliente de la coleccion "clientes", junto a su respectiva cuenta
        /// </summary>
        /// <param name="cliente"></param>
        /// <returns>El cliente modificado, null en caso contrario</returns>
        public async Task<Cliente> ModificarCliente(Cliente cliente, Boolean cambioPass)
        {
            try
            {
                // Recuperamos la cuenta de la base de datos
                Cliente clienteRecup = this.bdCollection.GetCollection<Cliente>("clientes").AsQueryable<Cliente>()
                    .Where((Cliente c) => c.IdCliente == cliente.IdCliente).Single<Cliente>();

                if (cambioPass)
                {
                    String hash = BCrypt.Net.BCrypt.HashPassword(cliente.cuenta.Password);
                    cliente.cuenta.Password = hash;

                    UpdateDefinition<Cliente> updatePass = Builders<Cliente>.Update
                        .Set(c => c.cuenta.Password, hash);

                    UpdateResult resultadoPass = await this.bdCollection.GetCollection<Cliente>("clientes")
                    .UpdateOneAsync<Cliente>(c => c.IdCliente == clienteRecup.IdCliente, updatePass);
                }

                // En caso de que no coincida con la que tiene el cliente

                Boolean mismoLogin = cliente.cuenta.Login == clienteRecup.cuenta.Login;
                Boolean mismoEmail = cliente.cuenta.Email == clienteRecup.cuenta.Email;
                Boolean mismoCuentaAcvt = cliente.cuenta.CuentaActivada == clienteRecup.cuenta.CuentaActivada;

                // En caso de que se haya modificado un campo de la cuenta
                if ((!mismoLogin || !mismoEmail || !mismoCuentaAcvt))
                {
                    UpdateDefinition<Cliente> updateCuenta = Builders<Cliente>.Update
                    .Set(c => c.cuenta.Login, cliente.cuenta.Login)
                    .Set(c => c.cuenta.Email, cliente.cuenta.Email)
                    .Set(c => c.cuenta.ImagenAvatarBASE64, cliente.cuenta.ImagenAvatarBASE64);


                    UpdateResult resultadoCuenta = await this.bdCollection.GetCollection<Cliente>("clientes")
                        .UpdateOneAsync<Cliente>(c => c.IdCliente == clienteRecup.IdCliente, updateCuenta);

                    // Si se ha modificado bien, modificamos la contrasela si lo quiere el cliente
                    if (resultadoCuenta.ModifiedCount > 0)
                    {
                        // Modificamos el cliente
                        UpdateDefinition<Cliente> updateCliente = Builders<Cliente>.Update
                                .Set(c => c.Nombre, cliente.Nombre)
                                .Set(c => c.Apellidos, cliente.Apellidos)
                                .Set(c => c.Telefono, cliente.Telefono)
                                .Set(c => c.FechaNacimiento, cliente.FechaNacimiento)
                                .Set(c => c.Genero, cliente.Genero)
                                .Set(c => c.NIF, cliente.NIF);

                        UpdateResult resultadoCliente = await this.bdCollection.GetCollection<Cliente>("clientes")
                            .UpdateOneAsync<Cliente>(c => c.IdCliente == cliente.IdCliente, updateCliente);

                        return await ObtenerClienteId(cliente.IdCliente);
                    }
                    else { return null; }

                }
                else
                {
                    // Modificamos el cliente
                    UpdateDefinition<Cliente> updateCliente = Builders<Cliente>.Update
                            .Set(c => c.Nombre, cliente.Nombre)
                            .Set(c => c.Apellidos, cliente.Apellidos)
                            .Set(c => c.Telefono, cliente.Telefono)
                            .Set(c => c.FechaNacimiento, cliente.FechaNacimiento)
                            .Set(c => c.Genero, cliente.Genero)
                            .Set(c => c.NIF, cliente.NIF);

                    UpdateResult resultadoCliente = await this.bdCollection.GetCollection<Cliente>("clientes")
                        .UpdateOneAsync<Cliente>(c => c.IdCliente == cliente.IdCliente, updateCliente);

                    if (resultadoCliente.ModifiedCount > 0)
                    {
                        // el cliente tambien se modifico correctamente, asi que retornamos el cliente modificado
                        return cliente;
                    }
                    else { return null; }
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Modifica la imagen del cliente, de la coleccion "clientes"
        /// </summary>
        /// <param name="cliente">Cliente para modificar</param>
        /// <param name="imagen">Imagen nueva</param>
        /// <returns>True en caso de que la operacion salga correctamente, false en caso contrario</returns>
        public async Task<bool> ModificarImagen(Cliente cliente, string imagen)
        {
            try
            {
                UpdateDefinition<Cliente> update = Builders<Cliente>.Update.Set(c => c.cuenta.ImagenAvatarBASE64, imagen);
                UpdateResult resultadoUpdate = await this.bdCollection.GetCollection<Cliente>("clientes")
                    .UpdateOneAsync<Cliente>(c => c.IdCliente == cliente.IdCliente, update);
                if (resultadoUpdate.ModifiedCount > 0)
                {
                    // Modificamos la imagen de los comentarios con este idCliente
                    UpdateDefinition<ComentarioCli> updateComentario = Builders<ComentarioCli>.Update.Set(cmc => cmc.ImagenCliente, imagen);
                    UpdateResult resultadoUpdateComentario = await this.bdCollection.GetCollection<ComentarioCli>("comentarios")
                        .UpdateManyAsync<ComentarioCli>(cmc => cmc.IdCliente == cliente.IdCliente, updateComentario);

                    return true;
                }
                else { return false; }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Introduce una Direccion en la coleccion "direcciones", tambien su id en el array del cliente
        /// </summary>
        /// <param name="direccion"></param>
        /// <param name="idCliente"></param>
        /// <returns>True en caso de que se haya registrado correctamente</returns>
        public async Task<bool> RegistrarDireccion(Direccion direccion, String idCliente)
        {
            try
            {
                // Añadimos la direccion a la coleccion y obtenemos su _id
                await this.bdCollection.GetCollection<Direccion>("direcciones").InsertOneAsync(direccion);

                String idDireccion = this.bdCollection.GetCollection<Direccion>("direcciones").AsQueryable<Direccion>()
                    .Where((Direccion d) => d.NombreContacto == direccion.NombreContacto && d.ApellidosContacto == direccion.ApellidosContacto &&
                    d.NombreEmpresa == direccion.NombreEmpresa && d.TelefonoContacto == direccion.TelefonoContacto &&
                    d.Calle == direccion.Calle && d.Numero == direccion.Numero && d.CP == direccion.CP &&
                    d.ProvDirec.Equals(direccion.ProvDirec) && d.MuniDirecc.Equals(direccion.MuniDirecc) &&
                    d.Pais == direccion.Pais && d.EsEnvio == direccion.EsEnvio && d.EsFaturacion == direccion.EsFaturacion)
                    .Select((Direccion dire) => dire.IdDireccion).Single<String>();

                // Guardamos el _id de la direccion en las direcciones del cliente
                FilterDefinition<Cliente> filtro = Builders<Cliente>.Filter.Eq((Cliente c) => c.IdCliente, idCliente);
                UpdateDefinition<Cliente> updateCliente = Builders<Cliente>.Update.AddToSet("direcciones", direccion.IdDireccion);
                UpdateResult resultadoOperacion = await this.bdCollection.GetCollection<Cliente>("clientes").UpdateOneAsync(filtro, updateCliente);

                if (resultadoOperacion.ModifiedCount > 0)
                {
                    return true;
                }
                else { return false; }

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Modifica la direccion enviada del cliente
        /// </summary>
        /// <param name="direcccion"></param>
        /// <returns>retorna la direccion modificada o null en caso de que haya algun fallo</returns>
        public async Task<Direccion> ModificarDireccion(Direccion direcccion, String idCliente)
        {
            try
            {
                FilterDefinition<Direccion> filtro = Builders<Direccion>.Filter.Eq((Direccion d) => d.IdDireccion, direcccion.IdDireccion);
                UpdateDefinition<Direccion> updateDireccion = Builders<Direccion>.Update
                    .Set(direc => direc.NombreEmpresa, direcccion.NombreEmpresa)
                    .Set(direc => direc.NombreContacto, direcccion.NombreContacto)
                    .Set(direc => direc.ApellidosContacto, direcccion.ApellidosContacto)
                    .Set(direc => direc.TelefonoContacto, direcccion.TelefonoContacto)
                    .Set(direc => direc.Calle, direcccion.Calle)
                    .Set(direc => direc.Numero, direcccion.Numero)
                    .Set(direc => direc.CP, direcccion.CP)
                    .Set(direc => direc.ProvDirec, direcccion.ProvDirec)
                    .Set(direc => direc.MuniDirecc, direcccion.MuniDirecc)
                    .Set(direc => direc.Pais, direcccion.Pais)
                    .Set(direc => direc.EsEnvio, direcccion.EsEnvio)
                    .Set(direc => direc.EsFaturacion, direcccion.EsFaturacion);
                UpdateResult resultadoOperacion = await this.bdCollection.GetCollection<Direccion>("direcciones").UpdateOneAsync(filtro, updateDireccion);


                if (resultadoOperacion.ModifiedCount > 0)
                {
                    return direcccion;
                }
                else { return null; }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Elimina la direccion que manda el cliente
        /// </summary>
        /// <param name="direcccion"></param>
        /// <returns>Retorna true en caso de que la eliminacion haya sido sartisfractoria</returns>
        public async Task<Boolean> EliminarDireccion(Direccion direcccion, String idCliente)
        {
            try
            {
                FilterDefinition<String> filtro = Builders<String>.Filter.Eq(str => str, direcccion.IdDireccion);
                UpdateDefinition<Cliente> update = Builders<Cliente>.Update.Pull("direcciones", direcccion.IdDireccion);

                UpdateResult resultado = await this.bdCollection.GetCollection<Cliente>("clientes")
                    .UpdateOneAsync<Cliente>(c => c.IdCliente == idCliente, update);

                if (resultado.ModifiedCount > 0)
                {
                    DeleteResult resultadoDelete = await this.bdCollection.GetCollection<Direccion>("direcciones")
                        .DeleteOneAsync<Direccion>((Direccion d) => d.IdDireccion == direcccion.IdDireccion);

                    if (resultadoDelete.DeletedCount > 0)
                    {
                        return true;
                    }
                    else { return false; }
                }
                else { return false; }

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion


        #region metodos Tienda

        /// <summary>
        /// Sube a la base de datos el pedido con sus respectivos ItemsPedido
        /// </summary>
        /// <param name="pedido"></param>
        /// <param name="cliente"></param>
        /// <returns>True en caso de que haya salido todo bien</returns>
        public async Task<Pedido> RegistrarPedido(Pedido pedido, Cliente cliente)
        {
            try
            {
                // preparamos el pedido
                pedido.IdCliente = cliente.IdCliente;

                await this.bdCollection.GetCollection<Pedido>("pedidos").InsertOneAsync(pedido);

                pedido.IdPedido = this.bdCollection.GetCollection<Pedido>("pedidos").AsQueryable<Pedido>().
                    Where((Pedido p) => p.IdCliente == cliente.IdCliente && p.ItemsPedido == pedido.ItemsPedido).Single<Pedido>().IdPedido;

                // Teniendo el Id del pedido, lo guardamos en los pedidos del cliente
                FilterDefinition<Cliente> filtro = Builders<Cliente>.Filter.Eq((Cliente c) => c.IdCliente, cliente.IdCliente);
                UpdateDefinition<Cliente> update = Builders<Cliente>.Update.AddToSet("pedidos", pedido.IdPedido);
                UpdateResult resultadoOperacion = await this.bdCollection.GetCollection<Cliente>("clientes").UpdateOneAsync(filtro, update);

                if (resultadoOperacion.ModifiedCount > 0)
                {
                    return pedido;
                }
                else { return null; }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Agrega un producto a la lista de deseos de un cliente
        /// </summary>
        /// <param name="idProducto">El id del prodcuto a añadir</param>
        /// <param name="idCliente">El id del cliente para su lista de deseos</param>
        /// <returns>True en caso de que el producto se haya añadido correctamente, False en caso contrario</returns>
        public async Task<Boolean> DesearProducto(String idProducto, String idCliente)
        {
            try
            {
                FilterDefinition<Cliente> filtro = Builders<Cliente>.Filter.Eq((Cliente c) => c.IdCliente, idCliente);
                UpdateDefinition<Cliente> update = Builders<Cliente>.Update.AddToSet("listaDeseos", idProducto);
                UpdateResult resultado = await this.bdCollection.GetCollection<Cliente>("clientes").UpdateOneAsync(filtro, update);

                if (resultado.ModifiedCount == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Elimina un producto de la lista de deseos de un cliente
        /// </summary>
        /// <param name="idProducto">Id del prodcuto a eliminar</param>
        /// <param name="idCliente">Id del cliente</param>
        /// <returns>True en caso de que la eliminacion haya sido satisfactoria, False en caso contrario</returns>
        public async Task<Boolean> DesDesearProducto(String idProducto, String idCliente)
        {
            try
            {
                FilterDefinition<Cliente> filtro = Builders<Cliente>.Filter.Eq((Cliente c) => c.IdCliente, idCliente);
                UpdateDefinition<Cliente> update = Builders<Cliente>.Update.Pull("listaDeseos", idProducto);
                UpdateResult resultado = await this.bdCollection.GetCollection<Cliente>("clientes").UpdateOneAsync(filtro, update);

                if (resultado.ModifiedCount > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Sube a la base de datos el comentario del cliente
        /// </summary>
        /// <param name="idCliente">Id del cliente</param>
        /// <param name="comentario">Comentario del cliente</param>
        /// <param name="nombreCliente">Nombre del cliente</param>
        /// <returns>True en caso de que el comentario de suba correctamente, False en caso contrario</returns>
        public async Task<Boolean> SubirComentario(String idCliente, String comentario, String nombreCliente, String idProducto, String imagenCliente)
        {
            try
            {
                await this.bdCollection.GetCollection<ComentarioCli>("comentarios").InsertOneAsync(
                    new ComentarioCli
                    {
                        Comentario = comentario,
                        IdCliente = idCliente,
                        NombreCliente = nombreCliente,
                        IdProducto = idProducto,
                        ImagenCliente = imagenCliente
                    });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Recupera los comentarios del producto en cuestion
        /// </summary>
        /// <param name="idProducto">El id del productos en cuestion</param>
        /// <returns>Una lista de comentarios</returns>
        public async Task<List<ComentarioCli>> RecuperarComentariosProducto(String idProducto)
        {
            try
            {
                FilterDefinition<ComentarioCli> filter = Builders<ComentarioCli>.Filter.Eq((ComentarioCli c) => c.IdProducto, idProducto);
                List<ComentarioCli> listaComentarios = await this.bdCollection.GetCollection<ComentarioCli>("comentarios").FindAsync(filter).Result.ToListAsync();
                return listaComentarios;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Sube a la base de datos los datos del pedido pagado con paypal
        /// </summary>
        /// <param name="datosPedido">Datos del pedido paypal</param>
        /// <returns>True en caso de que la subida de datos sea correcta, false en caso contrario</returns>
        public async Task<Boolean> IntroducirDatosPayPal(PaypalPedidoInfo datosPedido)
        {
            try
            {
                await this.bdCollection.GetCollection<PaypalPedidoInfo>("paypalTemp").InsertOneAsync(datosPedido);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Recupera los datos del pedido de paypal y los elimina de la base de datos
        /// </summary>
        /// <param name="idPedido">Id del pedido para localizarlos en la base de datos</param>
        /// <returns>La informacion en forma de PaypalPedidoInfo, null en caso contrario</returns>
        public async Task<PaypalPedidoInfo> RecuperarDatosPayPal(String idPedido)
        {
            try
            {
                FilterDefinition<PaypalPedidoInfo> filtro = Builders<PaypalPedidoInfo>.Filter.Eq((PaypalPedidoInfo p) => p.Pedido.IdPedido, idPedido);
                PaypalPedidoInfo recup = await this.bdCollection.GetCollection<PaypalPedidoInfo>("paypalTemp").Find(filtro).FirstOrDefaultAsync();
                if (recup == null) { return null; }
                else
                {
                    DeleteResult resultado = await this.bdCollection.GetCollection<PaypalPedidoInfo>("paypalTemp").DeleteOneAsync(filtro);
                    if (resultado.DeletedCount > 0) { return recup; } else { return null; }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Recupera un pedido de la base de datos a traves de su id
        /// </summary>
        /// <param name="idPedido">Id del pedido</param>
        /// <returns>El pedido recuperado, null en caso contrario</returns>
        public async Task<Pedido> ObtenerPedidoId(string idPedido)
        {
            try
            {
                FilterDefinition<Pedido> filtro = Builders<Pedido>.Filter.Eq((Pedido ped) => ped.IdPedido, idPedido);
                Pedido pedidoRecup = await this.bdCollection.GetCollection<Pedido>("pedidos").FindAsync(filtro).Result.SingleAsync();
                return pedidoRecup;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion


        #region metodos Administracion

        /// <summary>
        /// Recupera todos los clientes de la base de datos
        /// </summary>
        /// <returns>Una lista con los clientes recuperados</returns>
        public async Task<List<Cliente>> GetClientes()
        {
            List<Cliente> listaClientes = new List<Cliente>();
            FilterDefinition<BsonDocument> filtro = Builders<BsonDocument>.Filter.Empty;
            List<BsonDocument> listaClientesBson = this.bdCollection.GetCollection<BsonDocument>("clientes").Find(filtro).ToList();

            if (listaClientesBson != null || listaClientesBson.Count == 0)
            {
                foreach (BsonDocument clienteBson in listaClientesBson)
                {
                    Cliente cliente = new Cliente();

                    cliente.IdCliente = clienteBson.GetElement("_id").Value.ToString();
                    cliente.Nombre = clienteBson.GetElement("nombre").Value.ToString();
                    cliente.Apellidos = clienteBson.GetElement("apellidos").Value.ToString();
                    cliente.Telefono = clienteBson.GetElement("telefono").Value.ToString();
                    cliente.FechaNacimiento = System.Convert.ToDateTime(clienteBson.GetElement("fechaNacimiento").Value);
                    cliente.Genero = clienteBson.GetElement("genero").Value.ToString();
                    cliente.NIF = clienteBson.GetElement("nif").Value.ToString();
                    cliente.cuenta = new Cuenta
                    {
                        Email = clienteBson.GetElement("cuenta").Value.ToBsonDocument().GetElement("email").Value.ToString(),
                        Login = clienteBson.GetElement("cuenta").Value.ToBsonDocument().GetElement("login").Value.ToString(),
                        CuentaActivada = System.Convert.ToBoolean(clienteBson.GetElement("cuenta").Value.ToBsonDocument().GetElement("cuentaActiva").Value),
                        ImagenAvatarBASE64 = clienteBson.GetElement("cuenta").Value.ToBsonDocument().GetElement("imagenAvatarBASE64").Value.ToString(),
                        Password = clienteBson.GetElement("cuenta").Value.ToBsonDocument().GetElement("password").Value.ToString()
                    };

                    // Recupero la lista de deseos
                    List<int> listaProductosDeseados = clienteBson.GetElement("listaDeseos").Value.AsBsonArray.Select(s => s.ToInt32()).ToList() ?? new List<int>();
                    if (listaProductosDeseados.Count > 0)
                    {
                        HttpClient clienteHttp = new HttpClient();
                        foreach (int i in listaProductosDeseados)
                        {
                            HttpResponseMessage respuesta = await clienteHttp.GetAsync($"https://fakestoreapi.com/products/{i}");
                            String respuestaString = await respuesta.Content.ReadAsStringAsync();
                            ProductoAPI productoApi = JsonSerializer.Deserialize<ProductoAPI>(respuestaString);
                            cliente.ListaDeseos.Add(productoApi);
                        }
                    }

                    // Recupero las direcciones asociadas
                    List<String> listaIdsDirecciones = clienteBson.GetElement("direcciones").Value.AsBsonArray.Select(s => s.AsString).ToList() ?? new List<String>();

                    if (listaIdsDirecciones != null || listaIdsDirecciones.Count != 0)
                    {
                        List<Direccion> listaDirecciones = new List<Direccion>();

                        foreach (String s in listaIdsDirecciones)
                        {
                            Direccion dir = this.bdCollection.GetCollection<Direccion>("direcciones").AsQueryable<Direccion>()
                                .Where((Direccion d) => d.IdDireccion == s).Single<Direccion>();
                            if (dir != null)
                            {
                                listaDirecciones.Add(dir);
                            }
                        }

                        // Y las añadimos a las direcciones del cliente
                        cliente.MisDirecciones = listaDirecciones;
                    }

                    // Recuperamos los pedidos asociados
                    List<String> listaIdsPedidos = clienteBson.GetElement("pedidos").Value.AsBsonArray.Select(s => s.AsString).ToList() ?? new List<String>();

                    if (listaIdsPedidos != null || listaIdsPedidos.Count != 0)
                    {
                        List<Pedido> listaPedidos = new List<Pedido>();
                        foreach (String s in listaIdsPedidos)
                        {
                            Pedido ped = this.bdCollection.GetCollection<Pedido>("pedidos").AsQueryable<Pedido>()
                                .Where((Pedido p) => p.IdPedido == s).Single<Pedido>();
                            if (ped != null)
                            {
                                listaPedidos.Add(ped);
                            }
                        }
                        cliente.MisPedidos = listaPedidos;
                    }

                    listaClientes.Add(cliente);
                }

                return listaClientes;
            }
            else { return null; }

        }

        #endregion

        #endregion
    }
}
