using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProyectoTFG.Server.Models.Interfaces;
using ProyectoTFG.Shared;
using System.IdentityModel.Tokens.Jwt;

namespace ProyectoTFG.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AdministracionController : ControllerBase
    {
        #region atributos del controlador

        private IAccesoDatos accesoBD;

        public AdministracionController(IAccesoDatos accesoBD)
        {
            this.accesoBD = accesoBD;
        }

        #endregion

        #region metodos del controlador

        [HttpPost]
        public async Task<List<Cliente>> GetClientes([FromBody] Dictionary<String, String> datos)
        {
            try
            {
                String jwt = datos["jwt"];
                if (validarJWT(jwt))
                {
                    List<Cliente> listaClientes = await this.accesoBD.GetClientes();
                    return listaClientes;
                }
                else { return null; }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        /*******************************************************************/

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
