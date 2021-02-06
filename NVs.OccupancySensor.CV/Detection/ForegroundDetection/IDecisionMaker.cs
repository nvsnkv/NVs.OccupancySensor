using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Detection.ForegroundDetection
{
    public interface IDecisionMaker
    {
        bool PresenceDetected (Image<Gray, byte> mask);

        IDecisionMakerSettings Settings { get; set; }
    }
}