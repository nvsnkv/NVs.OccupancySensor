using System;
using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Denoising
{
    public interface IDenoiser : IObserver<Image<Rgb,byte>>, INotifyPropertyChanged
    {
        Image<Rgb, byte> Output { get; }

        IDenoisingSettings Settings { get; set; }

        void Reset();
    }
}