using System;
using Moq;
using Xunit;
using NVs.OccupancySensor.CV.Sense;
using NVs.OccupancySensor.API.MQTT;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using Microsoft.Extensions.Configuration;
using MQTTnet.Client.Options;
using System.Text;
using System.Threading.Tasks;
using MQTTnet.Client.Connecting;
using MQTTnet;
using System.Threading;
using MQTTnet.Client.Publishing;
using System.Linq;
using NVs.OccupancySensor.API.Tests.Utils;
using MQTTnet.Client.Subscribing;
using System.ComponentModel;
using System.Reflection;

namespace NVs.OccupancySensor.API.Tests
{
    public class HomeAssistantMqttAdapterShould
    {
        private readonly Mock<IOccupancySensor> sensor = new();
        private readonly Mock<ILogger<HomeAssistantMqttAdapter>> logger = new();
        private readonly Mock<IMqttClient> client = new();
        private readonly Mock<IDisposable> watchdog = new();
        private readonly Mock<IConfiguration> config = new();
        private readonly string expectedClientId = "AClientId";
        private readonly string expectedServer = "mqtt";
        private readonly int expectedPort = 5883;
        private readonly string expectedUser = "John";
        private readonly string expectedPassword = "John's password";
        
        private readonly SimpleMessageComparer comparer = new();

        private readonly Messages expectedMessages;

