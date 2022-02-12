using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;
using MQTTnet.Client.Receiving;
using MQTTnet.Client.Subscribing;
using NVs.OccupancySensor.API.MQTT.Watchdog;
using NVs.OccupancySensor.CV.Sense;

namespace NVs.OccupancySensor.API.MQTT
{
    /// <summary>
    /// MQTT Adapter complaint with Home Assistant MQTT Integration. See https://www.home-assistant.io/docs/mqtt/discovery/ for details
    /// </summary>
    internal sealed class HomeAssistantMqttAdapter : IMqttAdapter
    {
        private static readonly IMqttClientFactory ClientFactory = new MqttFactory();

        private static readonly MqttClientSubscribeResultCode[] SuccessfulSubscriptionResultCodes =
        {
            MqttClientSubscribeResultCode.GrantedQoS0,
            MqttClientSubscribeResultCode.GrantedQoS1,
            MqttClientSubscribeResultCode.GrantedQoS2
        };

        private readonly object thisLock = new();

        private readonly IOccupancySensor sensor;
        private readonly ILogger<HomeAssistantMqttAdapter> logger;
        private readonly IMqttClient client;
        private readonly IDisposable watchdog;
        private readonly IMqttClientOptions options;
        private readonly Messages messages;
        
        private volatile bool isRunning;
        
        public HomeAssistantMqttAdapter(IOccupancySensor sensor, ILogger<HomeAssistantMqttAdapter> logger, Func<(IMqttClient, IDisposable)> createClient, AdapterSettings settings)
        {
            if (createClient is null)
            {
                throw new ArgumentNullException(nameof(createClient));
            }

            this.sensor = sensor ?? throw new ArgumentNullException(nameof(sensor));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            messages = new Messages(settings.ClientId, Assembly.GetExecutingAssembly().GetName().Version?.ToString()  ?? string.Empty);

            try
            {
                (client, watchdog) = createClient();
                options = new MqttClientOptionsBuilder()
                    .WithClientId(settings.ClientId)
                    .WithTcpServer(settings.Server, settings.Port)
                    .WithWillMessage(messages.SensorUnavailable)
                    .WithCredentials(settings.User, settings.Password)
                    .Build();

                client.UseApplicationMessageReceivedHandler(ClientOnMessageReceived);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to set up MqttAdapter!");
                throw;
            }

            sensor.PropertyChanged += SensorOnPropertyChanged;
        }

        private void ClientOnMessageReceived(MqttApplicationMessageReceivedEventArgs args)
        {
            if (!messages.ServiceCommandTopic.Equals(args.ApplicationMessage.Topic))
            {
                return;
            }

            logger.LogInformation("Command received, updating sensor state...");

            bool newState;
            try
            {
                var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
                logger.LogInformation("Payload received:{payload}", payload);
                newState = payload.ToLowerInvariant().Equals("on");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to decode new state!");
                throw;
            }

            if (newState)
            {
                sensor.Start();
            }
            else
            {
                sensor.Stop();
            }
        }

        private async void SensorOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!IsRunning)
            {
                return;
            }

