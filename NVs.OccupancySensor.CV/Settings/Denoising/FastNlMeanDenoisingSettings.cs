using NVs.OccupancySensor.CV.Denoising;

namespace NVs.OccupancySensor.CV.Settings.Denoising 
{
    public sealed class FastNlMeanDenoisingSettings : IFastNlMeansDenoisingSettings
    {
        public FastNlMeanDenoisingSettings(float h, float hColor, int templateWindowSize, int searchWindowSize)
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

        public static FastNlMeanDenoisingSettings Default { get; } = new FastNlMeanDenoisingSettings(3f, 3f, 7, 21);
    }
}
