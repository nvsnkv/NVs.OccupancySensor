using System;
using System.ComponentModel;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Denoising.Denoisers;
using NVs.OccupancySensor.CV.Utils.Flow;

namespace NVs.OccupancySensor.CV.Denoising
{
    internal sealed class Denoiser : Stage<Image<Rgb, byte>, Image<Rgb, byte>>, IDenoiser
    {
        private readonly IDenoiserFactory factory;

        private IDenoisingSettings settings;

        public Denoiser([NotNull] IDenoiserFactory factory, [NotNull] IDenoisingSettings settings, [NotNull] ILogger<Denoiser> logger)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            OutputStream = new DenoisingStream(factory.Create(settings.Algorithm), CancellationToken.None, logger);
        }

        public override event PropertyChangedEventHandler PropertyChanged;

        [NotNull]
        public IDenoisingSettings Settings
        {
            get => settings;
            set
            {
                if (Equals(value ?? throw new ArgumentNullException(nameof(value)), settings)) return;
                settings = value;
                OnPropertyChanged();
            }
        }

        public void Reset()
        {
            var stream = CreateStream();
            ReplaceStream(OutputStream, stream);
        }

        protected override ProcessingStream<Image<Rgb, byte>, Image<Rgb, byte>> CreateStream()
        {
            return new DenoisingStream(factory.Create(Settings.Algorithm), CancellationToken.None, Logger);
        }
    }
}