using System;
using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Utils
{
    internal abstract class DebouncedObserver : IObserver<Image<Rgb, byte>>, INotifyPropertyChanged
    {
        private readonly ProcessingLock processingLock = new ProcessingLock();
        private readonly ILogger logger;

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(Image<Rgb, byte> value)
        {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}