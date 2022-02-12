using Microsoft.AspNetCore.Mvc;
using System;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Correction;
using NVs.OccupancySensor.CV.Utils;


namespace NVs.OccupancySensor.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CorrectionMaskController : ControllerBase
    {
        private readonly ICorrectionStrategyManager manager;
        private readonly ILogger<CorrectionMaskController> logger;
        private readonly IConfiguration config;

        public CorrectionMaskController(ICorrectionStrategyManager manager, ILogger<CorrectionMaskController> logger, IConfiguration config)
        {
            this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        
        [HttpPost("[action]")]
        public void Load()
        {
            logger.LogDebug("Load called");
            if (manager.CanManage)
            {
                manager.LoadState();
            }
        }

        [HttpPost("[action]")]
        public void Save()
        {
            logger.LogDebug("Save called");
            if (manager.CanManage)
            {
                manager.SaveState();
            }
        }

        [HttpDelete]
        public void Reset()
        {
            logger.LogDebug("Reset called");
            if (manager.CanManage)
            {
                manager.ResetState();
            }
        }
        
        [HttpGet]
        public Image<Gray, byte>? GetMask()
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
