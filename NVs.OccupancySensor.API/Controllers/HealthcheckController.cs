using System;
using System.Text;
using Emgu.CV;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.API.Models;
using NVs.OccupancySensor.API.MQTT;
using NVs.OccupancySensor.CV.Sense;
using NVs.OccupancySensor.CV.Utils;

namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class HealthcheckController : ControllerBase
    {
        [NotNull] private readonly ILogger<HealthcheckController> logger;
        [NotNull] private readonly IConfiguration configuration;
        [NotNull] private readonly IOccupancySensor sensor;
        [NotNull] private readonly IMqttAdapter adapter;
        [NotNull] private readonly Streams streams;

        public HealthcheckController([NotNull] ILogger<HealthcheckController> logger, [NotNull] IConfiguration configuration, [NotNull] IOccupancySensor sensor, [NotNull] IMqttAdapter adapter, [NotNull] Streams streams)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.sensor = sensor ?? throw new ArgumentNullException(nameof(sensor));
            this.adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            this.streams = streams ?? throw new ArgumentNullException(nameof(streams));
        }

        [HttpGet]
        public string Get()
        {
            logger.Log(LogLevel.Trace, "Healthcheck called");
            var ok = streams.Camera.Stream != null && streams.Denoiser.Output != null &&
                     streams.Subtractor.Output != null && streams.Corrector.Output != null;

            return ok ? "OK" : "FAIL";
        }

        [HttpGet("stats")]
        public string Stats()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Camera: {0}", streams.Camera.IsRunning ? "Running" : "Stopped");
            builder.AppendLine();
            AppendStage(builder, nameof(streams.Denoiser), streams.Denoiser.Statistics);
            AppendStage(builder, nameof(streams.Subtractor), streams.Subtractor.Statistics);
            AppendStage(builder, nameof(streams.Corrector), streams.Corrector.Statistics);
            builder.AppendFormat("Sensor: {0}", sensor.IsRunning ? "Running" : "Stopped");
            builder.AppendLine();
            builder.AppendFormat("MQTT Adapter: {0}", adapter.IsRunning ? "Running" : "Stopped");
            
            return builder.ToString();
        }

        private void AppendStage(StringBuilder builder, string stageName, IStatistics statistics)
        {
            builder.Append($"{stageName}: processed {statistics.ProcessedFrames} frames, dropped {statistics.DroppedFrames} frames, {statistics.Errors} errors occurred.");
            builder.AppendLine();
        }

        [HttpGet("versionadv")]
        public string VersionAdv()
        {
            logger.Log(LogLevel.Trace, "VersionAdv called");

            var version = configuration["Version"];
            var apiVersion = configuration["ApiVersion"];
            var emguVersion = typeof(CvInvoke).Assembly.GetName().Version;

            return $"App version: {version}{Environment.NewLine}API version: {apiVersion}{Environment.NewLine}EmguCV version: {emguVersion}";
        }

        [HttpGet("emgucvbuildinfo")]
        public string EmguCvBuildInfo()
        {
            var emguCvBuild = CvInvoke.BuildInformation;

            return $"EmguCV Build information:{Environment.NewLine}{emguCvBuild}";
        }
    }
}
