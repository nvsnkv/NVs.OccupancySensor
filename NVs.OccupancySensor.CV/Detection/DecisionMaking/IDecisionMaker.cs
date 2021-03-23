using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Detection.DecisionMaking
{
    [Obsolete]
    public interface IDecisionMaker
    {
        bool DetectPresence (Image<Gray, byte> mask);

        IDecisionMakerSettings Settings { get; set; }
    }
}