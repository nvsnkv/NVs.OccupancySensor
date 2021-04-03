namespace NVs.OccupancySensor.CV.Denoising.Denoisers
{
    public interface IFastNlMeansColoredDenoisingSettings : IFastNlMeansDenoisingSettings
    {
        float HColor { get; }
    }
}