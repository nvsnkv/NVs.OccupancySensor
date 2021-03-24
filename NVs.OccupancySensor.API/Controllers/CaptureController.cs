using System;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.API.ActionResults;
using NVs.OccupancySensor.CV.BackgroundSubtraction;
using NVs.OccupancySensor.CV.Capture;
using NVs.OccupancySensor.CV.Denoising;
using NVs.OccupancySensor.CV.Detection;
using NVs.OccupancySensor.CV.Observation;
using NVs.OccupancySensor.CV.Utils;


namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class CaptureController : ControllerBase
    {
        private readonly ICamera camera;
        [NotNull] private readonly IDenoiser denoiser;
        private readonly IPeopleDetector detector;
        private readonly IImageObserver<Rgb> rgbObserver;
        private readonly IImageObserver<Gray> grayObserver;

        private readonly ILogger<CaptureController> logger;
        
        public CaptureController([NotNull] ICamera camera, [NotNull] IDenoiser denoiser, [NotNull] IPeopleDetector detector, [NotNull] IImageObserver<Rgb> rgbObserver, [NotNull] IImageObserver<Gray> grayObserver, [NotNull] ILogger<CaptureController> logger)
        {
            this.camera = camera ?? throw new ArgumentNullException(nameof(camera));
            this.denoiser = denoiser ?? throw new ArgumentNullException(nameof(denoiser));
            this.detector = detector ?? throw new ArgumentNullException(nameof(detector));
            this.rgbObserver = rgbObserver ?? throw new ArgumentNullException(nameof(rgbObserver));
            this.grayObserver = grayObserver ?? throw new ArgumentNullException(nameof(grayObserver));
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
        [Produces("image/jpeg")]
        [Route("frame-denoised.jpg")]
        public async Task<Image<Rgb,byte>> GetDenoisedFrame()
        {
            logger.LogDebug("GetDenoisedFrame called");

            if (!camera.IsRunning)
            {
                return null;
            }
            
            using (denoiser.Output.Subscribe(rgbObserver))
            {
                return await rgbObserver.GetImage();
            }
        }

        [HttpGet]
        [Route("stream-denoised.mjpeg")]
        public IActionResult GetDenoisedStream()
        {
            logger.LogDebug("GetDenoisedStream called");

            var stream = denoiser.Output;
            return GetMjpegRgbStreamContent(stream);
        }
        
        [HttpGet]
        [Route("stream.mjpeg")]
        public IActionResult GetStream()
        {
            logger.LogDebug($"GetStream called");

            if (!camera.IsRunning) 
            {
                return NoContent();
            }

            var stream = detector.ToObservable(nameof(detector.Mask), () => detector.Mask);
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