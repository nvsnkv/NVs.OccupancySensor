using System;
using Emgu.CV;
using Emgu.CV.Structure;
using NVs.OccupancySensor.CV.Utils;

namespace NVs.OccupancySensor.CV.BackgroundSubtraction
{
    public interface IBackgroundSubtractor: IObserver<Image<Gray, byte>>
    {
            IObservable<Image<Gray, byte>> Output { get; }

            IStatistics Statistics { get; }

            void Reset();
    }
}