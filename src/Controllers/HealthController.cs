#nullable enable
using Microsoft.AspNetCore.Mvc;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Utils;

namespace DotnetMicroOrm.Controllers
{
    /// <summary>
    /// Controller for health checks.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public sealed class HealthController : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HealthController"/> class.
        /// </summary>
        /// <param name="_dbContext">The database context.</param>
        private readonly IDatabaseContext _dbContext;

        public HealthController(IDatabaseContext _dbContext)
        {
            this._dbContext = _dbContext;
        }

        /// <summary>
        /// Returns a success response indicating the service is healthy.
        /// </summary>
        /// <returns>A successful HTTP response.</returns>
        [HttpGet]
        public IActionResult GetHealth()
        {
            return Ok(ApiResponse.CreateSuccess(new { status = "healthy" }, "Service is healthy"));
        }

        /// <summary>
        /// Returns a success or error response indicating the service is ready or not.
        /// </summary>
        /// <returns>A successful or error HTTP response.</returns>
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
