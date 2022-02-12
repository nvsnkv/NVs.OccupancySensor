using System;
using Emgu.CV;
using Emgu.CV.Structure;
using NVs.OccupancySensor.CV.Utils;

namespace NVs.OccupancySensor.CV.Denoising
{
    public interface IDenoiser : IObserver<Image<Gray,byte>>
    {
        IObservable<Image<Gray, byte>> Output { get; }

        IDenoisingSettings Settings { get; set; }

        IStatistics Statistics { get; }
    }
}