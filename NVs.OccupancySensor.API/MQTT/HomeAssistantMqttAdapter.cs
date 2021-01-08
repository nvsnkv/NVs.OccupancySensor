using System;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Client.Subscribing;
using NVs.OccupancySensor.CV.Sense;

namespace NVs.OccupancySensor.API.MQTT
{
    /// <summary>
    /// MQTT Adapter complaint with Home Assistant MQTT Integration. See https://www.home-assistant.io/docs/mqtt/discovery/ for details
    /// </summary>
    internal sealed class HomeAssistantMqttAdapter : IDisposable
    {
        private readonly object thisLock = new object();
       
        private readonly IOccupancySensor sensor;
        private readonly ILogger<HomeAssistantMqttAdapter> logger;
        private readonly IMqttClient client;
        private readonly IMqttClientOptions options;
        private readonly Messages messages;
        
        private volatile bool isRunning;

        public HomeAssistantMqttAdapter([NotNull] IOccupancySensor sensor, [NotNull] ILogger<HomeAssistantMqttAdapter> logger, [NotNull] AdapterSettings settings)
        {
            this.sensor = sensor ?? throw new ArgumentNullException(nameof(sensor));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            messages = new Messages(settings.ClientId);
            
            try
            {
                client = new MqttFactory().CreateMqttClient();
                options = new MqttClientOptionsBuilder()
                    .WithClientId(settings.ClientId)
                    .WithTcpServer(settings.Server, settings.Port)
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

                await client.SubscribeAsync(messages.ServiceCommandTopic);
                logger.LogInformation("Client subscribed to command topic");
                
                await NotifyServiceIsOnline();
            }
            catch
            {
                isRunning = false;
                throw;
            }
            logger.LogInformation("Adapter started.");
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
                
                await client.DisconnectAsync();
                logger.LogInformation("Client successfully disconnected.");
            }
            finally
            {
                isRunning = false;
            }
            logger.LogInformation("Adapter stopped!");
        }

        private async Task NotifyServiceIsOffline()
        {
            try
            {
                await client.PublishAsync(messages.SensorUnavailable);
                await client.PublishAsync(messages.ServiceUnavailable);
                logger.LogInformation("Last will and testament successfully published.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to send last will and testament!");
                throw;
            }
        }

        private async Task NotifyServiceIsOnline()
        {
            try
            {
                await client.PublishAsync(messages.Configs);
                logger.LogInformation("Config messages successfully published.");

                await client.PublishAsync(messages.ServiceAvailable);
                await UpdateState();
                logger.LogInformation("Service state successfully updated.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to send configs and update service state!");
                throw;
            }
        }

        private async Task UpdateState()
        {
            await client.PublishAsync(
                sensor.IsRunning 
                    ? messages.ServiceEnabled 
                    : messages.ServiceDisabled);
            
            await client.PublishAsync(
                sensor.PresenceDetected.HasValue
                    ? messages.SensorAvailable
                    : messages.SensorUnavailable);

            if (sensor.PresenceDetected.HasValue)
            {
                await client.PublishAsync(
                    sensor.PresenceDetected.Value
                        ? messages.PresenceDetected
                        : messages.NoPresenceDetected);
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

        private async Task Connect()
        {
            try
            {
                await client.ConnectAsync(options);
                logger.LogInformation("Client successfully connected.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to connect to MQTT server!");
                throw;
            }
        }

        private async void SensorOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!IsRunning)
            {
                return;
            }

            try
            {
                await UpdateState();
                logger.LogInformation("State updated.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update state!");
            }
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
                logger.LogInformation($"Payload received:{payload}");
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

        public void Dispose()
        {
            sensor.PropertyChanged -= SensorOnPropertyChanged;
            client.UseApplicationMessageReceivedHandler((IMqttApplicationMessageReceivedHandler)null);
            
            client.Dispose();
        }
    }
}