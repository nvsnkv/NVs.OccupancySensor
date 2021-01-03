using System;
using Emgu.CV;
using Emgu.CV.BgSegm;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Transformation
{
    sealed class BackgroundSubtration: ITypedTransform
    {
        private readonly BackgroundSubtractorCNT subtractor = new BackgroundSubtractorCNT();
        
        private Image<Gray, byte> lastProcessedMask;
        public void Dispose()
        {
            subtractor?.Dispose();
        }

        public object Apply(object input)
        {
            //TODO: prevent parallel processing
            var image = (Image<Gray, byte>) input;
            var mask = new Image<Gray, byte>(image.Width, image.Height);
            
            subtractor.Apply(image, mask);
            return mask;
        }

        public ITransform Copy()
        {
            return new BackgroundSubtration();
        }

        public Type InType { get; } = typeof(Image<Gray, byte>);
        public Type OutType { get; } = typeof(Image<Gray, byte>);
    }
}