using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using BackendSGI.Models;

using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace BackendSGI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutenticacionController : ControllerBase
    {
        private readonly string secretKey;
        private readonly string cadenaSQL;

        public AutenticacionController(IConfiguration config)
        {  
            secretKey = config.GetSection("settings").GetSection("secretKey").ToString();
            cadenaSQL = config.GetConnectionString("CadenaSQL");
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginUsuario([FromBody] CredencialesUsuario credenciales)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    await conexion.OpenAsync();
                    var cmd = new SqlCommand("sp_LoginUsuario", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@Correo", credenciales.Correo));
                    cmd.Parameters.Add(new SqlParameter("@Contrasena", credenciales.Contrasena));

                    var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(resultadoParam);

                    await cmd.ExecuteNonQueryAsync();

                    var resultado = (int)resultadoParam.Value;

                    if (resultado != -1)
                    {
                        var keyBytes = Encoding.ASCII.GetBytes(secretKey);
                        var claims = new ClaimsIdentity();

                        claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, credenciales.Contrasena));

                        var tokenDescriptor = new SecurityTokenDescriptor
                        {
                            Subject = claims,
                            Expires = DateTime.UtcNow.AddMinutes(60),
                            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
                        };

                        var tokenHandler = new JwtSecurityTokenHandler();
                        var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);

                        string tokencreado = tokenHandler.WriteToken(tokenConfig);

                        var response = new
                        {
                            usuarioId = resultado,
                            token = tokencreado
                        };

                        return StatusCode(StatusCodes.Status200OK, new { message = "OK", response });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status401Unauthorized, new { token = "" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }
    }
}
