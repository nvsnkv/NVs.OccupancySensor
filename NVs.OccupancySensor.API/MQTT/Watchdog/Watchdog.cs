using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;

namespace NVs.OccupancySensor.API.MQTT.Watchdog
{
    internal sealed class Watchdog : IDisposable
    {
        private readonly IMqttClient client;
        private readonly WatchdogSettings settings;
        private readonly ILogger<Watchdog> logger;
        private int alreadyHandling;
        private int attemptsMade;
        private readonly CancellationTokenSource cts = new();

        public Watchdog(IMqttClient client, ILogger<Watchdog> logger, WatchdogSettings settings)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            client.UseDisconnectedHandler(Disconnected);
        }

        private async Task Disconnected(MqttClientDisconnectedEventArgs args)
        {
            using (logger.BeginScope("Client disconnected"))
            {
                if (args == null)
                {
                    var e = new ArgumentNullException(nameof(args));
                    logger.LogError(e, "Null argument received!");
                    throw e;
                }
                
                try
                {
                    if (Interlocked.CompareExchange(ref alreadyHandling, 1, 0) != 0)
                    {
                        logger.LogWarning("Watchdog is already handling a disconnect!");
                        return;
                    }

                    attemptsMade = 0;

                    while (ShouldRetry(args))
                    {
                        var delay = (attemptsMade + 1) * settings.Interval;
                        logger.LogInformation("Will reconnect in {delay}", delay);
                        await Task.Delay(delay, cts.Token);

                        try
                        {
                            var result = await client.ReconnectAsync(cts.Token);
                            if (result.ResultCode == MqttClientConnectResultCode.Success)
                            {
                                attemptsMade = 0;
                                logger.LogInformation("Client successfully reconnected.");
                                return;
                            }
                            else
                            {
                                logger.LogInformation("Failed to reconnect: received {resultCode}, {resultReason}", result.ResultCode, result.ReasonString);
                                attemptsMade++;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Exception raised during reconnect!");
                            attemptsMade++;
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to reconnect!");
                    throw;
                }
                finally
                {
                    Interlocked.Exchange(ref alreadyHandling, 0);
                }
            }
        }

        private bool ShouldRetry(MqttClientDisconnectedEventArgs args)
    {
        if (settings.AttemptsCount == 0)
        {
            logger.LogInformation("Retry is not configured: retry attempts count is zero");
            return false;
        }

        if (args.Reason == MqttClientDisconnectReason.NormalDisconnection)
        {
            logger.LogInformation("No retry needed: normal disconnection received");
            return false;
        }

        if (attemptsMade >= settings.AttemptsCount)
        {
            logger.LogWarning("No further retries will happen: attempts limit reached!");
            return false;
        }

        return true;
    }

    public void Dispose()
    {
        cts.Cancel();
        client.UseDisconnectedHandler((IMqttClientDisconnectedHandler)null!);
    }
}
}