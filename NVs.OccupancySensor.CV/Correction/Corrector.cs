using System;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Utils.Flow;

namespace NVs.OccupancySensor.CV.Correction
{
    sealed class Corrector : Stage<Image<Gray, byte>, Image<Gray, byte>>, ICorrector
    {
        private readonly ICorrectionStrategyFactory factory;
        [NotNull] private ICorrectionSettings settings;

        public Corrector([NotNull] ICorrectionStrategyFactory factory, [NotNull] ICorrectionSettings settings, [NotNull] ILogger<Corrector> logger) : base(logger)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            OutputStream = CreateStream();
        }

        protected override ProcessingStream<Image<Gray, byte>, Image<Gray, byte>> CreateStream()
        {
            return new CorrectionStream(factory.Create(Settings.Algorithm), Counter, CancellationToken.None, Logger);
        }
        
        [NotNull]
        public ICorrectionSettings Settings
        {
            get => settings;
            set => settings = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}