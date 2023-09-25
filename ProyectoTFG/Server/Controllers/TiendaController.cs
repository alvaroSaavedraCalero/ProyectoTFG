using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using PayPal.Api;
using ProyectoTFG.Server.Models;
using ProyectoTFG.Server.Models.Interfaces;
using ProyectoTFG.Shared;
using Stripe;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace ProyectoTFG.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TiendaController : ControllerBase
    {
        #region atributos del controlador

        private IAccesoDatos accesoBD;
        private IConfiguration accesoAppSettings;

        public TiendaController(IAccesoDatos accesoBD, IConfiguration accesoAppSettings)
        {
            this.accesoBD = accesoBD;
            this.accesoAppSettings = accesoAppSettings;
        }

        #endregion

        #region metodos del controlador

        [HttpPost]
        public async Task<RestMessage> AlmacenarPedido([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                Pedido p = JsonSerializer.Deserialize<Pedido>(datos["pedido"]);
                Cliente c = JsonSerializer.Deserialize<Cliente>(datos["cliente"]);
                String jwt = datos["jwt"];
                if (validarJWT(jwt))
                {
                    Pedido pedidoUpdate = await this.accesoBD.RegistrarPedido(p, c);

                    if (pedidoUpdate != null)
                    {
                        c.PedidoActual = pedidoUpdate;
                        c.MisPedidos.Add(pedidoUpdate);
                        return new RestMessage
                        {
                            Codigo = 0,
                            Mensaje = "update pedido correcto",
                            Error = null,
                            DatosCliente = c,
                            TokenSesion = null,
                            OtrosDatos = pedidoUpdate
                        };
                    }
                    else
                    {
                        return new RestMessage
                        {
                            Codigo = 2,
                            Mensaje = "Almacenado de pedido fallido",
                            Error = null,
                            DatosCliente = null,
                            TokenSesion = null,
                            OtrosDatos = null
                        };
                    }
                }
                else
                {
                    return new RestMessage
                    {
                        Codigo = 5,
                        Mensaje = "Tiempo de expiracion excedido, vuelva a realizar login",
                        Error = null,
                        DatosCliente = null,
                        TokenSesion = null,
                        OtrosDatos = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new RestMessage
                {
                    Codigo = 1,
                    Mensaje = "Error al almacenar el pedido",
                    Error = ex.Message,
                    DatosCliente = null,
                    TokenSesion = null,
                    OtrosDatos = null
                };
            }
        }

        [HttpPost]
        public async Task<RestMessage> RealizarPagoTarjeta([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                Cliente cliente = JsonSerializer.Deserialize<Cliente>(datos["cliente"]);
                DatosPago datosPago = JsonSerializer.Deserialize<DatosPago>(datos["datosPago"]);
                String jwt = datos["jwt"];

                if (validarJWT(jwt))
                {
                    Direccion direccionStripe;

                    if (datosPago.DireccionEnvio == null && datosPago.DireccionPrincipal == null)
                    {
                        return new RestMessage
                        {
                            Codigo = 7,
                            DatosCliente = cliente,
                            Error = "No hay una direccion asignada",
                            Mensaje = "No hay una direccion asignada, vaya a su perfil y configure una direccion",
                            OtrosDatos = null,
                            TokenSesion = jwt
                        };
                    }
                    else
                    {
                        direccionStripe = datosPago.DireccionEnvio ?? datosPago.DireccionPrincipal;

                    }


                    StripeConfiguration.ApiKey = this.accesoAppSettings["Stripe:StripeAPIKey"];

                    // Buscamos si ya estaba la tarjeta 
                    CustomerSearchOptions customerSearchOptions = new CustomerSearchOptions
                    {
                        Query = $"name:'{cliente.Nombre}' AND email:'{cliente.cuenta.Email}' AND metadata['IdCliente']:'{cliente.IdCliente}'"
                    };

                    CustomerService customerService = new CustomerService();
                    StripeSearchResult<Customer> resultadoBusqueda = customerService.Search(customerSearchOptions);

                    if (resultadoBusqueda.Data.Count > 0)
                    {
                        // Que hacemos en caso de encontrar el customer
                        Customer customer = resultadoBusqueda.Data[0];

                        /**
                        // Asociamos el customer a una tarjeta de credito
                        TokenCreateOptions tokenCreateOptions = new TokenCreateOptions
                        {
                            Card = new TokenCardOptions
                            {
                                Number = "4242424242424242", // usamos este numero para hacer pruebas, en caso contrario usariamos el objeto datosPago
                                ExpMonth = datosPago.MesCaducidad.ToString(),
                                ExpYear = datosPago.AnioCaducidad.ToString(),
                                Cvc = datosPago.CVV.ToString(),
                                Name = cliente.Nombre + " " + cliente.Apellidos,
                            }
                        };

                        Token tokenTarjeta = new TokenService().Create(tokenCreateOptions);

                        **/

                        // Usando el token, creamos la tarjeta
                        CardCreateOptions cardCreateOptions = new CardCreateOptions
                        {
                            Source = "tok_visa_debit" // en la ultima version de la API nos obliga a usar este "token" para pruebas
                        };
                        Card tarjetaCredito = new CardService().Create(customer.Id, cardCreateOptions);

                        // Por ultimo creamos el cargo
                        ChargeCreateOptions chargeOptions = new ChargeCreateOptions
                        {
                            Amount = (long)(cliente.PedidoActual.Total * 100),
                            Currency = "eur",
                            Source = tarjetaCredito.Id,
                            Description = $"Pedido del Proyecto TFG con el Id pedido: {cliente.PedidoActual.IdPedido}",
                            Customer = customer.Id
                        };

                        Charge cargoPedido = new ChargeService().Create(chargeOptions);


                        if (cargoPedido.Status.ToLower() == "succeeded")
                        {
                            return new RestMessage
                            {
                                Codigo = 0,
                                Mensaje = "Pago con tarjeta realizado correctamente",
                                Error = null,
                                DatosCliente = cliente,
                                TokenSesion = null,
                                OtrosDatos = null
                            };
                        }
                        else
                        {
                            return new RestMessage
                            {
                                Codigo = 2,
                                Mensaje = "Pago rechazado por la pasarela de pago, revisa los datos de tu tarjeta e intentalo de nuevo mas tarde",
                                Error = null,
                                DatosCliente = cliente,
                                TokenSesion = jwt,
                                OtrosDatos = null
                            };
                        }
                    }
                    else
                    {
                        // Creamos un customer para cargar el pedido
                        CustomerCreateOptions createOptions = new CustomerCreateOptions
                        {
                            Email = cliente.cuenta.Email,
                            Name = cliente.Nombre,
                            Phone = cliente.Telefono,
                            Address = new AddressOptions
                            {
                                City = direccionStripe.MuniDirecc.DMUN50,
                                State = direccionStripe.ProvDirec.PRO,
                                Country = direccionStripe.Pais,
                                Line1 = direccionStripe.Calle,
                                PostalCode = direccionStripe.CP.ToString()
                            },
                            Description = "Cliente prueba TFG",
                            Metadata = new Dictionary<string, string>
                        {
                            { "FechaNacimiento", cliente.FechaNacimiento.ToString() },
                            { "IdCliente", cliente.IdCliente }
                        }
                        };

                        Customer customerNuevo = new CustomerService().Create(createOptions);


                        /**
                        // Asociamos el customer a una tarjeta de credito
                        TokenCreateOptions tokenCreateOptions = new TokenCreateOptions
                        {
                            Card = new TokenCardOptions
                            {
                                Number = "4242424242424242", // usamos este numero para hacer pruebas, en caso contrario usariamos el objeto datosPago
                                ExpMonth = datosPago.MesCaducidad.ToString(),
                                ExpYear = datosPago.AnioCaducidad.ToString(),
                                Cvc = datosPago.CVV.ToString(),
                                Name = cliente.Nombre + " " + cliente.Apellidos,
                            }
                        };

                        Token tokenTarjeta = new TokenService().Create(tokenCreateOptions);
                        **/


                        // creamos la tarjeta de prueba
                        CardCreateOptions cardCreateOptions = new CardCreateOptions
                        {
                            Source = "tok_visa_debit" // en la ultima version de la API nos obliga a usar este "token" para pruebas
                        };
                        Card tarjetaCredito = new CardService().Create(customerNuevo.Id, cardCreateOptions);

                        // Por ultimo creamos el cargo
                        ChargeCreateOptions chargeOptions = new ChargeCreateOptions
                        {
                            Amount = (long)(cliente.PedidoActual.Total * 100),
                            Currency = "eur",
                            Source = tarjetaCredito.Id,
                            Description = $"Pedido del Proyecto TFG con el Id pedido: {cliente.PedidoActual.IdPedido}",
                            Customer = customerNuevo.Id
                        };

                        Charge cargoPedido = new ChargeService().Create(chargeOptions);


                        if (cargoPedido.Status.ToLower() == "succeeded")
                        {
                            return new RestMessage
                            {
                                Codigo = 0,
                                Mensaje = "Pago con tarjeta realizado correctamente",
                                Error = null,
                                DatosCliente = cliente,
                                TokenSesion = null,
                                OtrosDatos = null
                            };
                        }
                        else
                        {
                            return new RestMessage
                            {
                                Codigo = 2,
                                Mensaje = "Pago rechazado por la pasarela de pago, revisa los datos de tu tarjeta e intentalo de nuevo mas tarde",
                                Error = null,
                                DatosCliente = cliente,
                                TokenSesion = jwt,
                                OtrosDatos = null
                            };
                        }
                    }
                }
                else
                {
                    return new RestMessage
                    {
                        Codigo = 5,
                        Mensaje = "Tiempo de expiracion excedido, vuelva a realizar login",
                        Error = null,
                        DatosCliente = null,
                        TokenSesion = null,
                        OtrosDatos = null
                    };
                }

            }
            catch (Exception ex)
            {
                return new RestMessage
                {
                    Codigo = 1,
                    Mensaje = "error en el cobro con tarjeta",
                    Error = ex.Message,
                    DatosCliente = null,
                    TokenSesion = null,
                    OtrosDatos = null
                };
            }
        }

        [HttpPost]
        public async Task<String> RealizarPagoPayPal([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                Cliente cliente = JsonSerializer.Deserialize<Cliente>(datos["cliente"]);
                DatosPago datosPago = JsonSerializer.Deserialize<DatosPago>(datos["datosPago"]);
                Pedido pedido = cliente.PedidoActual;
                pedido.IdPedido = ObjectId.GenerateNewId().ToString();
                String jwt = datos["jwt"];

                if (validarJWT(jwt))
                {
                    Direccion direccionPaypal;

                    if (datosPago.DireccionEnvio == null && datosPago.DireccionPrincipal == null)
                    {
                        return null;
                    }
                    else
                    {
                        direccionPaypal = datosPago.DireccionEnvio ?? datosPago.DireccionPrincipal;

                        // Creamos el contexto para trabajar con la api de PayPal
                        
                        // Token de Acceso
                        OAuthTokenCredential tokenAccesoCredential = new OAuthTokenCredential(
                            this.accesoAppSettings["PayPal:ClientId"],
                            this.accesoAppSettings["PayPal:SecretKey"],
                            new Dictionary<String, String>
                            {
                                {"mode", this.accesoAppSettings["PayPal:mode"] },
                                {"business", this.accesoAppSettings["PayPal:bussinessDandboxAccount"] }
                            }
                            );
                        APIContext apiContext = new APIContext(tokenAccesoCredential.GetAccessToken());
                        apiContext.Config = PayPal.Api.ConfigManager.Instance.GetProperties();

                        // Creamos el cargo de paypal con las urls para redireccionar al cliente al 
                        // pago y la recepcion del servidor
                        ItemList itemsPaypal = new ItemList
                        {
                            items = new List<Item>()
                        };

                        List<Item> listaItemsPayPal = pedido.ItemsPedido.Select(
                            (ItemPedido item) => new Item
                            {
                                name = item.ProductoItem.title,
                                price = item.ProductoItem.price.ToString().Replace(",", "."),
                                currency = "EUR",
                                quantity = item.CatidadItem.ToString(),
                                sku = item.ProductoItem.id.ToString()
                            }
                            ).ToList<Item>();
                        itemsPaypal.items = listaItemsPayPal;

                        // url del pago de paypal
                        String redirectURL = $"https://localhost:7083/api/Tienda/PayPalCallBack?idPedido={pedido.IdPedido}&idCliente={cliente.IdCliente}";

                        Payment cargoCuenta = new Payment()
                        {
                            intent = "sale",
                            payer = new Payer { payment_method = "paypal"},
                            transactions = new List<Transaction>()
                            {
                                new Transaction()
                                {
                                    description = $"Pedido MegaShop con Id:{pedido.IdPedido} en fecha : {pedido.FechaPedido}",
                                    invoice_number = pedido.IdPedido.ToString(),
                                    item_list = itemsPaypal,
                                    amount = new Amount()
                                    {
                                        currency = "EUR",
                                        details = new Details()
                                        {
                                            tax = "0",
                                            shipping = pedido.GastosEnvio.ToString().Replace(",", "."),
                                            subtotal = pedido.SubTotal.ToString().Replace(",", ".")
                                        },
                                        total = pedido.Total.ToString().Replace(",", ".")
                                    }
                                }
                            },
                            redirect_urls = new RedirectUrls
                            {
                                cancel_url = redirectURL + "&Cancel=true",
                                return_url = redirectURL + "&Cancel=false"
                            }
                        }.Create(apiContext);

                        // Guardamos datos en la base de datos para recuperar en el CallBack
                        Boolean introduccionDatos = await this.accesoBD.IntroducirDatosPayPal(new Models.PaypalPedidoInfo
                        {
                            IdCargo = cargoCuenta.id,
                            IdCliente = cliente.IdCliente,
                            IdPedido = pedido.IdPedido,
                            PayPalContextClient = JsonSerializer.Serialize<APIContext>(apiContext)
                        });

                        if (introduccionDatos)
                        {
                            String urlPasarelaPaypal = cargoCuenta.links.Where((Links linksPaypal) => linksPaypal.rel.ToLower().Trim()
                            .Equals("approval_url")).Select((Links linkspaypal) => linkspaypal.href).Single<String>();
                            return urlPasarelaPaypal;
                        }
                        else { return null; }
                    }
                }
                else { return null; }
            }
            catch (Exception ex) { return null; }
        }
        
        [HttpPost]
        public async Task<RestMessage> DesearProducto([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                String idProducto = datos["idProducto"], jwt = datos["jwt"], idCliente = datos["idCliente"];

                if (validarJWT(jwt))
                {
                    Boolean likearProducto = await this.accesoBD.DesearProducto(idProducto, idCliente);
                    if (likearProducto)
                    {
                        return new RestMessage
                        {
                            Codigo = 0,
                            DatosCliente = null,
                            Error = null,
                            Mensaje = "Likeado correcto",
                            OtrosDatos = null,
                            TokenSesion = jwt
                        };
                    }
                    else
                    {
                        return new RestMessage
                        {
                            Codigo = 2,
                            DatosCliente = null,
                            Error = null,
                            Mensaje = "No se ha podido likear el producto",
                            OtrosDatos = null,
                            TokenSesion = jwt
                        };
                    }
                }
                else
                {
                    return new RestMessage
                    {
                        Codigo = 5,
                        Mensaje = "Tiempo de expiracion excedido, vuelva a realizar login",
                        Error = null,
                        DatosCliente = null,
                        TokenSesion = null,
                        OtrosDatos = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new RestMessage
                {
                    Codigo = 1,
                    Mensaje = "error al likear producto",
                    Error = ex.Message,
                    DatosCliente = null,
                    TokenSesion = null,
                    OtrosDatos = null
                };
            }
        }

        [HttpPost]
        public async Task<RestMessage> DesDesearProducto([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                String idProducto = datos["idProducto"], jwt = datos["jwt"], idCliente = datos["idCliente"];

                if (validarJWT(jwt))
                {
                    Boolean desLikearProducto = await this.accesoBD.DesDesearProducto(idProducto, idCliente);
                    if (desLikearProducto)
                    {
                        return new RestMessage
                        {
                            Codigo = 0,
                            DatosCliente = null,
                            Error = null,
                            Mensaje = "Deslikeado correcto",
                            OtrosDatos = null,
                            TokenSesion = jwt
                        };
                    }
                    else
                    {
                        return new RestMessage
                        {
                            Codigo = 2,
                            DatosCliente = null,
                            Error = null,
                            Mensaje = "No se ha podido deslikear el producto",
                            OtrosDatos = null,
                            TokenSesion = jwt
                        };
                    }
                }
                else
                {
                    return new RestMessage
                    {
                        Codigo = 5,
                        Mensaje = "Tiempo de expiracion excedido, vuelva a realizar login",
                        Error = null,
                        DatosCliente = null,
                        TokenSesion = null,
                        OtrosDatos = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new RestMessage
                {
                    Codigo = 1,
                    Mensaje = "error al deslikear producto",
                    Error = ex.Message,
                    DatosCliente = null,
                    TokenSesion = null,
                    OtrosDatos = null
                };
            }
        }

        [HttpPost]
        public async Task<RestMessage> AlmacenarComentario([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                String comentario = datos["comentario"], jwt = datos["jwt"], idCliente = datos["idCliente"],
                    nombreCliente = datos["nombreCliente"], idProducto = datos["idProducto"];
                if (validarJWT(jwt))
                {
                    Boolean comentarioSubido = await this.accesoBD.SubirComentario(idCliente, comentario, nombreCliente, idProducto);
                    if (comentarioSubido)
                    {
                        return new RestMessage
                        {
                            Codigo = 0,
                            DatosCliente = null,
                            Error = null,
                            Mensaje = "Comentario subido correctamente",
                            OtrosDatos = null,
                            TokenSesion = jwt
                        };
                    }
                    else
                    {
                        return new RestMessage
                        {
                            Codigo = 2,
                            DatosCliente = null,
                            Error = null,
                            Mensaje = "No se ha podido subir el comentario",
                            OtrosDatos = null,
                            TokenSesion = jwt
                        };
                    }
                }
                else
                {
                    return new RestMessage
                    {
                        Codigo = 5,
                        Mensaje = "Tiempo de expiracion excedido, vuelva a realizar login",
                        Error = null,
                        DatosCliente = null,
                        TokenSesion = null,
                        OtrosDatos = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new RestMessage
                {
                    Codigo = 1,
                    Mensaje = "error al almacenar comentario",
                    Error = ex.Message,
                    DatosCliente = null,
                    TokenSesion = null,
                    OtrosDatos = null
                };
            }
        }

        [HttpPost]
        public async Task<List<ComentarioCli>> RecuperarComentariosProducto([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                List<ComentarioCli> comentarios = await this.accesoBD.RecuperarComentariosProducto(datos["idProducto"]);
                return comentarios;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion


        #region metodos de la clase

        [HttpGet]
        public async Task<IActionResult> PayPalCallBack([FromQuery] String PayerId, [FromQuery] String idPedido, [FromQuery] String idCliente, [FromQuery] String Cancel = "false")
        {
            // Recuperamos el contexto del cliente
            PaypalPedidoInfo datosPed = await this.accesoBD.RecuperarDatosPayPal(idPedido);

            if (datosPed == null) { return Redirect("https://localhost:7083/Tienda/Cobro"); } else
            {
                APIContext apiContext = JsonSerializer.Deserialize<APIContext>(datosPed.PayPalContextClient);
                if (Convert.ToBoolean(Cancel))
                {
                    return Redirect("https://localhost:7083/Tienda/Carrito");
                }

                // Ejecutamos el pago
                Payment cargoCuenta = new Payment() { id = datosPed.IdCargo }.Execute(apiContext, new PaymentExecution { payer_id = PayerId });

                switch(cargoCuenta.state)
                {
                    case "approved":
                        return Redirect($"https://localhost:7083/Tienda/PedidoRealizadoOK");
                        break;

                    case "failed":
                        return Redirect("https://localhost:7083/Tienda/Carrito");
                        break;

                }
                return Redirect("https://localhost:7083/Tienda/Carrito");
            }
        }

        private Boolean validarJWT(String jwt)
        {
            try
            {
                // Comprobacion de expiracion del token
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler(); // Manejador de token
                var jwtToken = tokenHandler.ReadToken(jwt); // leemos el token a traves del handler

                // Comprobamos el tiempo de expiracion
                DateTime expiracionToken = jwtToken.ValidTo;
                DateTime tiempoActual = DateTime.UtcNow;

                // Si la fecha de expiracion es menor que el tiempo actual, entonces es que el jwt ha expirado
                return tiempoActual < expiracionToken;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion
    }
}
