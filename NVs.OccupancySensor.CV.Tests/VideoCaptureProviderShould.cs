using System;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using NVs.OccupancySensor.CV.Capture;
using NVs.OccupancySensor.CV.Settings;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests;

public class VideoCaptureProviderShould
{
    [Fact]
    public void ReturnTheSameCaptureInstanceWhenRequestedMultipleTimes()
    {
        var settings = new CaptureSettings("0", TimeSpan.MaxValue);
        var provider = new VideoCaptureProvider(settings);

        var tasks = new Func<VideoCapture>[]
        {
            () => provider.Get(),
            () => provider.Get(),
            () => provider.Get(),
            () => provider.Get(),
            () => provider.Get(),
            () => provider.Get(),
            () => provider.Get(),
            () => provider.Get(),
            () => provider.Get(),
            () => provider.Get(),
            () => provider.Get(),
            () => provider.Get(),
            () =>
            {
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                return provider.Get();
            }

        }.Select(a => Task.Factory.StartNew(a)).ToArray();

        Task.WaitAll(tasks);

        var captures = tasks.Select(t => t.Result).ToList();

        for (var i = 0; i < captures.Count - 1; i++)
        {
            Assert.True(ReferenceEquals(captures[i], captures[i + 1]));
        }
    }
}