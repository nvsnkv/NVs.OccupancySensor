using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Detection.BackgroundSubtraction
{
    public interface IDecisionMaker
    {
        bool PresenceDetected (Image<Gray, byte> mask);

        IDecisionMakerSettings Settings { get; set; }
    }
}