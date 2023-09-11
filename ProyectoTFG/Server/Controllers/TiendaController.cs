using Microsoft.AspNetCore.Mvc;
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

        /**
        [HttpPost]
        public async Task<RestMessage> RealizarPagoPayPal([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                Cliente cliente = JsonSerializer.Deserialize<Cliente>(datos["cliente"]);
                DatosPago datosPago = JsonSerializer.Deserialize<DatosPago>(datos["datosPago"]);
                String jwt = datos["jwt"];

                if (validarJWT(jwt))
                {
                    Direccion direccionPaypal;

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
                        direccionPaypal = datosPago.DireccionEnvio ?? datosPago.DireccionPrincipal;

                        

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
                    Mensaje = "error en el cobro con PayPal",
                    Error = ex.Message,
                    DatosCliente = null,
                    TokenSesion = null,
                    OtrosDatos = null
                };
            }
        }

        **/
        

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
