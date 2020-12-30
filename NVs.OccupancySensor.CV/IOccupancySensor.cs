using System;
using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV
{
    public interface IOccupancySensor : INotifyPropertyChanged
    {
        bool? PresenceDetected { get; }

        bool IsRunning { get; }

        void Start();

        void Stop();

        IObservable<Image<Rgb,int>> Stream { get; }
    }
}