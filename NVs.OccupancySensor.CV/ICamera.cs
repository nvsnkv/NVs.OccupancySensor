using System.ComponentModel;
using NVs.OccupancySensor.CV.Settings;

namespace NVs.OccupancySensor.CV
{
    public interface ICamera : INotifyPropertyChanged
    {
        ICameraStream Stream { get; }
        
        bool IsRunning { get; }

        void Start();

        void Stop();

        CameraSettings Settings { get; set; }
    }
}