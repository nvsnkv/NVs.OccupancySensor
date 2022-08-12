using System;
using System.ComponentModel;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Sense;

namespace NVs.OccupancySensor.API;

internal sealed class StateKeeper
{
    private static readonly string DefaultStateFile = "state";

    private readonly string stateFile;
    private readonly IOccupancySensor sensor;
    private readonly ILogger<StateKeeper> logger;

    public StateKeeper(IOccupancySensor sensor, ILogger<StateKeeper> logger) : this(sensor, logger, DefaultStateFile)
    {
    }

    public StateKeeper(IOccupancySensor sensor, ILogger<StateKeeper> logger, string stateFile)
    {
        this.sensor = sensor;
        this.logger = logger;
        this.stateFile = stateFile;
        sensor.PropertyChanged += OnSensorPropertyChanged;
    }

    private void OnSensorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        using (logger.BeginScope("Sensor state changed"))
        {
            logger.LogDebug("Property {propertyName} has changed.", e.PropertyName);
            if (e.PropertyName != nameof(IOccupancySensor.IsRunning))
            {
                return;
            }

            try
            {
                var sensorIsRunning = sensor.IsRunning;
                File.WriteAllText(stateFile, sensorIsRunning.ToString());
                logger.LogInformation("State {isRunning} successfully saved.", sensorIsRunning);
            }
            catch (Exception x)
            {
                logger.LogError(x, "Failed to save state change!");
            }
        }
    }

    public bool? CheckWasSensorRunning()
    {
        using (logger.BeginScope("Historical state requested"))
        {
            try
            {
                var state = File.ReadAllText(stateFile);
                logger.LogDebug("Got following historical state: {historicalState}", state);
                return state == true.ToString();
            }
            catch (FileNotFoundException notFound)
            {
                logger.LogInformation(notFound, "A file with historical state was not found.");
                return null;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to retrieve historical state from file!");
                return null;
            }
        }
    }


}