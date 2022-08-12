using System.ComponentModel;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Sense;
using Xunit;

namespace NVs.OccupancySensor.API.Tests;

public sealed class StateKeeperShould
{
    private readonly Mock<ILogger<StateKeeper>> logger = new();
    private readonly Mock<IOccupancySensor> sensor = new();
    private readonly string statePath = "state";


    private StateKeeper keeper;

    public StateKeeperShould()
    {
        keeper = new StateKeeper(sensor.Object, logger.Object);
    }

    [Fact]
    public void ReturnNullIfNoHistoricalChangeDetected()
    {
        if (File.Exists(statePath)){ 
            File.Delete(statePath);
        }

        var result = keeper.CheckWasSensorRunning();
        Assert.Null(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TrackStateChangesProperly(bool expectedState)
    {
        sensor.SetupGet(s => s.IsRunning).Returns(expectedState);
        sensor.Raise(s => s.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IOccupancySensor.IsRunning)));

        var result = keeper.CheckWasSensorRunning();
        Assert.Equal(expectedState, result);
    }

    [Fact]
    public void IgnoreUnrelatedProperties()
    {
        if (File.Exists(statePath))
        {
            File.Delete(statePath);
        }

        sensor.Raise(s => s.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IOccupancySensor.PresenceDetected)));

        var result = keeper.CheckWasSensorRunning();
        Assert.Null(result);
    }
}