using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV
{
    public interface IPeopleDetector : INotifyPropertyChanged
    {
        Image<Rgb, float> Detect(Image<Rgb, float> source);
        
        bool? PeopleDetected { get; }

        void Reset();
    }
}