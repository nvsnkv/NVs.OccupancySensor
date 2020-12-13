using System;
using System.Diagnostics;
using Emgu.CV;
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

            return "OK";
        }

        [HttpGet("versionadv")]
        public string VersionAdv()
        {
            _logger.Log(LogLevel.Trace, "VersionAdv called");

            var version = _configuration["Version"];

            var psi = new ProcessStartInfo("sh", "-c \"uname -a\"")
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var proc = new Process() { StartInfo = psi };
            proc.Start();

            var hostInfo = "Host: unable to detect - process is running more then 1 sec";
            if (proc.WaitForExit(1000))
            {
                if (proc.ExitCode == 0)
                {
                    hostInfo = "Host: " + proc.StandardOutput.ReadToEnd();
                }
                else
                {
                    hostInfo = "Host: unable to detect - " + proc.StandardError.ReadToEnd();
                }
            }

            var emguCvBuild = CvInvoke.BuildInformation;

            return $"Version: {version}" + Environment.NewLine + hostInfo + Environment.NewLine + "EmguCV Build information:" + Environment.NewLine + emguCvBuild;
        }
    }
}
