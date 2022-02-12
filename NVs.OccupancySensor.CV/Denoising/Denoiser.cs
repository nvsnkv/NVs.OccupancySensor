using System;
using System.ComponentModel;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Denoising.Denoisers;
using NVs.OccupancySensor.CV.Utils;
using NVs.OccupancySensor.CV.Utils.Flow;

namespace NVs.OccupancySensor.CV.Denoising
{
    internal sealed class Denoiser : Stage, IDenoiser
    {
        private readonly IDenoiserFactory factory;

        private IDenoisingSettings settings;

        public Denoiser([NotNull] IDenoiserFactory factory, [NotNull] IDenoisingSettings settings, [NotNull] ILogger<Denoiser> logger):base(logger)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            OutputStream = new DenoisingStream(factory.Create(settings.Algorithm), Counter, CancellationToken.None, logger);
        }

        [NotNull]
        public IDenoisingSettings Settings
        {
            get => settings;
            set
            {
                if (Equals(value ?? throw new ArgumentNullException(nameof(value)), settings)) return;
                settings = value;
            }
        }

        protected override ProcessingStream CreateStream()
        {
            return new DenoisingStream(factory.Create(Settings.Algorithm), Counter, CancellationToken.None, Logger);
        }
    }
}