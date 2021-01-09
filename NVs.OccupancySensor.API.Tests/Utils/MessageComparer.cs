using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MQTTnet;

namespace NVs.OccupancySensor.API.Tests.Utils
{
    internal sealed class SimpleMessageComparer : IEqualityComparer<MqttApplicationMessage>
    {
        public bool Equals([AllowNull] MqttApplicationMessage x, [AllowNull] MqttApplicationMessage y)
        {
            if (ReferenceEquals(x, null) && ReferenceEquals(y, null))
            {
                return true;
            }

            if(ReferenceEquals(x, null) || ReferenceEquals(y, null)) 
            {
                return false;
            };

            return x.Topic == y.Topic && x.Payload.SequenceEqual(y.Payload) && x.QualityOfServiceLevel == y.QualityOfServiceLevel;
        }

        public int GetHashCode([DisallowNull] MqttApplicationMessage obj)
        {
            return obj.GetHashCode();
        }
    }
}