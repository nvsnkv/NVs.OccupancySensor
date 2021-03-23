using System;
using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.BackgroundSubtraction
{
    [Obsolete]
    public interface IBackgroundSubtractor: IObserver<Image<Rgb,byte>>, INotifyPropertyChanged
    {
            IObservable<Image<Gray, byte>> Output { get; }

            IBackgroundSubtractorSettings Settings { get; set; }

            void Reset();
    }
}