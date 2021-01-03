using System;
using System.ComponentModel;
using System.Net.Mime;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Detection
{
    public interface IPeopleDetector : IObserver<Image<Rgb, byte>>, INotifyPropertyChanged
    {
        void Detect(Image<Rgb, byte> source);
        
        bool? PeopleDetected { get; }

        void Reset();
    }
}