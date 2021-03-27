using Microsoft.AspNetCore.Mvc;
using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Correction;

namespace NVs.OccupancySensor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CorrectionController : ControllerBase
    {
        private readonly ICorrectionStrategyManager manager;
        private readonly ILogger<CorrectionController> logger;

        public CorrectionController([NotNull] ICorrectionStrategyManager manager, [NotNull] ILogger<CorrectionController> logger)
        {
            this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    }
}