        public HomeAssistantMqttAdapterShould() 
        {
            expectedMessages = new Messages(expectedClientId, typeof(HomeAssistantMqttAdapter).Assembly.GetName().Version.ToString());

            var section = new Mock<IConfigurationSection>();
            section.SetupGet(s => s[It.Is<string>(v => "ClientId".Equals(v))]).Returns(expectedClientId);
            section.SetupGet(s => s[It.Is<string>(v => "Server".Equals(v))]).Returns(expectedServer);
            section.SetupGet(s => s[It.Is<string>(v => "Port".Equals(v))]).Returns(expectedPort.ToString());
            section.SetupGet(s => s[It.Is<string>(v => "User".Equals(v))]).Returns(expectedUser);
            section.SetupGet(s => s[It.Is<string>(v => "Password".Equals(v))]).Returns(expectedPassword);
   
            config.Setup(c => c.GetSection(It.Is<string>(v => "MQTT".Equals(v)))).Returns(section.Object);
            
            client.Setup(c => c.ConnectAsync(It.IsAny<IMqttClientOptions>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new MqttClientConnectResult()));
            client.Setup(c => c.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new MqttClientPublishResult()));
            client.Setup(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new MqttClientSubscribeResult()));
        }

        [Fact]
        public void DisposeClientOnDispose()
        {
            client.Setup(c => c.Dispose()).Verifiable("Dispose was not called!");
            var adapter = new HomeAssistantMqttAdapter(sensor.Object, logger.Object, CreateClient, new AdapterSettings(config.Object));
            
            adapter.Dispose();
            client.Verify();
        }

        [Fact]
        public void DisposeWatchdogOnDispose()
        {
            watchdog.Setup(c => c.Dispose()).Verifiable("Dispose was not called!");
            var adapter = new HomeAssistantMqttAdapter(sensor.Object, logger.Object, CreateClient, new AdapterSettings(config.Object));

            adapter.Dispose();
            watchdog.Verify();
        }

        [Fact]
        public async Task ProvideMqttSettingsFromConstructor()
        {
            client.Setup(c => c.ConnectAsync(
                It.Is<IMqttClientOptions>(
                    o => expectedClientId.Equals(o.ClientId)
                    && expectedUser.Equals(o.Credentials.Username)
                    && expectedPassword.Equals(Encoding.UTF8.GetString(o.Credentials.Password))
                    && o.ChannelOptions is MqttClientTcpOptions
                    && expectedServer.Equals((o.ChannelOptions as MqttClientTcpOptions).Server)
                    && expectedPort.Equals((o.ChannelOptions as MqttClientTcpOptions).Port)
                    ), It.IsAny<CancellationToken>()
                ))
                .Returns(Task.FromResult(new MqttClientConnectResult()))
                .Verifiable("Settings were not provided");

            var adapter = new HomeAssistantMqttAdapter(sensor.Object, logger.Object, CreateClient, new AdapterSettings(config.Object));
            await adapter.Start();

            client.Verify();
        }

        [Fact]
        public async Task SendConfigurationTopicsWhenStarted()
        {
            var sensorConfig = expectedMessages.Configs.First();
            var switchConfig = expectedMessages.Configs.Skip(1).First();

            client.Setup(c => c.ConnectAsync(It.IsAny<IMqttClientOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new MqttClientConnectResult()));

            client.Setup(c => c.PublishAsync(It.Is<MqttApplicationMessage>(m => comparer.Equals(sensorConfig, m)), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(new MqttClientPublishResult()))
            .Verifiable("Sensor config was not published!");

            client.Setup(c => c.PublishAsync(It.Is<MqttApplicationMessage>(m => comparer.Equals(m, switchConfig)), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(new MqttClientPublishResult()))
            .Verifiable("Switch config was not published!");

            var adapter = new HomeAssistantMqttAdapter(sensor.Object, logger.Object, CreateClient, new AdapterSettings(config.Object));
            await adapter.Start();

            client.Verify();
        }

        [Fact]
        public async Task SendNotificationWhenStopped()
        {
            client.Setup(c => c.PublishAsync(It.Is<MqttApplicationMessage>(m => comparer.Equals(m, expectedMessages.SensorUnavailable)), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(new MqttClientPublishResult()))
            .Verifiable("Sensor unavailable was not published!");

            sensor.Setup(s => s.Start()).Verifiable("Start was not called!");

            var adapter = new HomeAssistantMqttAdapter(sensor.Object, logger.Object, CreateClient, new AdapterSettings(config.Object));
            await adapter.Start();
            await adapter.Stop();

            client.Verify();
        }

        [Fact]
        public async Task SubscribeOnCommandTopicAfterStart()
        {
            client.Setup(s =>s.SubscribeAsync(
                It.Is<MqttClientSubscribeOptions>(o => o.TopicFilters.Any(i => expectedMessages.ServiceCommandTopic.Equals(i.Topic))), 
                It.IsAny<CancellationToken>())
                ).Verifiable("Subscribe was not called!");

            var adapter = new HomeAssistantMqttAdapter(sensor.Object, logger.Object, CreateClient, new AdapterSettings(config.Object));
            await adapter.Start();

            client.Verify();
        }

        [Fact]
        public void NotPublishMessagesIfNotStarted()
        {
            sensor.SetupGet(s => s.IsRunning).Returns(true);
            sensor.SetupGet(s => s.PresenceDetected).Returns(true);
            client.Setup(c => c.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>())).Verifiable("Publish was called");

            var adapter = new HomeAssistantMqttAdapter(sensor.Object, logger.Object, CreateClient, new AdapterSettings(config.Object));
            
            sensor.Raise(s => s.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IOccupancySensor.IsRunning)));
            sensor.Raise(s => s.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IOccupancySensor.PresenceDetected)));
            client.Verify(c => c.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task PublishMessagesWhenRunning()
        {
            sensor.SetupGet(s => s.IsRunning).Returns(true);
            client.Setup(c => c.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new MqttClientPublishResult()))
                .Verifiable("Publish was not called");

            var adapter = new HomeAssistantMqttAdapter(sensor.Object, logger.Object, CreateClient, new AdapterSettings(config.Object));
            await adapter.Start();

            sensor.Raise(s => s.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IOccupancySensor.IsRunning)));

            client.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PublishAppropriateMessagesWhenSensorIsRunningChanges(bool state)
        {
            var expectedMessage = state 
                ? expectedMessages.ServiceEnabled 
                : expectedMessages.ServiceDisabled;

            sensor.SetupGet(s => s.IsRunning).Returns(state);
            client.Setup(c => c.ConnectAsync(It.IsAny<IMqttClientOptions>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new MqttClientConnectResult()));
            client.Setup(c => c.PublishAsync(It.Is<MqttApplicationMessage>(m => comparer.Equals(m, expectedMessage)), It.IsAny<CancellationToken>())).Verifiable("Publish was not called");

            var adapter = new HomeAssistantMqttAdapter(sensor.Object, logger.Object, CreateClient, new AdapterSettings(config.Object));

            await adapter.Start();
            sensor.Raise(s => s.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IOccupancySensor.IsRunning)));

            client.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(null)]
        public async Task PublishAppropriateMessagesWhenPresenceDetectedChanges(bool? state)
        {
            var expectedAvailability = state.HasValue
                ? expectedMessages.SensorAvailable
                : expectedMessages.SensorUnavailable;
            
            sensor.SetupGet(s => s.PresenceDetected).Returns(state);
            client.Setup(c => c.ConnectAsync(It.IsAny<IMqttClientOptions>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new MqttClientConnectResult()));
            client.Setup(c => c.PublishAsync(It.Is<MqttApplicationMessage>(m => comparer.Equals(m, expectedAvailability)), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new MqttClientPublishResult()))
                .Verifiable("Publish was not called");
            client.Setup(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new MqttClientSubscribeResult()));
            if (state.HasValue)
            {
                var expectedMessage = state.Value
                    ? expectedMessages.PresenceDetected
                    : expectedMessages.NoPresenceDetected;
                
                client.Setup(c => c.PublishAsync(It.Is<MqttApplicationMessage>(m => comparer.Equals(m, expectedMessage)), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(new MqttClientPublishResult()))
                    .Verifiable("Publish was not called");
            }

            var adapter = new HomeAssistantMqttAdapter(sensor.Object, logger.Object, CreateClient, new AdapterSettings(config.Object));

            await adapter.Start();
            sensor.Raise(s => s.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IOccupancySensor.PresenceDetected)));

            client.Verify();
        }

        private (IMqttClient, IDisposable) CreateClient() => (client.Object, watchdog.Object);
    }
}
