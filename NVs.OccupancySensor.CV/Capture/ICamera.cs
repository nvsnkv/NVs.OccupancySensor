using System;
using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;
using NVs.OccupancySensor.CV.Settings;

namespace NVs.OccupancySensor.CV.Capture
{
    public interface ICamera : INotifyPropertyChanged
    {
        IObservable<Image<Gray, byte>> Stream { get; }
        
        bool IsRunning { get; }

        void Start();

        void Stop();

        CaptureSettings Settings { get; }
    }
}