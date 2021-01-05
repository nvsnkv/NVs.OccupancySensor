using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Transformation.Background;

namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("controller")]
    public sealed class BackgroundSubtractionController : ControllerBase
    {
        private readonly ILogger<BackgroundSubtractionController> logger;
        private readonly IBackgroundSubtraction subtraction;

        public BackgroundSubtractionController([NotNull] IBackgroundSubtraction subtraction, ILogger<BackgroundSubtractionController> logger)
        {
            this.subtraction = subtraction ?? throw new ArgumentNullException(nameof(subtraction));
            this.logger = logger;
        }

        [HttpPost]
        
        public void ResetModel()
        {
            logger.LogInformation("Reset called");
            subtraction.ResetModel();
        }
    }
}