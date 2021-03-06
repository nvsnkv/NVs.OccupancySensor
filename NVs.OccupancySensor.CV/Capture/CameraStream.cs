﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Utils.Flow;

namespace NVs.OccupancySensor.CV.Capture
{
    internal sealed class CameraStream : Stream<Image<Rgb, byte>>, ICameraStream
    {
        private readonly VideoCapture videoCapture;
        private readonly TimeSpan frameInterval;

        private int framesCaptured;
        
        public CameraStream(VideoCapture videoCapture, CancellationToken ct, ILogger<CameraStream> logger, TimeSpan frameInterval)
            :base(ct, logger)
        {
            this.videoCapture = videoCapture ?? throw new ArgumentNullException(nameof(videoCapture));
            this.frameInterval = frameInterval;
            
            Task.Run(QueryFrames, ct);
        }
              
        private async Task QueryFrames()
        {
            while (!Ct.IsCancellationRequested)
            {
                Logger.LogInformation($"Capturing frame {framesCaptured + 1}");
                Mat frame;
                try
                {
                    frame = videoCapture.QueryFrame();
                    Logger.LogInformation("Got new frame");
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Unable to query frame!");
                    Notify(o => o.OnError(e));
                    Notify(o => o.OnCompleted());

                    return;
                }

                if (frame != null)
                {
                    Image<Rgb, byte> image;
                    try
                    {
                        image = frame.ToImage<Rgb, byte>();
                        Logger.LogInformation("Frame successfully converted to image!");
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, "Failed to convert frame to image!");
                        throw;
                    }
                    Notify(o => o.OnNext(image));
                }
                else
                {
                    Logger.LogWarning("null frame received");
                }

                
                ++framesCaptured;
                Logger.LogInformation($"Frame {framesCaptured} processed");
                
                if (framesCaptured == int.MaxValue - 1)
                {
                    Logger.LogInformation("Resetting captured frames counter since it reached int.MaxValue - 1");
                    framesCaptured = 0;
                }
                
                await Task.Delay(frameInterval, Ct);
            }
        }
    }
}