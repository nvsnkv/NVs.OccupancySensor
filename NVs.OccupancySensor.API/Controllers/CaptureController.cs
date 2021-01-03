using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.API.ActionResults;
using NVs.OccupancySensor.CV.Capture;
using NVs.OccupancySensor.CV.Observervation;
using NVs.OccupancySensor.CV.Transformation;

namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class CaptureController : ControllerBase
    {
        private readonly ICamera camera;
        private readonly IImageObserver observer;
        private readonly IList<IImageTransformer> transformers;

        private readonly ILogger<CaptureController> logger;
        
        public CaptureController([NotNull] ICamera camera, [NotNull] IImageObserver observer, [NotNull] IImageTransformer transformer, [NotNull] ILogger<CaptureController> logger)
        {
            if (transformer == null) throw new ArgumentNullException(nameof(transformer));

            transformers = new List<IImageTransformer>();
            
            while (transformer != null)
            {
                transformers.Add(transformer);
                transformer = transformer.GetPreviousTransformer();
            }
            
            this.camera = camera ?? throw new ArgumentNullException(nameof(camera));
            this.observer = observer ?? throw new ArgumentNullException(nameof(observer));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
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
            
            using (camera.Stream.Subscribe(observer))
            {
                return await observer.GetImage();
            }
        }

        [HttpGet]
        [Route("stream-raw.mjpeg")]
        public IActionResult GetRawStream()
        {
            logger.LogDebug("GetRawStream called");

            var stream = camera.Stream;
            return GetMjpegStreamContent(stream);
        }

        [HttpGet]
        [Route("streams/count")]
        public int GetStreamsCount()
        {
            return transformers.Count;
        }
        
        [HttpGet]
        [Route("stream-{index}.mjpeg")]
        public IActionResult GetStream(int index)
        {
            logger.LogDebug($"GetStream({index}) called");

            var stream = camera.Stream?.Select(transformers[index].Transform);
            return GetMjpegStreamContent(stream);
        }

        private IActionResult GetMjpegStreamContent(IObservable<Image<Rgb,byte>> stream)
        {
            var unsubscriber = stream?.Subscribe(observer);

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