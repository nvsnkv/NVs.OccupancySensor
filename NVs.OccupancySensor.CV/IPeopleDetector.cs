using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV
{
    public interface IPeopleDetector : INotifyPropertyChanged
    {
        Image<Rgb, int> Detect(Image<Rgb, int> source);
        
        bool? PeopleDetected { get; }
    }
}