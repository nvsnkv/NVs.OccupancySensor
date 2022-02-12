using System;
using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Detection
{
    public interface IPeopleDetector : IObserver<Image<Gray, byte>>, INotifyPropertyChanged
    {
        bool? PeopleDetected { get; }

        Image<Gray,byte>? Mask { get; }
        
        void Reset();
    }
}