using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthcheckController : ControllerBase
    {
        private readonly ILogger<HealthcheckController> _logger;
        private readonly IConfiguration _configuration;

        public HealthcheckController(ILogger<HealthcheckController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public string Get()
        {
            _logger.Log(LogLevel.Trace, "Healthcheck called");
            return $"OK (API version: {_configuration["Version"]})";
        }
    }
}
