using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Detection.Correction 
{
    public interface ICorrectionStrategyFactory 
    {
        ICorrectionStrategy Create(string name);
        
        [NotNull] IStaticMaskSettings StaticMaskSettings { get; set; }
    }
}