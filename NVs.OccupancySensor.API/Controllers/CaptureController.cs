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
using NVs.OccupancySensor.CV.Observation;
using NVs.OccupancySensor.CV.Transformation;
using NVs.OccupancySensor.CV.Transformation.Grayscale;

namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class CaptureController : ControllerBase
    {
        private readonly ICamera camera;
        private readonly IImageObserver<Rgb> rgbObserver;
        private readonly IImageObserver<Gray> grayObserver;
        private readonly IGrayscaleStreamTransformer streamTransformer;

        private readonly ILogger<CaptureController> logger;
        
        public CaptureController([NotNull] ICamera camera, [NotNull] IImageObserver<Rgb> rgbObserver, [NotNull] IImageObserver<Gray> grayObserver, [NotNull] IGrayscaleStreamTransformer streamTransformer, [NotNull] ILogger<CaptureController> logger)
        {
            this.streamTransformer = streamTransformer ?? throw new ArgumentNullException(nameof(streamTransformer));
            this.camera = camera ?? throw new ArgumentNullException(nameof(camera));
            this.rgbObserver = rgbObserver ?? throw new ArgumentNullException(nameof(rgbObserver));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.grayObserver = grayObserver ?? throw new ArgumentNullException(nameof(grayObserver));
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
            
            using (camera.Stream.Subscribe(rgbObserver))
            {
                return await rgbObserver.GetImage();
            }
        }

        [HttpGet]
        [Route("stream-raw.mjpeg")]
        public IActionResult GetRawStream()
        {
            logger.LogDebug("GetRawStream called");

            var stream = camera.Stream;
            return GetMjpegRgbStreamContent(stream);
        }

        [HttpGet]
        [Route("streams/count")]
        public int GetStreamsCount()
        {
            return streamTransformer.OutputStreams.Count;
        }
        
        [HttpGet]
        [Route("stream-{index}.mjpeg")]
        public IActionResult GetStream(int index)
        {
            logger.LogDebug($"GetStream({index}) called");

            if (!camera.IsRunning) 
            {
                return NoContent();
            }

            if (index < 0) 
            {
                return BadRequest();
            }

            if (index >= streamTransformer.OutputStreams.Count) 
            {
                return NotFound();
            }

            var stream = streamTransformer.OutputStreams[streamTransformer.OutputStreams.Count - 1 - index];
            return GetMjpegGrayStreamContent(stream);
        }

        private IActionResult GetMjpegRgbStreamContent(IObservable<Image<Rgb,byte>> stream)
        {
            var unsubscriber = stream?.Subscribe(rgbObserver);

            if (!camera.IsRunning || unsubscriber == null)
            {
                return NoContent();
            }

            return new MjpegStreamContent(
                async cts => (await rgbObserver.GetImage())?.ToJpegData(),
                () => unsubscriber.Dispose());
        }
        
        private IActionResult GetMjpegGrayStreamContent(IObservable<Image<Gray,byte>> stream)
        {
            var unsubscriber = stream?.Subscribe(grayObserver);

            if (!camera.IsRunning || unsubscriber == null)
            {
                return NoContent();
            }

            return new MjpegStreamContent(
                async cts => (await grayObserver.GetImage())?.ToJpegData(),
                () => unsubscriber.Dispose());
        }
    }
}