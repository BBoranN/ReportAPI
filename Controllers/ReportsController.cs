using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Npgsql;
using ReportApi.Models;

namespace ReportApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {

        private readonly JwtSettings _jwtSettings;
        private string Constr;
        public NpgsqlConnection connection;
        public ReportsController(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
            Constr = "User ID=postgres;Password=password;Server=localhost;Port=5433;Database=postgres;Integrated Security=true;Pooling=true; ";
        }

        [HttpGet]
        [Route("{Id:Guid}")]
        public async Task<ActionResult> GetUserReports([FromRoute] Guid id)
        {
            try
            {
                var reportList = new List<Report>();
                connection = new NpgsqlConnection(Constr);

                connection.Open();

                string query = "select title,description,st_x(geom),st_y(geom),Id" +
                    " from reports where userid=@id;";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {

                    command.Parameters.AddWithValue("@id", id);
                    
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {


                        while (reader.Read())
                        {
                            var report = new Report
                            {
                                
                                reportTitle = reader.IsDBNull(0) ? null : reader.GetString(0),
                                reportDescription = reader.IsDBNull(1) ? null :reader.GetString(1),
                                x = reader.GetDouble(2),
                                y = reader.GetDouble(3),
                                Id = reader.GetInt16(4)
                            };

                            reportList.Add(report);
                        }

                        reader.Close();
                    }
                }
                return Ok(reportList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        [HttpPost]
        //[Route("{Id:Guid}")]
        public async Task<IActionResult> NewReport( Report report)
        {
            try
            {
                connection = new NpgsqlConnection(Constr);

                connection.Open();

                string query = "insert into reports (userid,title,description,geom) values (@userid,@title,@description,st_setsrid( st_makepoint(@x,@y),4326 )) returning id";
                NpgsqlCommand command = new NpgsqlCommand(query, connection);

                _ = command.Parameters.AddWithValue("@userid", report.UserId);
                command.Parameters.AddWithValue("@title", report.reportTitle);
                command.Parameters.AddWithValue("@description", report.reportDescription);
                command.Parameters.AddWithValue("@x", report.x);
                command.Parameters.AddWithValue("@y", report.y);

                Object rid = await command.ExecuteScalarAsync();
                report.Id=Convert.ToInt32(rid);

                return Ok(report);
                 

            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
