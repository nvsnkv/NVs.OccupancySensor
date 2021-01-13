﻿using System.Collections.Generic;
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
            
            object device = new {
                            identifiers = new [] { $"id_{instanceId}_device" },
                            name = "occupancy_sensor",
                            sw_version = "0",
                            model = "DYI Optical Occupancy Sensor based on RPI4",
                            manufacturer = "nvsnkv"
                        };

            Configs = new[]
            {
                new MqttApplicationMessageBuilder()
                    .WithTopic($"homeassistant/binary_sensor/nvs_occupancy_sensor/{instanceId}/config")
                    .WithPayload(JsonConvert.SerializeObject(new
                    {
                        name = "Occupancy", 
                        device_class = "occupancy",
                        state_topic = $"homeassistant/binary_sensor/nvs_occupancy_sensor/{instanceId}/state",
                        availability_topic = $"homeassistant/binary_sensor/nvs_occupancy_sensor/{instanceId}/availability",
                        unique_id = $"id_{instanceId}_sensor",
                        device 
                    }))
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build(),
                new MqttApplicationMessageBuilder()
                    .WithTopic($"homeassistant/switch/nvs_occupancy_sensor/{instanceId}/config")
                    .WithPayload(JsonConvert.SerializeObject(new
                    {
                        name = "Service", 
                        state_topic = $"homeassistant/switch/nvs_occupancy_sensor/{instanceId}/state",
                        command_topic = ServiceCommandTopic,
                        availability_topic = $"homeassistant/switch/nvs_occupancy_sensor/{instanceId}/availability",
                        payload_off = "OFF",
                        payload_on = "ON",
                        unique_id = $"id_{instanceId}_service",
                        device
                    }))
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