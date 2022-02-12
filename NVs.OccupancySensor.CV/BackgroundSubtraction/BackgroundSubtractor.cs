using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.BackgroundSubtraction.Subtractors;
using NVs.OccupancySensor.CV.Utils.Flow;

namespace NVs.OccupancySensor.CV.BackgroundSubtraction
{
    internal sealed class BackgroundSubtractor : Stage, IBackgroundSubtractor
    {
        private readonly IBackgroundSubtractorFactory factory;

        public BackgroundSubtractor(IBackgroundSubtractorFactory factory,IBackgroundSubtractorSettings settings, ILogger<BackgroundSubtractor> logger): base(logger)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Settings = settings;
            OutputStream = new BackgroundSubtractionStream(factory.Create(Settings.Algorithm), Counter, CancellationToken.None, Logger);
        }

        protected override ProcessingStream CreateStream()
        {
            var strategy = factory.Create(Settings.Algorithm);
            return new BackgroundSubtractionStream(strategy, Counter, CancellationToken.None, Logger);
        }

        public IBackgroundSubtractorSettings Settings { get; set; }
    }
}