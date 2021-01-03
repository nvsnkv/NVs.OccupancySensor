using System;
using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Detection
{
    sealed class DummyPeopleDetector : IPeopleDetector
    {
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(Image<Rgb, byte> value)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        public void Detect(Image<Rgb, byte> source)
        {
        }

        public bool? PeopleDetected { get; }
        
        public void Reset()
        {
        }
    }
}