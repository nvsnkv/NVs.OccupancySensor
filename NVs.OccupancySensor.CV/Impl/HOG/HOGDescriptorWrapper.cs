using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Impl.HOG
{
    sealed class HOGDescriptorWrapper : IHOGDescriptorWrapper
    {
        private readonly HOGDescriptor descriptor;

        public HOGDescriptorWrapper() 
        {
            descriptor = new HOGDescriptor();
        }
        public MCvObjectDetection[] DetectMultiScale(Image<Rgb, int> source)
        {
            return descriptor.DetectMultiScale(source);
        }

        public void Dispose()
        {
            descriptor?.Dispose();
        }

        static HOGDescriptorWrapper Create() 
        {
            return new HOGDescriptorWrapper();
        }
    }
}