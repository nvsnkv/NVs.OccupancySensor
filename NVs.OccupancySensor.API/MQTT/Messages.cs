using System.Collections.Generic;
using MQTTnet;
using MQTTnet.Protocol;
using Newtonsoft.Json;

namespace NVs.OccupancySensor.API.MQTT
{
    internal sealed class Messages
    {
        public readonly IEnumerable<MqttApplicationMessage> Configs;
        
        public readonly MqttApplicationMessage ServiceAvailable;
        public readonly MqttApplicationMessage ServiceUnavailable;
        
        public readonly MqttApplicationMessage SensorAvailable;
        public readonly MqttApplicationMessage SensorUnavailable;

        public readonly MqttApplicationMessage ServiceEnabled;
        public readonly MqttApplicationMessage ServiceDisabled;

        public readonly MqttApplicationMessage PresenceDetected;
        public readonly MqttApplicationMessage NoPresenceDetected;

        public readonly string ServiceCommandTopic;

        public Messages(string instanceId)
        {
            ServiceCommandTopic = $"homeassistant/switch/nvs_occupancy_sensor/{instanceId}/set";
            
            Configs = new[]
            {
                new MqttApplicationMessageBuilder()
                    .WithTopic($"homeassistant/binary_sensor/nvs_occupancy_sensor/{instanceId}/config")
                    .WithPayload(JsonConvert.SerializeObject(new
                    {
                        name = instanceId, device_class = "occupancy",
                        state_topic = $"homeassistant/binary_sensor/nvs_occupancy_sensor/{instanceId}/state",
                        availability_topic = $"homeassistant/binary_sensor/nvs_occupancy_sensor/{instanceId}/availability"
                    }))
                    .WithRetainFlag()
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build(),
                new MqttApplicationMessageBuilder()
                    .WithTopic($"homeassistant/switch/nvs_occupancy_sensor/{instanceId}/config")
                    .WithPayload(JsonConvert.SerializeObject(new
                    {
                        name = instanceId, device_class = "connectivity",
                        state_topic = $"homeassistant/switch/nvs_occupancy_sensor/{instanceId}/state",
                        command_topic = ServiceCommandTopic,
                        availability_topic = $"homeassistant/switch/nvs_occupancy_sensor/{instanceId}/availability"
                    }))
                    .WithRetainFlag()
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build()
            };

            ServiceAvailable = new MqttApplicationMessageBuilder()
                .WithTopic($"homeassistant/switch/nvs_occupancy_sensor/{instanceId}/availability")
                .WithPayload("online")
                .Build();
            
            ServiceUnavailable = new MqttApplicationMessageBuilder()
                .WithTopic($"homeassistant/switch/nvs_occupancy_sensor/{instanceId}/availability")
                .WithPayload("offline")
                .Build();
            
            SensorAvailable = new MqttApplicationMessageBuilder()
                .WithTopic($"homeassistant/binary_sensor/nvs_occupancy_sensor/{instanceId}/availability")
                .WithPayload("online")
                .Build();
            
            SensorUnavailable = new MqttApplicationMessageBuilder()
                .WithTopic($"homeassistant/binary_sensor/nvs_occupancy_sensor/{instanceId}/availability")
                .WithPayload("offline")
                .Build();
            
            ServiceEnabled = new MqttApplicationMessageBuilder()
                .WithTopic($"homeassistant/switch/nvs_occupancy_sensor/{instanceId}/state")
                .WithPayload("ON")
                .Build();
            
            ServiceDisabled = new MqttApplicationMessageBuilder()
                .WithTopic($"homeassistant/switch/nvs_occupancy_sensor/{instanceId}/state")
                .WithPayload("OFF")
                .Build();
            
            PresenceDetected = new MqttApplicationMessageBuilder()
                .WithTopic($"homeassistant/binary_sensor/nvs_occupancy_sensor/{instanceId}/state")
                .WithPayload("ON")
                .Build();
            
            NoPresenceDetected = new MqttApplicationMessageBuilder()
                .WithTopic($"homeassistant/binary_sensor/nvs_occupancy_sensor/{instanceId}/state")
                .WithPayload("OFF")
                .Build();
            
        }
    }
}