using NVs.OccupancySensor.CV.Denoising;
using NVs.OccupancySensor.CV.Denoising.Denoisers;

namespace NVs.OccupancySensor.CV.Settings.Denoising 
{
    public sealed class FastNlMeansDenoisingSettings : IFastNlMeansDenoisingSettings
    {
        public FastNlMeansDenoisingSettings(float h, float hColor, int templateWindowSize, int searchWindowSize)
        {
            H = h;
            HColor = hColor;
            TemplateWindowSize = templateWindowSize;
            SearchWindowSize = searchWindowSize;
        }

        public float H { get; }

        public float HColor { get; }

        public int TemplateWindowSize { get; }

        public int SearchWindowSize { get; }

        public static FastNlMeansDenoisingSettings Default { get; } = new FastNlMeansDenoisingSettings(3f, 3f, 7, 21);
    }
}
