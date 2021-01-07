using System.ComponentModel;

namespace NVs.OccupancySensor.CV.Sense
{
    public interface IOccupancySensor : INotifyPropertyChanged
    {
        bool? PresenceDetected { get; }

        bool IsRunning { get; }

        void Start();

        void Stop();
    }
}