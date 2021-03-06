namespace NVs.OccupancySensor.CV.Denoising.Denoisers
{
    public interface IFastNlMeansColoredDenoisingSettings
    {
        float H { get; }

        float HColor { get; }

        int TemplateWindowSize { get; }

        int SearchWindowSize { get; }
    }
}