using System;
using System.IO;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.API.CV;

namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class CaptureController : ControllerBase
    {
        private readonly IObservable<Mat> camera;
        private readonly ILogger<CaptureController> logger;
        
        public CaptureController(IObservable<Mat> camera, ILogger<CaptureController> logger)
        {
            this.camera = camera;
            this.logger = logger;
        }

        [HttpGet]
        [Produces("image/jpeg")]
        public async Task<Image<Rgb,int>> GetCapture()
        {
            logger.LogDebug("GetCapture called");

            var observer = new RawImageObserver(logger);
            using (camera.Subscribe(observer))
            {
                return await observer.GetImage();
            }
        }
    }
}