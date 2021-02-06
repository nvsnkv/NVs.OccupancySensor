namespace NVs.OccupancySensor.CV.Detection.BackgroundSubtraction.Subtractors
{
    public interface IBackgroundSubtractorFactory
    {
        IBackgroundSubtractor Create(string algorithm);
    }
}