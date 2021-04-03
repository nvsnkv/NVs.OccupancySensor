using NVs.OccupancySensor.CV.Denoising.Denoisers;

namespace NVs.OccupancySensor.CV.Settings.Denoising
{
    public sealed class MedianBlurDenoisingSettings : IMedianBlurSettings
    {
        public MedianBlurDenoisingSettings(int k)
        {
            K = k;
        }

        public int K { get; }

        public static MedianBlurDenoisingSettings Default = new MedianBlurDenoisingSettings(3);
    }
}