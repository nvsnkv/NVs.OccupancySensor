using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV
{
    public interface IPeopleDetector : INotifyPropertyChanged
    {
        Image<Rgb,byte> Detect(Image<Rgb,byte> source);
        
        bool? PeopleDetected { get; }

        void Reset();
    }
}