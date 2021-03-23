using System;
using Emgu.CV;
using Emgu.CV.Structure;
using NVs.OccupancySensor.CV.Detection;

namespace NVs.OccupancySensor.CV.BackgroundSubtraction
{
    [Obsolete]
    public interface IBackgroundSubtractionBasedDetector : IPeopleDetector
    {
        Image<Gray, byte> Mask { get; }

        IBackgroundSubtractionBasedDetectorSettings Settings { get; }
    }
}