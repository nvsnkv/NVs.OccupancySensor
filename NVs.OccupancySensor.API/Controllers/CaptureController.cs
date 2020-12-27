using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.API.ActionResults;
using NVs.OccupancySensor.CV;

namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class CaptureController : ControllerBase
    {
        private readonly ICamera camera;
        private readonly IImageObserver observer;
        private readonly ILogger<CaptureController> logger;
        
        public CaptureController([NotNull] ICamera camera, IImageObserver observer, ILogger<CaptureController> logger)
        {
            this.camera = camera ?? throw new ArgumentNullException(nameof(camera));
            this.observer = observer ?? throw new ArgumentNullException(nameof(observer));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Produces("image/jpeg")]
        [Route("frame.jpg")]
        public async Task<Image<Rgb,int>> GetSingleCapture()
        {
            logger.LogDebug("GetSingleCapture called");

            if (!camera.IsRunning)
            {
                return null;
            }
            
            using (camera.Stream.Subscribe(observer))
            {
                return await observer.GetImage();
            }
        }

        [HttpGet]
        [Route("stream.mjpeg")]
        public IActionResult GetStream()
        {
            logger.LogDebug("GetStream called");

            var unsubscriber = camera.Stream?.Subscribe(observer);

            if (!camera.IsRunning || unsubscriber == null)
            {
                return NoContent();
            }

            return new MjpegStreamContent(
                async cts => (await observer.GetImage())?.ToJpegData(), 
                () => unsubscriber.Dispose());
        }
    }
}