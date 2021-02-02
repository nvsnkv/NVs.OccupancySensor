using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Transformation
{
    internal static class Transforms
    {
        public static Func<Image<Gray, byte>, Image<Gray, byte>> Resize(double resizeFactor)
        {
            return Math.Abs(resizeFactor - 1) <= double.Epsilon ? Bypass : i => i.Resize(resizeFactor, Inter.Linear);
        }
        
        public static Func<Image<Gray, byte>, Image<Gray, byte>> MedianBlur(int ksize)
        {
            return ksize == 0
                ? Bypass
                : i =>
                {
                    Image<Gray, byte> denoised = new Image<Gray, byte>(i.Width, i.Height);
                    CvInvoke.MedianBlur(i, denoised, ksize);
                    return denoised;
                };
        }

        private static Func<Image<Gray, byte>, Image<Gray, byte>> Bypass { get; } = i => i;
    }
}