using Microsoft.AspNetCore.Mvc;
using System;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Correction;
using NVs.OccupancySensor.CV.Utils;


namespace NVs.OccupancySensor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CorrectionController : ControllerBase
    {
        private readonly ICorrectionStrategyManager manager;
        private readonly ILogger<CorrectionController> logger;
        private readonly IConfiguration config;

        public CorrectionController([NotNull] ICorrectionStrategyManager manager, [NotNull] ILogger<CorrectionController> logger, [NotNull] IConfiguration config)
        {
            this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        [HttpGet("[action]")]
        public bool CanManageStrategy()
        {
            logger.LogDebug("CanManageStrategy called");
            return manager.CanManage;
        }

        [HttpPost("[action]")]
        public void LoadStrategyState()
        {
            logger.LogDebug("LoadStrategyState called");
            manager.LoadState();
        }

        [HttpPost("[action]")]
        public void SaveStrategyState()
        {
            logger.LogDebug("SaveStrategyState called");
            manager.SaveState();
        }

        [HttpPost("[action]")]
        public void ResetStrategyState()
        {
            logger.LogDebug("ResetStrategyState called");
            manager.ResetState();
        }
        
        [HttpGet]
        public Image<Gray, byte> GetMask()
        {
            logger.LogDebug("ResetStrategyState called");
            var maskPath = config.GetStaticMaskSettings().MaskPath;
            if (System.IO.File.Exists(maskPath))
            {
                return new Image<Gray, byte>(maskPath);
            }

            return null;
        }
    }
}
