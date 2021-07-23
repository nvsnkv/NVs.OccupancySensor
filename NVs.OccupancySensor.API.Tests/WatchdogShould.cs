using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using NVs.OccupancySensor.API.MQTT;
using NVs.OccupancySensor.API.MQTT.Watchdog;
using Xunit;

namespace NVs.OccupancySensor.API.Tests
{
    public sealed class WatchdogShould
    {
        private readonly Mock<IMqttClient> client = new Mock<IMqttClient>();
        private IMqttClientDisconnectedHandler handler;

        private readonly Mock<ILogger<Watchdog>> logger = new Mock<ILogger<Watchdog>>();
        private readonly Mock<IConfiguration> config = new Mock<IConfiguration>();
        private readonly Mock<IConfigurationSection> retriesSection = new Mock<IConfigurationSection>();
        

        public WatchdogShould()
        {
            client.SetupSet(c => c.DisconnectedHandler = It.IsAny<IMqttClientDisconnectedHandler>()).Callback<IMqttClientDisconnectedHandler>(val => handler = val);
            config.Setup(s => s.GetSection("MQTT:Reconnect")).Returns(retriesSection.Object);
        }

        [Fact]
        public async Task NotReconnectAfterNormalDisconnect()
        {
            retriesSection.Setup(s => s["AttemptsCount"]).Returns("1");
            client.Setup(c => c.ConnectAsync(It.IsAny<IMqttClientOptions>(),It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new MqttClientAuthenticateResult() { ResultCode = MqttClientConnectResultCode.Success }))
                .Verifiable("Reconnect was called!");

            var _ = new Watchdog(client.Object, logger.Object, new WatchdogSettings(config.Object));
            await handler.HandleDisconnectedAsync(new MqttClientDisconnectedEventArgs(true, new Exception("Test exception"), new MqttClientAuthenticateResult(), MqttClientDisconnectReason.NormalDisconnection));

            client.Verify(c => c.ConnectAsync(It.IsAny<IMqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData("0")]
        [InlineData(null)]
        public async Task NotReconnectIfItsNotConfigured(string retriesCount)
        {
            retriesSection.Setup(s => s["AttemptsCount"]).Returns(retriesCount);
            client.Setup(c => c.ConnectAsync(It.IsAny<IMqttClientOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new MqttClientAuthenticateResult() { ResultCode = MqttClientConnectResultCode.Success }))
                .Verifiable("Reconnect was called!");
            var _ = new Watchdog(client.Object, logger.Object, new WatchdogSettings(config.Object));
            await handler.HandleDisconnectedAsync(new MqttClientDisconnectedEventArgs(true, new Exception("Test exception"), new MqttClientAuthenticateResult(), MqttClientDisconnectReason.ProtocolError));

            client.Verify(c => c.ConnectAsync(It.IsAny<IMqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RetryMultipleTimes()
        {
            retriesSection.Setup(s => s["AttemptsCount"]).Returns("2");
            client.Setup(c => c.ConnectAsync(It.IsAny<IMqttClientOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new MqttClientAuthenticateResult() { ResultCode = MqttClientConnectResultCode.ServerUnavailable }))
                .Verifiable("Reconnect was not called!");

            var _ = new Watchdog(client.Object, logger.Object, new WatchdogSettings(config.Object));
            await handler.HandleDisconnectedAsync(new MqttClientDisconnectedEventArgs(true, new Exception("Test exception"), new MqttClientAuthenticateResult(), MqttClientDisconnectReason.ProtocolError));

            client.Verify(c => c.ConnectAsync(It.IsAny<IMqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task HaveDelayBetweenAttempts()
        {
            retriesSection.Setup(s => s["AttemptsCount"]).Returns("2");
            retriesSection.Setup(s => s["IntervalBetweenAttempts"]).Returns("00:00:06");

            var reconnectsRequested = 0;
            client.Setup(c => c.ConnectAsync(It.IsAny<IMqttClientOptions>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(new MqttClientAuthenticateResult() { ResultCode = reconnectsRequested++ > 0 ? MqttClientConnectResultCode.Success : MqttClientConnectResultCode.ServerUnavailable }))
                .Verifiable("Reconnect was not called!");

            var _ = new Watchdog(client.Object, logger.Object, new WatchdogSettings(config.Object));
            var sw = new Stopwatch();
            sw.Start();
            await handler.HandleDisconnectedAsync(new MqttClientDisconnectedEventArgs(true, new Exception("Test exception"), new MqttClientAuthenticateResult(), MqttClientDisconnectReason.ProtocolError));
            sw.Stop();

            client.Verify(c => c.ConnectAsync(It.IsAny<IMqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            Assert.True(sw.Elapsed >= TimeSpan.FromSeconds(18));
        }

        [Fact]
        public void HandleDisconnectOnce()
        {
            retriesSection.Setup(s => s["AttemptsCount"]).Returns("2");
            retriesSection.Setup(s => s["IntervalBetweenAttempts"]).Returns("00:00:06");

            client.Setup(c => c.ConnectAsync(It.IsAny<IMqttClientOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new MqttClientAuthenticateResult() { ResultCode = MqttClientConnectResultCode.Success }))
                .Verifiable();

            var _ = new Watchdog(client.Object, logger.Object, new WatchdogSettings(config.Object));

            Task.WaitAll(
                handler.HandleDisconnectedAsync(new MqttClientDisconnectedEventArgs(true, new Exception("Test exception"), new MqttClientAuthenticateResult(), MqttClientDisconnectReason.UnspecifiedError)),
                handler.HandleDisconnectedAsync(new MqttClientDisconnectedEventArgs(true, new Exception("Test exception"), new MqttClientAuthenticateResult(), MqttClientDisconnectReason.ServerBusy))
                );

            client.Verify(c => c.ConnectAsync(It.IsAny<IMqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void DoNotHandleDisconnectIfDisposed()
        {
            var watchdog = new Watchdog(client.Object, logger.Object, new WatchdogSettings(config.Object));
            watchdog.Dispose();

            Assert.Null(handler);
        }
    }
}