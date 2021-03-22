using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Detection.Correction
{
    internal class BypassCorrectionStrategy : ICorrectionStrategy
    {
        public Image<Gray, byte> Apply(Image<Gray, byte> source)
        {
            return source;
        }
    }
}