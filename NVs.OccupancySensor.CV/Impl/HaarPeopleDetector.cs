using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Impl
{
    sealed class HaarPeopleDetector : IPeopleDetector
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Image<Rgb, float> Detect(Image<Rgb, float> source)
        {
            throw new System.NotImplementedException();
        }

        public bool? PeopleDetected { get; }
        public void Reset()
        {
            throw new System.NotImplementedException();
        }
    }
}