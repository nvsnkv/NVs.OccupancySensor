namespace NVs.OccupancySensor.CV.Detection.BackgroundSubtraction.Subtractors
{
    public interface IBackgroundSubtractorFactory
    {
        ISubtractionStrategy Create(string algorithm);
    }
}