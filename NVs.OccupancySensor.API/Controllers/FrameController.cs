using System;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.API.Models;

namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FrameController : ControllerBase
    {
        [NotNull] private readonly Streams streams;
        [NotNull] private readonly Observers observers;

        private readonly ILogger<StreamsController> logger;
        
        public FrameController([NotNull] Streams streams, [NotNull] Observers observers, [NotNull] ILogger<StreamsController> logger)
        {
            this.streams = streams ?? throw new ArgumentNullException(nameof(streams));
            this.observers = observers ?? throw new ArgumentNullException(nameof(observers));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Produces("image/jpeg")]
        [Route("frame-denoised.jpg")]
        public async Task<Image<Rgb,byte>> GetDenoisedFrame()
        {
            logger.LogDebug("GetDenoisedFrame called");

            if (!streams.Camera.IsRunning)
            {
                return null;
            }
            
            using (streams.Denoiser.Output.Subscribe(observers.Rgb))
            {
                return await observers.Rgb.GetImage();
            }
        }

        [HttpGet]
        [Produces("image/jpeg")]
        [Route("frame-subtracted.jpg")]
        public async Task<Image<Gray,byte>> GetSubtractedFrame()
        {
            logger.LogDebug("GetSubtractedFrame called");

            if (!streams.Camera.IsRunning)
            {
                return null;
            }
            
            using (streams.Subtractor.Output.Subscribe(observers.Gray))
            {
                return await observers.Gray.GetImage();
            }
        }

        [HttpGet]
        [Produces("image/jpeg")]
        [Route("frame-corrected.jpg")]
        public async Task<Image<Gray,byte>> GetCorrectedFrame()
        {
            logger.LogDebug("GetCorrectedFrame called");

            if (!streams.Camera.IsRunning)
            {
                return null;
            }
            
            using (streams.Corrector.Output.Subscribe(observers.Gray))
            {
                return await observers.Gray.GetImage();
            }
        }

        
        [HttpGet]
        [Produces("image/jpeg")]
        [Route("frame-raw.jpg")]
        public async Task<Image<Rgb,byte>> GetRawFrame()
        {
            logger.LogDebug("GetRawFrame called");

            if (!streams.Camera.IsRunning)
            {
                return null;
            }
            
            using (streams.Camera.Stream.Subscribe(observers.Rgb))
            {
                return await observers.Rgb.GetImage();
            }
        }
    }
}