using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Detection;

namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class DetectorController : ControllerBase
    {
        private readonly ILogger<DetectorController> logger;
        private readonly IPeopleDetector detector;

        public DetectorController([NotNull] IPeopleDetector detector, ILogger<DetectorController> logger)
        {
            this.detector = detector ?? throw new ArgumentNullException(nameof(detector));
            this.logger = logger;
        }

        [HttpPost]
        [Route("[action]")]
        public void Reset()
        {
            logger.LogInformation("Reset called");
            detector.Reset();
        }
    }
}