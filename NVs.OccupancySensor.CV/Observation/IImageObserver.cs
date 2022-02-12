using System;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;

namespace NVs.OccupancySensor.CV.Observation
{
    public interface IImageObserver<TColor> : IObserver<Image<TColor, byte>?>
    where TColor : struct, IColor
    {
        Task<Image<TColor, byte>?> GetImage(CancellationToken ct);
    }
}