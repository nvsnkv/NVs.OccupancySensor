using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace NVs.OccupancySensor.API.MQTT
{
    internal sealed class AdapterSettings
    {
        public AdapterSettings([NotNull] IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            
            var section = configuration.GetSection("MQTT");
            ClientId = section["ClientId"] ?? throw new InvalidOperationException("MQTT:ClientId field is not defined in configuration!");
            Server = section["Server"] ?? throw new InvalidOperationException("MQTT:Server field is not defined in configuration!");
            Port = int.TryParse(section["Port"], out var port) ? port : (int?)null;
            User = section["User"] ?? throw new InvalidOperationException("MQTT:User field is not defined in configuration!");
            Password = section["Password"] ?? throw new InvalidOperationException("MQTT:Password field is not defined in configuration!");
        }

        public string ClientId { get; }
        
        public string Server { get; }
        
        public int? Port { get; }
        
        public string User { get; }
        
        public string Password { get; }
    }
}