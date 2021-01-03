using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Impl.Detectors.HOG
{
    sealed class HogPeopleDetector : PeopleDetectorBase
    {
        private readonly IHOGDescriptorWrapper descriptor;

        public HogPeopleDetector([NotNull] ILogger<HogPeopleDetector> logger, [NotNull] Func<IHOGDescriptorWrapper> createDescriptor) : base(logger)
        {
            descriptor = createDescriptor();
        }

        protected override Rectangle[] PerformDetection(Image<Rgb, float> source)
        {
            var results = descriptor.DetectMultiScale(source);
            return results.Select(r => r.Rect).ToArray();
        }

        protected override void DoDispose(ILogger<PeopleDetectorBase> logger)
        {
            descriptor?.Dispose();
        }
    }
}