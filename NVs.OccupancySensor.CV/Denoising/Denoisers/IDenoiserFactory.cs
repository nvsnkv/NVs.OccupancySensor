using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Denoising.Denoisers
{
    public interface IDenoiserFactory
    {
        IDenoisingStrategy Create(string algorithm);

                IFastNlMeansColoredDenoisingSettings FastNlMeansColoredDenoisingSettings { get; set; }

                IMedianBlurSettings MedianBlurSettings { get; set; }
    }
}