using ProyectoTFG.Server.Models.Interfaces;
using ProyectoTFG.Shared;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace ProyectoTFG.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        #region atributos del controlador


        // Inyectamos el acceso a la base de datos y a el archivo appsettings.json

        private IAccesoDatos accesoBD;
        private IConfiguration accesoAppSettings;
        private IClienteEmail emailService;

        public ClienteController(IAccesoDatos accesoBD, IConfiguration accesoAppSettings, IClienteEmail emailService)
        {
            this.accesoBD = accesoBD;
            this.accesoAppSettings = accesoAppSettings;
            this.emailService = emailService;
        }

        #endregion

        #region metodos del controlador
        
        [HttpPost]
        public async Task<RestMessage> Login([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                Cuenta cuenta = JsonSerializer.Deserialize<Cuenta>(datos["cuenta"]);
                Cliente cliente = await this.accesoBD.ComprobarCredenciales(cuenta.Email, cuenta.Password);

                if (cliente == null)
                {
                    return new RestMessage
                    {
                        Codigo = 2,
                        Mensaje = "Las credenciales son incorrectas",
                        Error = null,
                        DatosCliente = null,
                        TokenSesion = null,
                        OtrosDatos = null
                    };
                }
                else
                {
                    if (!cliente.cuenta.CuentaActivada)
                    {
                        return new RestMessage
                        {
                            Codigo = 3,
                            Mensaje = "Las cuenta no ha sido activada, revise su correo electronico",
                            Error = null,
                            DatosCliente = null,
                            TokenSesion = null,
                            OtrosDatos = null
                        };
                    }
                    else
                    {
                        String tokenJWt = this.generarJWT(cliente.Nombre, cliente.Apellidos, cuenta.Email, cliente.IdCliente);
                        cliente.cuenta.Password = "";
                        return new RestMessage
                        {
                            Codigo = 0,
                            Mensaje = "login correcto",
                            Error = null,
                            DatosCliente = cliente,
                            TokenSesion = tokenJWt,
                            OtrosDatos = null
                        };
                    }
                }
                
            }
            catch (Exception ex)
            {
                return new RestMessage
                {
                    Codigo = 1,
                    Mensaje = "Ha ocurrido un error en el login, vuelve a intentarlo mas tarde",
                    Error = ex.Message,
                    DatosCliente = null,
                    TokenSesion = null,
                    OtrosDatos = null
                };
            }
        }

        [HttpPost]
        public async Task<RestMessage> ObtenerClienteGoogle([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                Cliente cliente = await this.accesoBD.ObtenerClienteIdGoogle(datos["idGoogle"]);
                if (cliente != null)
                {
                    String tokenJWT = this.generarJWT(cliente.Nombre,cliente.Apellidos, cliente.cuenta.Email, cliente.IdCliente);
                    return new RestMessage
                    {
                        Codigo = 0,
                        Mensaje = "Login google correcto",
                        Error = null,
                        DatosCliente = cliente,
                        TokenSesion = tokenJWT,
                        OtrosDatos = null
                    };
                }
                else
                {
                    return new RestMessage
                    {
                        Codigo = 2,
                        Mensaje = "El id google no es correcto.",
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
                    Mensaje = "Ha ocurrido un error en la obtencion del cliente, vuelve a intentarlo mas tarde",
                    Error = ex.Message,
                    DatosCliente = null,
                    TokenSesion = null,
                    OtrosDatos = null
                };
            }
        }

        [HttpPost]
        public async Task<RestMessage> ObtenerClienteId([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                Cliente cliente = await this.accesoBD.ObtenerClienteId(datos["idCliente"]);
                if (cliente != null)
                {
                    String jwt = generarJWT(cliente.Nombre, cliente.Apellidos, cliente.cuenta.Email, cliente.IdCliente);
                    return new RestMessage
                    {
                        Codigo = 0,
                        Mensaje = "Obtencion correcta",
                        Error = null,
                        DatosCliente = cliente,
                        TokenSesion = jwt,
                        OtrosDatos = null
                    };
                }
                else
                {
                    return new RestMessage
                    {
                        Codigo = 2,
                        Mensaje = "El id google no es correcto.",
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
                    Mensaje = "Ha ocurrido un error en la obtencion del cliente, vuelve a intentarlo mas tarde",
                    Error = ex.Message,
                    DatosCliente = null,
                    TokenSesion = null,
                    OtrosDatos = null
                };
            }
        }

        [HttpPost]
        public async Task<RestMessage> Registro([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                Cliente cliente = JsonSerializer.Deserialize<Cliente>(datos["cliente"]);
                Boolean registroCorrecto = await this.accesoBD.RegistrarCliente(cliente);

                if (registroCorrecto)
                {
                    Cliente clienteRecup = await this.accesoBD.ComprobarCredenciales(cliente.cuenta.Email, cliente.cuenta.Password);
                    String urlMail = Url.RouteUrl("ActivarCuenta", new {id = clienteRecup.IdCliente}, "https", "localhost:7083");
                    String mensajeEmail = $@"<h3><strong> Te has registrado correctamente en MegaShop</strong></h3>
                                            Pulsa el siguiente enlace para <a href={urlMail}>ACTIVAR TU CUENTA </a> de usuario en MegaShop";

                    this.emailService.enviarEmail(clienteRecup.cuenta.Email, "Bienvenido al portal de MegaShop, activa tu cuenta", mensajeEmail, null);

                    return new RestMessage
                    {
                        Codigo = 0,
                        Mensaje = "registro correcto",
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
                        Mensaje = "Registro fallido, intentelo mas tarde",
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
                    Mensaje = "Error en el registro",
                    Error = ex.Message,
                    DatosCliente = null,
                    TokenSesion = null,
                    OtrosDatos = null
                };
            }
        }

        [HttpPost]
        public async Task<RestMessage> ModificarCliente([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                Cliente cliente = JsonSerializer.Deserialize<Cliente>(datos["cliente"]);
                Boolean cambioPass = Boolean.Parse(datos["cambioPass"]);
                String jwt = datos["jwt"];

                if (validarJWT(jwt))
                {
                    Cliente clienteModificado = await this.accesoBD.ModificarCliente(cliente, cambioPass);

                    if (clienteModificado != null)
                    {
                        return new RestMessage
                        {
                            Codigo = 0,
                            Mensaje = "Modificacion del cliente correcta.",
                            Error = null,
                            DatosCliente = clienteModificado,
                            TokenSesion = jwt,
                            OtrosDatos = null
                        };
                    }
                    else
                    {
                        return new RestMessage
                        {
                            Codigo = 2,
                            Mensaje = "Modificacion del cliente fallido, intentelo mas tarde",
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
                    Mensaje = "Error en la modificacion del cliente",
                    Error = ex.Message,
                    DatosCliente = null,
                    TokenSesion = null,
                    OtrosDatos = null
                };
            }
        }

        [HttpPost]
        public async Task<RestMessage> ModificarImagen([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                Cliente cliente = JsonSerializer.Deserialize<Cliente>(datos["cliente"]);
                String imagen = datos["imagen"];
                String jwt = datos["jwt"];

                if (validarJWT(jwt))
                {
                    Boolean modCorrecta = await this.accesoBD.ModificarImagen(cliente, imagen);

                    if (modCorrecta)
                    {
                        return new RestMessage
                        {
                            Codigo = 0,
                            Mensaje = "Modificacion de la imagen correcta",
                            Error = null,
                            DatosCliente = null,
                            TokenSesion = datos["jwt"],
                            OtrosDatos = null
                        };
                    }
                    else
                    {
                        return new RestMessage
                        {
                            Codigo = 2,
                            Mensaje = "Modificacion de la imagen fallida, intentelo mas tarde",
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
                    Mensaje = "Error en la modificacion de la imagen.",
                    Error = ex.Message,
                    DatosCliente = null,
                    TokenSesion = null,
                    OtrosDatos = null
                };
            }
        }

        [HttpPost]
        public async Task<RestMessage> IntroducirDireccion([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                Direccion direc = JsonSerializer.Deserialize<Direccion>(datos["direccion"]);
                Cliente cliente = JsonSerializer.Deserialize<Cliente>(datos["cliente"]);
                String jwt = datos["jwt"];

                if (validarJWT(jwt))
                {
                    Boolean insertDireccionOk = await this.accesoBD.RegistrarDireccion(direc, cliente.IdCliente);

                    if (insertDireccionOk)
                    {
                        // Si hemos insertado bien la direccion, la metemos en el cliente
                        cliente.MisDirecciones.Add(direc);

                        return new RestMessage
                        {
                            Codigo = 0,
                            Mensaje = "registro correcto",
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
                            Mensaje = "Ha ocurrido un error inesperado, intentelo mas tarde",
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
                    Mensaje = "Error en la inserccion de la direccion",
                    Error = ex.Message,
                    DatosCliente = null,
                    TokenSesion = null,
                    OtrosDatos = null
                };
            }
        }

        [HttpPost]
        public async Task<RestMessage> ModificarDireccion([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                Cliente cliente = JsonSerializer.Deserialize<Cliente>(datos["cliente"]);
                Direccion direccion = JsonSerializer.Deserialize<Direccion>(datos["direccion"]);
                String jwt = datos["jwt"];

                if (validarJWT(jwt))
                {
                    Direccion direccionModificada = await this.accesoBD.ModificarDireccion(direccion, cliente.IdCliente);

                    if (direccionModificada != null && cliente != null)
                    {
                        int indiceDir = cliente.MisDirecciones.FindIndex((Direccion d) => d.IdDireccion == direccionModificada.IdDireccion);
                        cliente.MisDirecciones[indiceDir] = direccionModificada;

                        return new RestMessage
                        {
                            Codigo = 0,
                            Mensaje = "Modificacion de la direccion correcta",
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
                            Mensaje = "Modificacion de la direccion fallida, intentelo mas tarde",
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
                    Mensaje = "error en la modificacion de la direccion",
                    Error = ex.Message,
                    DatosCliente = null,
                    TokenSesion = null,
                    OtrosDatos = null
                };
            }
        }

        [HttpPost]
        public async Task<RestMessage> EliminarDireccion([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                Cliente cliente = JsonSerializer.Deserialize<Cliente>(datos["cliente"]);
                Direccion direccion = JsonSerializer.Deserialize<Direccion>(datos["direccion"]);
                String jwt = datos["jwt"];

                if (validarJWT(jwt))
                {
                    Boolean dirDeleted = await this.accesoBD.EliminarDireccion(direccion, cliente.IdCliente);

                    if (dirDeleted)
                    {
                        int posDireccion = cliente.MisDirecciones.FindIndex((Direccion d) => d.IdDireccion == direccion.IdDireccion);
                        cliente.MisDirecciones.RemoveAt(posDireccion);
                        return new RestMessage
                        {
                            Codigo = 0,
                            Mensaje = "Eliminado de la direccion correcta",
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
                            Mensaje = "Eliminacion de la direccion fallida, intentelo mas tarde",
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
                    Mensaje = "error en la eliminacion de la direccion",
                    Error = ex.Message,
                    DatosCliente = null,
                    TokenSesion = null,
                    OtrosDatos = null
                };
            }
        }

        [HttpGet]
        public String LoginGoogle()
        {
            /*
              codigo teorico que no dicen en todos lados que el navegador bloquea al hacer una redireccion forzada
              cod 302 a una url que no es la de blazor
             
            String redirectUrl = Url.Action(nameof(LoginCallBackGoogle), "Cliente", null, Request.Scheme);
            AuthenticationProperties propsGoogle = new AuthenticationProperties
            {
                RedirectUri = redirectUrl,
                IsPersistent = true, // obliga a generar codigo cliente
                AllowRefresh = true
            };
            return Challenge(propsGoogle, GoogleDefaults.AuthenticationScheme); // redirecciona pero de otra forma
            


            // ¿solucion? generamos url a pelo con los parametro de necesita google
            // https://accounts.google.com/o/oauth2/v2/auth ? 
            //            response_type = code &
            //            client_id = 987193666356 - a3ehmpjueng8agmf3csvt77q70ul2ig0.apps.googleusercontent.com &
            //            redirect_uri = https://localhost:7016/signin-google & 
            //            scope = openid profile email &
            //            state = CfDJ8O8V7QK....

            */

            // - redirectURl, client_id, scope, state, accesstype

            try
            {
                String redirectURL = Url.Action(nameof(LoginCallBackGoogle), "Cliente", null, Request.Scheme);
                String client_id = this.accesoAppSettings["Google:client_id"];
                String scope = "openid profile email";  //"https://www.googleapis.com/auth/userinfo.profile";
                String state = "6gre66v1df6v1a54FBAEBaa8baebAEBAERBAEB"; // codigo aleatorio
                String accessType = "offline";

                String urlRedirect = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={client_id}&redirect_uri={redirectURL}&response_type=code&scope={scope}&acces_type={accessType}&state={state}";
                return urlRedirect;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion
        #region metodos de la clase

        [HttpGet]
        public async Task<IActionResult> LoginCallBackGoogle([FromQuery] String code)
        {
            // En la url que nos manda google, van estos parametros:
            //  -code <-- codigo cliente para solicitar jwt de uso de las google-apis
            // -state <-- codigo aleatorio para id.cliente
            // -scope <-- ambito de valide del codigo para solicitar el uso de las google-apis
            // -authuser
            // -prompt

            try
            {
                String codeGoogle = this.HttpContext.Request.Query["code"];
                String[] scopes = this.HttpContext.Request.Query["scope"].ToString().Split(" ");

                GoogleAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(
                        new GoogleAuthorizationCodeFlow.Initializer
                        {
                            ClientSecrets = new Google.Apis.Auth.OAuth2.ClientSecrets
                            {
                                ClientId = this.accesoAppSettings["Google:client_id"],
                                ClientSecret = this.accesoAppSettings["Google:client_secret"]
                            },
                            Scopes = scopes, // "https://www.googleapis.com/auth/userinfo.profile"
                            DataStore = new FileDataStore("Google.Apis.Auth")
                        }
                    );

                // Generamos el token jwt del cliente
                TokenResponse tokenResponse = await flow.ExchangeCodeForTokenAsync("user", codeGoogle, "https://localhost:7083/api/Cliente/LoginCallBackGoogle", CancellationToken.None);
                UserCredential credencialesAPI = new UserCredential(flow, "user", tokenResponse);

                // a obtener info del cliente que ha usado gmail para autentificarse
                Oauth2Service servAPIS = new Oauth2Service(new Google.Apis.Services.BaseClientService.Initializer { HttpClientInitializer = credencialesAPI });
                Userinfo userinfo = await servAPIS.Userinfo.Get().ExecuteAsync();

                // Generamos un cliente con los datos del userInfo y lo guardamos en la base de datos.
                // Por otra parte, guardamos en un objeto el id del userInfo junto al id del cliente guardado

                Cliente nuevoCliente = new Cliente
                {
                    Nombre = userinfo.GivenName,
                    Apellidos = userinfo.FamilyName,
                    cuenta = new Cuenta
                    {
                        CuentaActivada = true,
                        Email = userinfo.Email,
                    }
                };

                // Antes de registrar y redirigir, tenemos que comprobar si el cliente esta registrado
                Cliente clienteRecup = await this.accesoBD.ObtenerClienteIdGoogle(userinfo.Id);
                if (clienteRecup == null)
                {
                    // El cliente es nuevo
                    Cliente clienteRegistrado = await this.accesoBD.RegistrarClienteGoogle(nuevoCliente);
                    Boolean registroCredsGoogle = await this.accesoBD.GuardarCredencialesGoogle(userinfo.Id, clienteRegistrado.IdCliente);
                    return Redirect($"https://localhost:7083/Cliente/PanelCliente?idgooglesesion={userinfo.Id}");
                }
                else
                {
                    return Redirect($"https://localhost:7083/Cliente/PanelCliente?idgooglesesion={userinfo.Id}");
                }
            }
            catch (Exception ex)
            {
                return Redirect($"https://localhost:7083/Cliente/Login");
            }
        }

        [HttpGet(Name = "ActivarCuenta")]
        public async Task ActivarCuenta([FromQuery] String id)
        {
            Cliente cliente = await this.accesoBD.ObtenerClienteId(id);
            if (cliente != null && cliente.IdCliente == id)
            {
                Boolean respServer = await this.accesoBD.ActivarCuenta(id);
            }
        }

        /*************************************************************************/

        [HttpGet]
        public async Task<List<Provincia>> RecuperarProvincias()
        {
            HttpClient clienteHttp = new HttpClient();
            APIRestResponse<Provincia> respuesta = await clienteHttp.GetFromJsonAsync<APIRestResponse<Provincia>>("https://apiv1.geoapi.es/provincias?type=JSON&key=&sandbox=1");
            return respuesta.DatosConvertidos(respuesta.Data).OrderBy((Provincia p) => p.PRO).ToList<Provincia>();
        }

        [HttpGet]
        public async Task<List<Municipio>> RecuperarMunicipios([FromQuery] String codprov)
        {
            HttpClient clienteHttp = new HttpClient();
            APIRestResponse<Municipio> respuesta = await clienteHttp.GetFromJsonAsync<APIRestResponse<Municipio>>($"https://apiv1.geoapi.es/municipios?CPRO={codprov}&type=JSON&key=&sandbox=1");
            return respuesta.DatosConvertidos(respuesta.Data).OrderBy((Municipio m) => m.DMUN50).ToList<Municipio>();
        }

        public class APIRestResponse<T>
        {
            //claes para mapear respuesta de servicio REST para obtener provs y municipios
            //es un json con estas props: { update_date:'', size: xx, data: [ {..},{..},{..}], warning:'' }
            #region ....propiedades de clase .....
            public String Update_date { get; set; }

            public int Size { get; set; }

            public List<JsonElement> Data { get; set; }

            public String Warning { get; set; }
            #endregion


            #region ...metodos de clase....
            public List<T> DatosConvertidos(List<JsonElement> data)
            {
                List<T> _datosConvertidos = new List<T>();
                foreach (JsonElement item in data)
                {
                    _datosConvertidos.Add(JsonSerializer.Deserialize<T>(item));
                }
                return _datosConvertidos;
            }

            #endregion

        }
        

        /*************************************************************************/

        private String generarJWT(String nombre, String apellidos, String email, String idCliente)
        {
            // Generamos una clave en base a un texto
            SecurityKey claveFirma = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(this.accesoAppSettings["JWT:firmaJWT"]));

            // Asignacion de datos del token
            JwtSecurityToken jwt = new JwtSecurityToken(
                    issuer: this.accesoAppSettings["JWT:issuer"],
                    audience: null,
                    claims: new List<Claim> {
                                new Claim("nombre",nombre),
                                new Claim("apellidos", apellidos),
                                new Claim("email", email),
                                new Claim("idCliente", idCliente)
                    },
                    notBefore: null,
                    expires: DateTime.Now.AddHours(2), 
                    signingCredentials: new SigningCredentials(claveFirma, SecurityAlgorithms.HmacSha256)
                );

            String tokenjwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return tokenjwt;
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
