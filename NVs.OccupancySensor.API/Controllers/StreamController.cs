using System;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.API.ActionFilters;
using NVs.OccupancySensor.API.ActionResults;
using NVs.OccupancySensor.API.Models;
using NVs.OccupancySensor.CV.Utils;


namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public sealed class StreamsController : ControllerBase
    {
        private readonly Streams streams;
        private readonly Observers observers;
        private readonly IConfiguration config;
        private readonly ILogger<StreamsController> logger;

        public StreamsController(Streams streams, Observers observers, ILogger<StreamsController> logger, IConfiguration config)
        {
            this.streams = streams ?? throw new ArgumentNullException(nameof(streams));
            this.observers = observers ?? throw new ArgumentNullException(nameof(observers));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        [HttpGet]
        [IfStreamingAllowed]
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
        [IfStreamingAllowed]
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
        [IfStreamingAllowed]
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
        [IfStreamingAllowed]
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
        [IfStreamingAllowed]
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

        private IActionResult GetMjpegRgbStreamContent(IObservable<Image<Gray, byte>?>? stream)
        {
            var unsubscriber = stream?.Subscribe(observers.Gray);

            if (!streams.Camera.IsRunning || unsubscriber == null)
            {
                return NoContent();
            }

            return new MjpegStreamContent(
                async ct => (await observers.Gray.GetImage(ct))?.ToJpegData(),
                () => unsubscriber.Dispose());
        }

        private IActionResult GetMjpegGrayStreamContent(IObservable<Image<Gray, byte>?>? stream)
        {
            var unsubscriber = stream?.Subscribe(observers.Gray);

            if (!streams.Camera.IsRunning || unsubscriber == null)
            {
                return NoContent();
            }

            return new MjpegStreamContent(
                async ct => (await observers.Gray.GetImage(ct))?.ToJpegData(),
                () => unsubscriber.Dispose());
        }

        private bool IsStreamingAllowed => bool.TryParse(config["StreamingAllowed"], out var b) && b;
    }
}