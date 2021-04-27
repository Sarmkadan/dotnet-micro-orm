using Microsoft.AspNetCore.Mvc;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Utils;

namespace DotnetMicroOrm.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IDatabaseContext _dbContext;

        public HealthController(IDatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetHealth()
        {
            return Ok(ApiResponse.CreateSuccess(new { status = "healthy" }, "Service is healthy"));
        }

        [HttpGet("ready")]
        public async Task<IActionResult> GetReady()
        {
            try
            {
                await _dbContext.TestConnectionAsync();
                return Ok(ApiResponse.CreateSuccess(new { status = "ready" }, "Service is ready"));
            }
            catch (Exception ex)
            {
                return StatusCode(503, ApiResponse.CreateError("Database connection failed", "DB_CONNECTION_FAILED", new { status = "unhealthy" }));
            }
        }
    }
}