using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Transformation
{
    internal static class Transforms
    {
        public static Func<Image<Gray, byte>, Image<Gray, byte>> MedianBlur(int ksize)
        {
            return i =>
            {
                Image<Gray, byte> denoised = new Image<Gray, byte>(i.Width, i.Height);
                CvInvoke.MedianBlur(i, denoised, ksize);
                return denoised;
            };
        }
    }
}