namespace NVs.OccupancySensor.CV.Settings
{
    public sealed class TransformSettings
    {
        public TransformSettings(double resizeFactor, int inputBlurKernelSize, int outputBlurKernelSize)
        {
            ResizeFactor = resizeFactor;
            InputBlurKernelSize = inputBlurKernelSize;
            OutputBlurKernelSize = outputBlurKernelSize;
        }

        public double ResizeFactor { get; }
        
        public int InputBlurKernelSize { get; }
        
        public int OutputBlurKernelSize { get; }

        public static TransformSettings Default { get; } = new TransformSettings(0.5, 5, 5);
    }
}