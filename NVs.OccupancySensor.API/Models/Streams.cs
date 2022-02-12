using System;
using JetBrains.Annotations;
using NVs.OccupancySensor.CV.BackgroundSubtraction;
using NVs.OccupancySensor.CV.Capture;
using NVs.OccupancySensor.CV.Correction;
using NVs.OccupancySensor.CV.Denoising;
using NVs.OccupancySensor.CV.Detection;

namespace NVs.OccupancySensor.API.Models
{
    public sealed class Streams
    {
        public Streams(ICamera camera, IDenoiser denoiser, IBackgroundSubtractor subtractor, ICorrector corrector, IPeopleDetector detector)
        {
            Camera = camera ?? throw new ArgumentNullException(nameof(camera));
            Denoiser = denoiser ?? throw new ArgumentNullException(nameof(denoiser));
            Subtractor = subtractor ?? throw new ArgumentNullException(nameof(subtractor));
            Corrector = corrector ?? throw new ArgumentNullException(nameof(corrector));
            Detector = detector ?? throw new ArgumentNullException(nameof(detector));
        }

        public ICamera Camera { get; }

        public IDenoiser Denoiser { get; }

        public IBackgroundSubtractor Subtractor { get; }

        public ICorrector Corrector { get; }

        public IPeopleDetector Detector { get; }
    }
}