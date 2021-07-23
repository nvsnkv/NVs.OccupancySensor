using System;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.API.ActionResults;
using NVs.OccupancySensor.API.Models;
using NVs.OccupancySensor.CV.Utils;


namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public sealed class StreamsController : ControllerBase
    {
        [NotNull] private readonly Streams streams;
        [NotNull] private readonly Observers observers;
        [NotNull] private readonly IConfiguration config;
        [NotNull] private readonly ILogger<StreamsController> logger;
        
        public StreamsController([NotNull] Streams streams, [NotNull] Observers observers, [NotNull] ILogger<StreamsController> logger, [NotNull] IConfiguration config)
        {
            this.streams = streams ?? throw new ArgumentNullException(nameof(streams));
            this.observers = observers ?? throw new ArgumentNullException(nameof(observers));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }
        
        [HttpGet]
        [Route("stream-raw.mjpeg")]
        public IActionResult GetRawStream()
        {
            logger.LogDebug("GetRawStream called");

            if (!streams.Camera.IsRunning || !IsStreamingAllowed) 
            {
                return NoContent();
            }

            var stream = streams.Camera.Stream;
            return GetMjpegRgbStreamContent(stream);
        }


        [HttpGet]
        [Route("stream-denoised.mjpeg")]
        public IActionResult GetDenoisedStream()
        {
            logger.LogDebug("GetDenoisedStream called");

            if (!streams.Camera.IsRunning || !IsStreamingAllowed) 
            {
                return NoContent();
            }

            var stream = streams.Denoiser.Output;
            return GetMjpegRgbStreamContent(stream);
        }

        [HttpGet]
        [Route("stream-subtracted.mjpeg")]
        public IActionResult GetSubtractedStream()
        {
            logger.LogDebug("GetSubtractedStream called");

            if (!streams.Camera.IsRunning || !IsStreamingAllowed) 
            {
                return NoContent();
            }

            var stream = streams.Subtractor.Output;
            return GetMjpegGrayStreamContent(stream);
        }

        [HttpGet]
        [Route("stream-corrected.mjpeg")]
        public IActionResult GetCorrectedStream()
        {
            logger.LogDebug("GetCorrectedStream called");

            if (!streams.Camera.IsRunning || !IsStreamingAllowed) 
            {
                return NoContent();
            }

            var stream = streams.Corrector.Output;
            return GetMjpegGrayStreamContent(stream);
        }
        
        [HttpGet]
        [Route("stream.mjpeg")]
        public IActionResult GetStream()
        {
            logger.LogDebug($"GetStream called");

            if (!streams.Camera.IsRunning || !IsStreamingAllowed) 
            {
                return NoContent();
            }

            var stream = streams.Detector.ToObservable(nameof(streams.Detector.Mask), () => streams.Detector.Mask);
            return GetMjpegGrayStreamContent(stream);
        }

        private bool IsStreamingAllowed => bool.TryParse(config["StreamingAllowed"], out var b) && b;

        private IActionResult GetMjpegRgbStreamContent(IObservable<Image<Rgb,byte>> stream)
        {
            var unsubscriber = stream?.Subscribe(observers.Rgb);

            if (!streams.Camera.IsRunning || unsubscriber == null)
            {
                return NoContent();
            }

            return new MjpegStreamContent(
                async cts => (await observers.Rgb.GetImage())?.ToJpegData(),
                () => unsubscriber.Dispose());
        }
        
        private IActionResult GetMjpegGrayStreamContent(IObservable<Image<Gray,byte>> stream)
        {
            var unsubscriber = stream?.Subscribe(observers.Gray);

            if (!streams.Camera.IsRunning || unsubscriber == null)
            {
                return NoContent();
            }

            return new MjpegStreamContent(
                async cts => (await observers.Gray.GetImage())?.ToJpegData(),
                () => unsubscriber.Dispose());
        }
    }
}