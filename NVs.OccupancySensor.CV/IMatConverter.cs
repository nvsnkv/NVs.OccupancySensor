using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV
{
    public interface IMatConverter 
    {
            Image<Rgb, float> Convert(Mat input);
    }
}