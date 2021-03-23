using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Correction 
{
    public interface ICorrectionStrategyFactory 
    {
        ICorrectionStrategy Create(string name);
        
        [NotNull] IStaticMaskSettings StaticMaskSettings { get; set; }
    }
}