using System;
using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Detection
{
    public interface IPeopleDetector : IObserver<Image<Rgb, byte>>, INotifyPropertyChanged
    {
        bool? PeopleDetected { get; }

        void Reset();
    }
}