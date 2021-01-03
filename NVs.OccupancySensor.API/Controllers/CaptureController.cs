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
        private readonly IOccupancySensor sensor;
        private readonly IImageObserver observer;

        private readonly IMatConverter matConverter;
        private readonly ILogger<CaptureController> logger;
        
        public CaptureController([NotNull] ICamera camera, [NotNull] IOccupancySensor sensor, [NotNull] IImageObserver observer, [NotNull] IMatConverter converter, [NotNull] ILogger<CaptureController> logger)
        {
            this.camera = camera ?? throw new ArgumentNullException(nameof(camera));
            this.observer = observer ?? throw new ArgumentNullException(nameof(observer));
            this.matConverter = converter ?? throw new ArgumentException(nameof(converter));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.sensor = sensor ?? throw new ArgumentNullException(nameof(sensor));
        }

        [HttpGet]
        [Produces("image/jpeg")]
        [Route("frame-raw.jpg")]
        public async Task<Image<Rgb,byte>> GetRawFrame()
        {
            logger.LogDebug("GetRawFrame called");

            if (!camera.IsRunning)
            {
                return null;
            }
            
            using (camera.Stream.Select(f => matConverter.Convert(f)).Subscribe(observer))
            {
                return await observer.GetImage();
            }
        }

        [HttpGet]
        [Route("stream-raw.mjpeg")]
        public IActionResult GetRawStream()
        {
            logger.LogDebug("GetRawStream called");

            var unsubscriber = camera.Stream?.Select(f => matConverter.Convert(f))?.Subscribe(observer);

            if (!camera.IsRunning || unsubscriber == null)
            {
                return NoContent();
            }

            return new MjpegStreamContent(
                async cts => (await observer.GetImage())?.ToJpegData(), 
                () => unsubscriber.Dispose());
        }

        [HttpGet]
        [Produces("image/jpeg")]
        [Route("frame.jpg")]
        public async Task<Image<Rgb,byte>> GetFrame()
        {
            logger.LogDebug("GetFrame called");

            if (!sensor.IsRunning)
            {
                return null;
            }

            using (sensor.Stream.Subscribe(observer))
            {
                return await observer.GetImage();
            }
        }

        [HttpGet]
        [Route("stream.mjpeg")]
        public IActionResult GetStream()
        {
            logger.LogDebug("GetStream called");

            var unsubscriber = sensor.Stream?.Subscribe(observer);

            if (!sensor.IsRunning || unsubscriber == null)
            {
                return NoContent();
            }

            return new MjpegStreamContent(
                async cts => (await observer.GetImage())?.ToJpegData(),
                () => unsubscriber.Dispose());
        }
    }
}