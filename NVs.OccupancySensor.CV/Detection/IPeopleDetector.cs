using System;
using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Detection
{
    public interface IPeopleDetector : IObserver<Image<Gray, byte>>, INotifyPropertyChanged
    {
        bool? PeopleDetected { get; }

        Image<Gray,byte> Mask { get; }

        IDetectionSettings Settings { get; set; }

        void Reset();
    }
}