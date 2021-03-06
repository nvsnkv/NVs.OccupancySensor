﻿using System;
using System.Diagnostics;
using System.Text;
using Emgu.CV;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.API.MQTT;
using NVs.OccupancySensor.CV.Sense;

namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class HealthcheckController : ControllerBase
    {
        private readonly ILogger<HealthcheckController> logger;
        private readonly IConfiguration configuration;
        private readonly IOccupancySensor sensor;
        private readonly IMqttAdapter adapter;

        public HealthcheckController([NotNull] ILogger<HealthcheckController> logger, [NotNull] IConfiguration configuration, [NotNull] IOccupancySensor sensor, [NotNull] IMqttAdapter adapter)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.sensor = sensor ?? throw new ArgumentNullException(nameof(sensor));
            this.adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        [HttpGet]
        public string Get()
        {
            logger.Log(LogLevel.Trace, "Healthcheck called");
            var builder = new StringBuilder();
            builder.AppendFormat("Sensor: {0}{1}", sensor.IsRunning ? "Running" : "Stopped", Environment.NewLine);
            builder.AppendFormat("MQTT Adapter: {0}{1}", adapter.IsRunning ? "Running" : "Stopped", Environment.NewLine);

            return builder.ToString();
        }

        [HttpGet("versionadv")]
        public string VersionAdv()
        {
            logger.Log(LogLevel.Trace, "VersionAdv called");

            var version = configuration["Version"];

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
