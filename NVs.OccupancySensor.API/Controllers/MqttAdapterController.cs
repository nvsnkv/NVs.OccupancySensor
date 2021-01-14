using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NVs.OccupancySensor.API.MQTT;

namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class MqttAdapterController : ControllerBase
    {
        private readonly IMqttAdapter adapter;

        public MqttAdapterController(IMqttAdapter adapter)
        {
            this.adapter = adapter;
        }

        [HttpGet]
        [Route("[action]")]
        public bool IsRunning()
        {
            return adapter.IsRunning;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task Start() 
        {
            await adapter.Start();
        }

        [HttpPost]
        [Route("[action]")]
        public async Task Stop() 
        {
            await adapter.Stop();
        }
    }
    
}