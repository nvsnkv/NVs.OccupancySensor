using System;
using Emgu.CV;
using Emgu.CV.BgSegm;
using Emgu.CV.Structure;
using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Detection.BackgroundSubtraction.Subtractors
{
    internal sealed class CNTSubtractor : IBackgroundSubtractor
    {
        private readonly BackgroundSubtractorCNT subtractor;

        public CNTSubtractor([NotNull] ICNTSubtractorSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            subtractor = new BackgroundSubtractorCNT(settings.MinPixelStability, settings.UseHistory, settings.MaxPixelStability, settings.IsParallel);
        }

        public Image<Gray, byte> GetForegroundMask([NotNull] Image<Rgb, byte> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var mask = new Image<Gray, byte>(source.Width, source.Height);
            subtractor.Apply(source, mask);

            return mask;
        }

        public void Dispose()
        {
            subtractor.Dispose();
        }
    }
}