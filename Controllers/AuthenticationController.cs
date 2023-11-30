using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using ReportApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ReportApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly JwtSettings _jwtSettings;
        private string Constr;
        public NpgsqlConnection connection;
        public AuthenticationController(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
            Constr = DatabaseConfig.databaseConnectionString;
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] User userInformation)
        {
            var user = authenticate(userInformation);
            if(user.Result == null) { return NotFound("User not found"); }

            var userWithToken = createToken(user.Result);
            return Ok(userWithToken);
        }

        private async Task<User>? authenticate(User userInformation)
        {
            try
            {
                connection = new NpgsqlConnection(Constr);
                connection.Open();

                string query = " Select * from appusers where username=@name and userpassword=@password";

                NpgsqlCommand command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@name", userInformation.name);
                command.Parameters.AddWithValue("@password", userInformation.password);

                NpgsqlDataReader reader= await command.ExecuteReaderAsync();

                if(reader.Read())
                {
                    var user = new User
                    {
                        Id = reader.GetInt16(0),
                        name = reader.GetString(1),
                        password = reader.GetString(2),
                        role = reader.GetString(3)
                    };
                    reader.Close();

                    return user;
                }
                return null;
                
            }catch(Exception ex)
            {
                return null;
            }

        }

        private User createToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claimSeries = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.name!),
                new Claim("USERIDCLAIM",user.Id.ToString()),
                new Claim(ClaimTypes.Role,user.role)
            };
            var token = new JwtSecurityToken(claims:claimSeries,expires: DateTime.Now.AddMinutes(30),signingCredentials: credentials);

            user.token= new JwtSecurityTokenHandler().WriteToken(token);
            return user;
        }
    }
}
