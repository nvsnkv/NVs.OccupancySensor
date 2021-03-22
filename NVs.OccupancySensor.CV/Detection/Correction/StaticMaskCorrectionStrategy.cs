using System;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Detection.Correction
{
    sealed class StaticMaskCorrectionStrategy : IStatefulCorrectionStrategy
    {
        [NotNull] private readonly IStaticMaskSettings settings;

        [NotNull] private volatile Image<Gray, byte> mask = new Image<Gray, byte>(1, 1);


        public StaticMaskCorrectionStrategy([NotNull] IStaticMaskSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public Image<Gray, byte> Apply([NotNull] Image<Gray, byte> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.Width != mask.Width || source.Height != mask.Height)
                throw new ArgumentException("Dimensions of source and mask does not match!", nameof(source))
                {
                    Data =
                    {
                        {"Source", source.Size},
                        {"Mask", mask.Size}
                    }
                };

            var result = new Image<Gray, byte>(mask.Width, mask.Height);
            CvInvoke.BitwiseAnd(source, mask, result);
            return result;
        }

        public void Load()
        {
            mask = new Image<Gray, byte>(settings.MaskPath);
        }

        public void Save()
        {
            mask.Save(settings.MaskPath);
        }

        public void Reset()
        {
            throw new System.NotImplementedException();
        }
    }
}