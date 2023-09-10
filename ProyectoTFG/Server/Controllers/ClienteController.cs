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

        public ClienteController(IAccesoDatos accesoBD, IConfiguration accesoAppSettings)
        {
            this.accesoBD = accesoBD;
            this.accesoAppSettings = accesoAppSettings;
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
        public async Task<RestMessage> Registro([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                Cliente cliente = JsonSerializer.Deserialize<Cliente>(datos["cliente"]);
                Boolean registroCorrecto = await this.accesoBD.RegistrarCliente(cliente);

                if (registroCorrecto)
                {
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
                            Mensaje = "Modificacion del cliente correcta",
                            Error = null,
                            DatosCliente = clienteModificado,
                            TokenSesion = null,
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
             * codigo teorico que no dicen en todos lados que el navegador bloquea al hacer una redireccion forzada
             * cod 302 a una url que no es la de blazor
             
            String redirectUrl = Url.Action(nameof(LoginCallBackGoogle), "RESTCliente", null, Request.Scheme);
            AuthenticationProperties propsGoogle = new AuthenticationProperties
            {
                RedirectUri = redirectUrl,
                IsPersistent = true, // obliga a generar codigo cliente
                AllowRefresh = true
            };
            return Challenge(propsGoogle, GoogleDefaults.AuthenticationScheme); // redirecciona pero de otra forma
            */

            // ¿solucion? generamos url a pelo con los parametro de necesita google
            // - redirectURl, client_id, scope, state, accesstype
            String redirectURL = Url.Action(nameof(LoginCallBackGoogle), "Cliente", null, Request.Scheme);
            String client_id = this.accesoAppSettings["Google:client_id"];
            String scope = "openid profile email";  //"https://www.googleapis.com/auth/userinfo.profile";
            String state = "6gre66v1df6v1a54FBAEBaa8baebAEBAERBAEB"; // codigo aleatorio
            String accessType = "offline";

            String urlRedirect = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={client_id}&redirect_uri={redirectURL}&response_type=code&scope={scope}&acces_type={accessType}&state={state}";
            return urlRedirect;
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

            TokenResponse tokenResponse = await flow.ExchangeCodeForTokenAsync("user", codeGoogle, "https://localhost:7088/api/RESTCliente/LoginCallBackGoogle", CancellationToken.None);
            UserCredential credencialesAPI = new UserCredential(flow, "user", tokenResponse);

            // a obtener info del cliente que ha usado gmail para autentificarse
            Oauth2Service servAPIS = new Oauth2Service(new Google.Apis.Services.BaseClientService.Initializer { HttpClientInitializer = credencialesAPI });
            Userinfo userinfo = await servAPIS.Userinfo.Get().ExecuteAsync();

            // con este objecto userInfo generas tu propio JWT de la aplicacion
            // lo almacenas en mongoDB en coleccion googleSession, (y su id lo almacenamos en una variable que pasamos en la url) unicamente de forma temporal
            // en cuanto se desloguea el cliente, lo eliminamos de la base de datos
            String idGoogleSession = "615616";

            return Redirect($"https://localhost:7088/Cliente/PanelCliente?idgooglesesion{idGoogleSession}");
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
