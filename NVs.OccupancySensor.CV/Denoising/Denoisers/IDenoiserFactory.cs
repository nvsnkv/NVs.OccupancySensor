using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Denoising.Denoisers
{
    public interface IDenoiserFactory
    {
        IDenoisingStrategy Create([NotNull] string algorithm);

        [NotNull]
        IFastNlMeansColoredDenoisingSettings FastNlMeansColoredDenoisingSettings { get; set; }
    }
}