            var messagesToSend = PrepareMessages(e.PropertyName);
            try
            {
                await client.PublishAsync(messagesToSend);
                logger.LogInformation("State change message was published.");
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to publish state change! {ex}", ex);
            }
        }

        private IEnumerable<MqttApplicationMessage> PrepareMessages(string? propertyName)
        {
            switch (propertyName)
            {
                case nameof(sensor.IsRunning):
                    logger.LogInformation("Sensor.IsRunning has been changed to {isRunning}, preparing the message...", sensor.IsRunning);
                    yield return sensor.IsRunning
                        ? messages.ServiceEnabled
                        : messages.ServiceDisabled;
                    break;
                
                case nameof(sensor.PresenceDetected):
                    logger.LogInformation("Sensor.DetectPresence has been changed to {detected}, preparing the messages...", sensor.PresenceDetected);

                    yield return sensor.PresenceDetected.HasValue
                        ? messages.SensorAvailable
                        : messages.SensorUnavailable;

                    if (sensor.PresenceDetected.HasValue)
                    {
                        yield return sensor.PresenceDetected.Value
                            ? messages.PresenceDetected
                            : messages.NoPresenceDetected;
                    }
                    break;

            }
        }

        private bool SetIsRunning(bool newState)
        {
            if (isRunning)
            {
                return false;
            }

            lock (thisLock)
            {
                if (isRunning)
                {
                    return false;
                }

                isRunning = newState;
            }

            return true;
        }

        public bool IsRunning => isRunning;

        public async Task Start()
        {
            logger.LogInformation("Start requested...");
            if (!SetIsRunning(true))
            {
                logger.LogWarning("Adapter is already running, no action will be taken!");
                return;
            }

            try
            {
                await Connect();
                await SubscribeOnCommandTopic();
                await NotifyServiceIsOnline();
            }
            catch
            {
                isRunning = false;
                throw;
            }
            logger.LogInformation("Adapter started.");
        }

        private async Task Connect()
        {
            try
            {
                var result = await client.ConnectAsync(options);
                if (result.ResultCode != MqttClientConnectResultCode.Success)
                {
                    throw new IOException("Failed to connect to MQTT server! Client received unsuccessful result!")
                    {
                        Data = { { "Result", result } }
                    };
                }
                logger.LogInformation("Client successfully connected.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to connect to MQTT server!");
                throw;
            }
        }

        private async Task SubscribeOnCommandTopic()
        {
            try
            {
                var result = await client.SubscribeAsync(messages.ServiceCommandTopic);
                EnsureSubscriptionSuccessful(result);

                logger.LogInformation("Client subscribed to command topic");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to subscribe on command topic!");
            }
        }

        private static void EnsureSubscriptionSuccessful(MqttClientSubscribeResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (result.Items.Count == 0) throw new ArgumentException("No items returned!", nameof(result));
            var failed = result.Items.Where(i => !SuccessfulSubscriptionResultCodes.Contains(i.ResultCode)).ToList();
            if (failed.Any())
            {
                throw new ArgumentException("Client returned a set of unsuccessful subscriptions!")
                {
                    Data = { { "Failed items", failed } }
                };
            }
        }

        private async Task NotifyServiceIsOnline()
        {
            try
            {
                foreach (var config in messages.Configs)
                {
                    EnsureResultSuccessful(await client.PublishAsync(config));
                }

                logger.LogInformation("Config messages successfully published.");

                await client.PublishAsync(sensor.IsRunning
                    ? messages.ServiceEnabled
                    : messages.ServiceDisabled
                );
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to send configs and update service state!");
                throw;
            }
        }

        private static void EnsureResultSuccessful(MqttClientPublishResult result)
        {
            if (result.ReasonCode != MqttClientPublishReasonCode.Success)
            {
                throw new IOException("Failed to publish message!")
                {
                    Data = {{"Result", result}}
                };
            }
        }

        public async Task Stop()
        {
            logger.LogInformation("Stop requested...");
            if (!SetIsRunning(false))
            {
                logger.LogWarning("Adapter is already stopped, no action will be taken!");
            }

            try
            {
                await NotifyServiceIsOffline();
                await Disconnect();
            }
            finally
            {
                isRunning = false;
            }
            logger.LogInformation("Adapter stopped!");
        }

        private async Task Disconnect()
        {
            try
            {
                await client.DisconnectAsync();
                logger.LogInformation("Client successfully disconnected.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to disconnect from server!");
            }
        }

        private async Task NotifyServiceIsOffline()
        {
            try
            {
                await client.PublishAsync(messages.SensorUnavailable);
                logger.LogInformation("Last will and testament successfully published.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to send last will and testament!");
                throw;
            }
        }

        public void Dispose()
        {
            sensor.PropertyChanged -= SensorOnPropertyChanged;
            watchdog.Dispose();

            client.UseApplicationMessageReceivedHandler((IMqttApplicationMessageReceivedHandler)null!);
            client.Dispose();
            
        }

        public static Func<(IMqttClient, IDisposable)> CreateClient(WatchdogSettings watchdogSettings, ILogger<Watchdog.Watchdog> watchdogLogger)
        {
            if (watchdogSettings == null) throw new ArgumentNullException(nameof(watchdogSettings));
            if (watchdogLogger == null) throw new ArgumentNullException(nameof(watchdogLogger));

            var client = ClientFactory.CreateMqttClient();
            var watchdog = new Watchdog.Watchdog(client, watchdogLogger, watchdogSettings);

            return () => (client, watchdog);
        }
    }
}