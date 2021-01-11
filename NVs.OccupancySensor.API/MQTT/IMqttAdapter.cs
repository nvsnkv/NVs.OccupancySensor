using System;
using System.Threading.Tasks;

namespace NVs.OccupancySensor.API.MQTT
{
    public interface IMqttAdapter : IDisposable
    {
        bool IsRunning { get; }

        Task Start();

        Task Stop();
    }
}