using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using NVs.OccupancySensor.CV;

namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class CameraController : ControllerBase
    {
        private readonly ICamera camera;

        public CameraController([NotNull] ICamera camera)
        {
            this.camera = camera ?? throw new ArgumentNullException(nameof(camera));
        }
        
        [HttpGet]
        [Route("[action]")]
        public bool IsRunning()
        {
            return camera.IsRunning;
        }

        [HttpPost]
        [Route("[action]")]
        public void Start()
        {
            camera.Start();
        }

        [HttpPost]
        [Route("[action]")]
        public void Stop()
        {
            camera.Stop();
        }
    }
}