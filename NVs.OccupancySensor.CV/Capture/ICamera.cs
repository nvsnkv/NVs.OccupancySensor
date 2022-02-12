using System;
using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Capture
{
    public interface ICamera : INotifyPropertyChanged, IDisposable
    {
        IObservable<Image<Gray, byte>> Stream { get; }
        
        bool IsRunning { get; }

        void Start();

        void Stop();
    }
}