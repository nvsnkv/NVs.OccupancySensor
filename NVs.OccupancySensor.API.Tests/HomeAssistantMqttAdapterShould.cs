using Moq;
using Xunit;
using NVs.OccupancySensor.CV.Sense;
using NVs.OccupancySensor.API.MQTT;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using Microsoft.Extensions.Configuration;
using System.Runtime.CompilerServices;

namespace NVs.OccupancySensor.API.Tests
{
    public class HomeAssistantMqttAdapterShould
    {
        private readonly Mock<IOccupancySensor> sensor = new Mock<IOccupancySensor>();
        private readonly Mock<ILogger<HomeAssistantMqttAdapter>> logger = new Mock<ILogger<HomeAssistantMqttAdapter>>();
        private readonly Mock<IMqttClient> client = new Mock<IMqttClient>();
        private readonly Mock<IConfiguration> config = new Mock<IConfiguration>();
        private readonly string expectedClientId = "AClientId";
        private readonly string expectedServer = "mqtt";
        private readonly string expectedPort = "1883";
        private readonly string expectedUser = "John";
        private readonly string expectedPassword = "John's password";

        public HomeAssistantMqttAdapterShould() 
        {
            var section = new Mock<IConfigurationSection>();
            section.SetupGet(s => s[It.Is<string>(v => "ClientId".Equals(v))]).Returns(expectedClientId);
            section.SetupGet(s => s[It.Is<string>(v => "Server".Equals(v))]).Returns(expectedServer);
            section.SetupGet(s => s[It.Is<string>(v => "Port".Equals(v))]).Returns(expectedPort);
            section.SetupGet(s => s[It.Is<string>(v => "User".Equals(v))]).Returns(expectedUser);
            section.SetupGet(s => s[It.Is<string>(v => "Password".Equals(v))]).Returns(expectedPassword);
   
            config.Setup(c => c.GetSection(It.Is<string>(v => "MQTT".Equals(v)))).Returns(section.Object);
        }

        [Fact]
        public void DisposeClientOnDispose()
        {
            client.Setup(c => c.Dispose()).Verifiable("Dispose was not called!");
            var adapter = new HomeAssistantMqttAdapter(sensor.Object, logger.Object, CreateClient, new AdapterSettings(config.Object));
            
            adapter.Dispose();
            client.Verify();
        }



        private IMqttClient CreateClient() => client.Object;
    }
}
