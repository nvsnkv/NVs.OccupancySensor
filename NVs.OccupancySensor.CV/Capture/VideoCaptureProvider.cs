using Emgu.CV;
using NVs.OccupancySensor.CV.Settings;

namespace NVs.OccupancySensor.CV.Capture
{
    internal class VideoCaptureProvider
    {
        private volatile VideoCapture capture;
        private readonly object thisLock = new object();
        private readonly CaptureSettings settings;

        public VideoCaptureProvider(CaptureSettings settings)
        {
            this.settings = settings;
        }

        public VideoCapture Get()
        {
            if (capture != null)
            {
                return capture;
            }

            lock (thisLock)
            {
                if (capture == null)
                {
                    capture = int.TryParse(settings.Source, out var camIndex)
                        ? new VideoCapture(camIndex)
                        : new VideoCapture(settings.Source);
                }
            }

            return capture;
        }
    }
}