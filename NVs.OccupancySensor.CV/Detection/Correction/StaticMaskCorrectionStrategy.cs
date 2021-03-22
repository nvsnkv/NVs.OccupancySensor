using System;
using System.Drawing;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Detection.Correction
{
    sealed class StaticMaskCorrectionStrategy : IStatefulCorrectionStrategy
    {
        [NotNull] private readonly IStaticMaskSettings settings;
        private readonly ReaderWriterLockSlim maskLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private volatile Image<Gray, byte> mask;
        private volatile Image<Gray, byte> adjusted;
        
        public StaticMaskCorrectionStrategy([NotNull] IStaticMaskSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public Image<Gray, byte> Apply([NotNull] Image<Gray, byte> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var maskCopy = Mask;

            if (maskCopy == null)
            {
                ResetMask(source.Size);
                maskCopy = Mask;
            }

            if (source.Size != maskCopy.Size)
                throw new ArgumentException("Dimensions of source and mask does not match!", nameof(source))
                {
                    Data =
                    {
                        {"Source", source.Size},
                        {"Mask", maskCopy.Size}
                    }
                };

            var result = new Image<Gray, byte>(maskCopy.Width, maskCopy.Height);
            CvInvoke.BitwiseAnd(source, maskCopy, result);

            if (IsLearning)
            {
                AdjustMask(source);
            }

            return result;
        }

        public void Load()
        {
            Mask = new Image<Gray, byte>(settings.MaskPath);
        }

        public void Save()
        {
            Mask.Save(settings.MaskPath);
            adjusted = null;
        }

        public void Reset()
        {
            var copy = Mask;
            if (copy == null) throw new InvalidOperationException("Unable to reset mask if previous mask was not defined!");
            ResetMask(copy.Size);
            AdjustMask(Mask);
        }

        private Image<Gray, byte> Mask
        {
            get
            {
                maskLock.EnterReadLock();
                var val = mask;
                maskLock.ExitReadLock();
                return val;
            }

            set
            {
                maskLock.EnterWriteLock();
                mask = value;
                maskLock.ExitWriteLock();
            }
        }

        private bool IsLearning => adjusted != null;
        private void ResetMask(Size size)
        {
            var image = new Image<Gray, byte>(size);
            image.SetValue(new Gray(255));

            Mask = image;
        }

        private void AdjustMask(Image<Gray, byte> source)
        {
            if (adjusted == null)
            {
                adjusted = new Image<Gray, byte>(source.Size);
            }
            
            adjusted = adjusted.AddWeighted(source, 0.95d, 0.05d, 0d);
            Mask = adjusted.ThresholdBinaryInv(new Gray(200), new Gray(255));
        }
    }
}