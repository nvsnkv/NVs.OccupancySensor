using Microsoft.Extensions.Configuration;
using Moq;
using NVs.OccupancySensor.API.MQTT;
using Xunit;

namespace NVs.OccupancySensor.API.Tests 
{
    public sealed class AdapterSettingsShould
    {
        [Theory]
        [InlineData(1883)]
        [InlineData(null)]
        public void RetrievePropertiesFromSettings(int? portNumber)
        {
            var expectedClientId = "Expected Client ID";
            var expectedServer = "mqtt.server.example.com";
            var expectedPort = portNumber;
            var expectedUser = "user";
            var expectedPassword = "passwd";
            var expectedVersion = "0.0.TEST";

            var configuration = new Mock<IConfiguration>();
            var section = new Mock<IConfigurationSection>();
            configuration.Setup(c => c.GetSection(It.Is<string>(v => "MQTT".Equals(v)))).Returns(section.Object);
            section.SetupGet(s => s[It.Is<string>(v => "ClientId".Equals(v))]).Returns(expectedClientId);
            section.SetupGet(s => s[It.Is<string>(v => "Server".Equals(v))]).Returns(expectedServer);
            section.SetupGet(s => s[It.Is<string>(v => "Port".Equals(v))]).Returns(expectedPort?.ToString());
            section.SetupGet(s => s[It.Is<string>(v => "User".Equals(v))]).Returns(expectedUser);
            section.SetupGet(s => s[It.Is<string>(v => "Password".Equals(v))]).Returns(expectedPassword);
            configuration.SetupGet(c => c[It.Is<string>(v => "Version".Equals(v))]).Returns(expectedVersion);

            var settings = new AdapterSettings(configuration.Object);
            
            Assert.Equal(expectedClientId, settings.ClientId);
            Assert.Equal(expectedServer, settings.Server);
            Assert.Equal(expectedPort, settings.Port);
            Assert.Equal(expectedUser, settings.User);
            Assert.Equal(expectedPassword, settings.Password);
            Assert.Equal(expectedVersion, settings.Version);
        }
    }
}