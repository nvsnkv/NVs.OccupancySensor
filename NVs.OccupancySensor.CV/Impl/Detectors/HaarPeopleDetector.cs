using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Impl.Detectors
{
    sealed class HaarPeopleDetector : PeopleDetectorBase
    {
        private readonly CascadeClassifier classifier;
        
        public HaarPeopleDetector([NotNull] ILogger<PeopleDetectorBase> logger) : base(logger)
        {
            var path = GetPathToCascade();
            classifier = new CascadeClassifier(path);
        }

        private static  string GetPathToCascade()
        {
            var assembly = Assembly.GetAssembly(typeof(HaarPeopleDetector));
            var location = assembly.Location;
            return location.Replace(assembly.ManifestModule.Name, "haarcascade_upperbody.xml");
        }

        protected override Rectangle[] PerformDetection(Image<Rgb,byte> source)
        {
            var image = source.Convert<Bgr, byte>();
            return classifier.DetectMultiScale(image);
        }

        protected override void DoDispose(ILogger<PeopleDetectorBase> logger)
        {
            classifier?.Dispose();
        }
    }
}