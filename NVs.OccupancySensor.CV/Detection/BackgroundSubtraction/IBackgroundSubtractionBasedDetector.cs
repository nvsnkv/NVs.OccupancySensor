using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Detection.BackgroundSubtraction
{
    public interface IBackgroundSubtractionBasedDetector : IPeopleDetector
    {
        Image<Gray, byte> Mask { get; }

        IBackgroundSubtractionBasedDetectorSettings Settings { get; }
    }
}