﻿using System;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ILogger<StreamsController> logger;

        public StreamsController(Streams streams, Observers observers, ILogger<StreamsController> logger)
        {
            this.streams = streams;
            this.observers = observers;
            this.logger = logger;
        }

        [HttpGet]
        [IfStreamingAllowed(Streaming.Enabled)]
        [Route("stream-raw.mjpeg")]
        public IActionResult GetRawStream()
        {
            logger.LogDebug("GetRawStream called");

            if (!streams.Camera.IsRunning)
            {
                return NoContent();
            }

            var stream = streams.Camera.Stream;
            return GetMjpegRgbStreamContent(stream);
        }

        [HttpGet]
        [IfStreamingAllowed(Streaming.Enabled)]
        [Route("stream-denoised.mjpeg")]
        public IActionResult GetDenoisedStream()
        {
            logger.LogDebug("GetDenoisedStream called");

            if (!streams.Camera.IsRunning)
            {
                return NoContent();
            }

            var stream = streams.Denoiser.Output;
            return GetMjpegRgbStreamContent(stream);
        }

        [HttpGet]
        [IfStreamingAllowed(Streaming.Enabled)]
        [Route("stream-subtracted.mjpeg")]
        public IActionResult GetSubtractedStream()
        {
            logger.LogDebug("GetSubtractedStream called");

            if (!streams.Camera.IsRunning)
            {
                return NoContent();
            }

            var stream = streams.Subtractor.Output;
            return GetMjpegGrayStreamContent(stream);
        }

        [HttpGet]
        [IfStreamingAllowed(Streaming.OnlyFinal)]
        [Route("stream-corrected.mjpeg")]
        public IActionResult GetCorrectedStream()
        {
            logger.LogDebug("GetCorrectedStream called");

            if (!streams.Camera.IsRunning)
            {
                return NoContent();
            }

            var stream = streams.Corrector.Output;
            return GetMjpegGrayStreamContent(stream);
        }

        [HttpGet]
        [IfStreamingAllowed(Streaming.OnlyFinal)]
        [Route("stream.mjpeg")]
        public IActionResult GetStream()
        {
            logger.LogDebug($"GetStream called");

            if (!streams.Camera.IsRunning)
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
    }
}