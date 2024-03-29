﻿using System;
using System.Text;
using Emgu.CV;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.API.Models;
using NVs.OccupancySensor.API.MQTT;
using NVs.OccupancySensor.CV.Sense;
using NVs.OccupancySensor.CV.Utils;

namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public sealed class HealthcheckController : ControllerBase
    {
        private readonly ILogger<HealthcheckController> logger;
        private readonly IOccupancySensor sensor;
        private readonly IMqttAdapter adapter;
        private readonly Streams streams;

        public HealthcheckController(ILogger<HealthcheckController> logger, IOccupancySensor sensor, IMqttAdapter adapter, Streams streams)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.sensor = sensor ?? throw new ArgumentNullException(nameof(sensor));
            this.adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            this.streams = streams ?? throw new ArgumentNullException(nameof(streams));
        }

        [HttpGet]
        public string Get()
        {
            logger.Log(LogLevel.Trace, "Healthcheck called");
            var ok = adapter.IsRunning;

            return ok ? "OK" : "FAIL";
        }

        [HttpGet("stats")]
        public string Stats()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("MQTT Adapter: {0}", adapter.IsRunning ? "Running" : "Stopped");
            builder.AppendLine();
            builder.AppendFormat("Camera: {0}", streams.Camera.IsRunning ? "Running" : "Stopped");
            builder.AppendLine();
            AppendStage(builder, nameof(streams.Subtractor), streams.Subtractor.Statistics);
            AppendStage(builder, nameof(streams.Denoiser), streams.Denoiser.Statistics);
            AppendStage(builder, nameof(streams.Corrector), streams.Corrector.Statistics);
            builder.AppendFormat("Sensor: {0}", sensor.IsRunning ? "Running" : "Stopped");
            builder.AppendLine();

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

            var hostVersion = typeof(HealthcheckController).Assembly.GetName().Version;
            var cvlibVersion  = typeof(IOccupancySensor).Assembly.GetName().Version;
            var emguVersion = typeof(CvInvoke).Assembly.GetName().Version;

            return $"App version: {cvlibVersion}{Environment.NewLine}Host version: {hostVersion}{Environment.NewLine}EmguCV version: {emguVersion}";
        }

        [HttpGet("emgucvbuildinfo")]
        public string EmguCvBuildInfo()
        {
            var emguCvBuild = CvInvoke.BuildInformation;

            return $"EmguCV Build information:{Environment.NewLine}{emguCvBuild}";
        }
    }
}
