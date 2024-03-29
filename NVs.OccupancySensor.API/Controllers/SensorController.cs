﻿using System;
using Microsoft.AspNetCore.Mvc;
using NVs.OccupancySensor.CV.Sense;

namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public sealed class SensorController : ControllerBase
    {
        private readonly IOccupancySensor sensor;

        public SensorController(IOccupancySensor sensor)
        {
            this.sensor = sensor ?? throw new ArgumentNullException(nameof(sensor));
        }


        [HttpGet]
        [Route("[action]")]
        public bool IsRunning()
        {
            return sensor.IsRunning;
        }

        [HttpGet]
        [Route("[action]")]
        public bool? PresenceDetected()
        {
            return sensor.PresenceDetected;
        }

        [HttpPost]
        [Route("[action]")]
        public void Start()
        {
            sensor.Start();
        }

        [HttpPost]
        [Route("[action]")]
        public void Stop()
        {
            sensor.Stop();
        }
    }
}