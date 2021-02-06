using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Detection.BackgroundSubtraction
{
    public interface IBackgroundSubtractionBasedPeopleDetector : IPeopleDetector
    {
        Image<Gray, byte> Mask { get; }
    }
}