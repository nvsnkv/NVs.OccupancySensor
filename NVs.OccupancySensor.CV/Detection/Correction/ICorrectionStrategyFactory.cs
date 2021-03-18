namespace NVs.OccupancySensor.CV.Detection.Correction 
{
    public interface ICorrectionStrategyFactory 
    {
        ICorrectionStrategy Create(string name);
        
        IStaticMaskSettings StaticMaskSettings { get; }
    }
}