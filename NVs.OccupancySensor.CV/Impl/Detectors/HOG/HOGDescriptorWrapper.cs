using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Impl.Detectors.HOG
{
    sealed class HOGDescriptorWrapper : IHOGDescriptorWrapper
    {
        private readonly HOGDescriptor descriptor;

        private HOGDescriptorWrapper() 
        {
            descriptor = new HOGDescriptor();
        }
        public MCvObjectDetection[] DetectMultiScale(Image<Rgb, float> source)
        {
            return descriptor.DetectMultiScale(source);
        }

        public void Dispose()
        {
            descriptor?.Dispose();
        }

        public static HOGDescriptorWrapper Create() 
        {
            return new HOGDescriptorWrapper();
        }
    }
}