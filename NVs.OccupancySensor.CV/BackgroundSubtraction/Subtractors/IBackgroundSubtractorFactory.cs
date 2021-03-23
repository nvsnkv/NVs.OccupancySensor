namespace NVs.OccupancySensor.CV.BackgroundSubtraction.Subtractors
{
    public interface IBackgroundSubtractorFactory
    {
        ISubtractionStrategy Create(string algorithm);
    }
}