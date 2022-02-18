using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.API.ActionFilters;
using NVs.OccupancySensor.API.Models;

namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class FrameController : ControllerBase
    {
        private readonly Streams streams;
        private readonly Observers observers;
        private readonly ILogger<StreamsController> logger;

        public FrameController(Streams streams, Observers observers, ILogger<StreamsController> logger)
        {
            this.streams = streams;
            this.observers = observers;
            this.logger = logger;
        }

        [HttpGet]
        [IfStreamingAllowed(Streaming.Enabled)]
        [Produces("image/jpeg")]
        [Route("frame-denoised.jpg")]
        public async Task<Image<Gray, byte>?> GetDenoisedFrame(CancellationToken ct)
        {
            logger.LogDebug("GetDenoisedFrame called");

            if (!streams.Camera.IsRunning)
            {
                return null;
            }
            
            using (streams.Denoiser.Output.Subscribe(observers.Gray))
            {
                return await observers.Gray.GetImage(ct);
            }
        }

        [HttpGet]
        [IfStreamingAllowed(Streaming.Enabled)]
        [Produces("image/jpeg")]
        [Route("frame-subtracted.jpg")]
        public async Task<Image<Gray,byte>?> GetSubtractedFrame(CancellationToken ct)
        {
            logger.LogDebug("GetSubtractedFrame called");

            if (!streams.Camera.IsRunning)
            {
                return null;
            }
            
            using (streams.Subtractor.Output.Subscribe(observers.Gray))
            {
                return await observers.Gray.GetImage(ct);
            }
        }

        [HttpGet]
        [IfStreamingAllowed(Streaming.OnlyFinal)]
        [Produces("image/jpeg")]
        [Route("frame-corrected.jpg")]
        public async Task<Image<Gray, byte>?> GetCorrectedFrame(CancellationToken ct)
        {
            logger.LogDebug("GetCorrectedFrame called");

            if (!streams.Camera.IsRunning)
            {
                return null;
            }
            
            using (streams.Corrector.Output.Subscribe(observers.Gray))
            {
                return await observers.Gray.GetImage(ct);
            }
        }

        
        [HttpGet]
        [IfStreamingAllowed(Streaming.Enabled)]
        [Produces("image/jpeg")]
        [Route("frame-raw.jpg")]
        public async Task<Image<Gray, byte>?> GetRawFrame(CancellationToken ct)
        {
            logger.LogDebug("GetRawFrame called");

            if (!streams.Camera.IsRunning)
            {
                return null;
            }
            
            using (streams.Camera.Stream.Subscribe(observers.Gray))
            {
                return await observers.Gray.GetImage(ct);
            }
        }
    }
}