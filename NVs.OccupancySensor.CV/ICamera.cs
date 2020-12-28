﻿using System.ComponentModel;

namespace NVs.OccupancySensor.CV
{
    public interface ICamera : INotifyPropertyChanged
    {
        ICameraStream Stream { get; }
        
        bool IsRunning { get; }

        void Start();

        void Stop();

        Settings Settings { get; set; }
    }
}