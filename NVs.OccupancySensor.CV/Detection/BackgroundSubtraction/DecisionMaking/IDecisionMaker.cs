using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Detection.BackgroundSubtraction.DecisionMaking
{
    public interface IDecisionMaker
    {
        bool DetectPresence (Image<Gray, byte> mask);

        IDecisionMakerSettings Settings { get; set; }
    }
}