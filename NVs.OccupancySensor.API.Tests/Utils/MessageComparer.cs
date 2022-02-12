using System.Collections.Generic;
using System.Linq;
using MQTTnet;

namespace NVs.OccupancySensor.API.Tests.Utils
{
    internal sealed class SimpleMessageComparer : IEqualityComparer<MqttApplicationMessage>
    {
        public bool Equals(MqttApplicationMessage? x, MqttApplicationMessage? y)
        {
            if (x is null && y is null)
            {
                return true;
            }

            if(x is null || y is null) 
            {
                return false;
            }

            return x.Topic == y.Topic && x.Payload.SequenceEqual(y.Payload) && x.QualityOfServiceLevel == y.QualityOfServiceLevel;
        }

        public int GetHashCode(MqttApplicationMessage obj)
        {
            return obj.GetHashCode();
        }
    }
}