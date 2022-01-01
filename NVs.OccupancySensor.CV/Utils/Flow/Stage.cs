using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Utils.Flow
{
    internal abstract class Stage : IObserver<Image<Gray, byte>>
    {
        private readonly ProcessingLock processingLock = new ProcessingLock();
        private readonly object streamLock = new object();
        protected readonly Counter Counter = new Counter();
        protected readonly ILogger Logger;

        protected volatile ProcessingStream OutputStream;
        

        public IObservable<Image<Gray, byte>> Output => OutputStream;

        protected Stage([NotNull] ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void OnCompleted()
        {
            Logger.LogInformation("Stream Completed. Setting Output to null.");
            OutputStream.Complete();
        }

        public void OnError(Exception error)
        {
            Logger.LogWarning($"Error received! Setting output to null.{Environment.NewLine}, Exception:{error}");
            OutputStream.Error(error);
            OutputStream.Complete();
        }

        public void OnNext([NotNull] Image<Gray, byte> value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            Logger.LogInformation("New frame received...");

            if (!processingLock.Acquire())
            {
                Logger.LogWarning("Previously started operation is still in progress, the frame will be dropped!");
                Counter.IncreaseDropped();
                return;
            }

            try
            {
                if (OutputStream?.Completed ?? true)
                {
                    ReplaceStream(OutputStream, CreateStream());
                }

                OutputStream.Process(value);
                Logger.LogInformation("Noise filter applied.");
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to apply noise filter!");
                throw;
            }
            finally
            {
                processingLock.Release();
            }
        }

        public void Reset()
        {
            var stream = CreateStream();
            ReplaceStream(OutputStream, stream);
        }

        public IStatistics Statistics => Counter;

        public virtual event PropertyChangedEventHandler PropertyChanged;

        private void ReplaceStream(ProcessingStream expectedStream, ProcessingStream newStream)
        {
            if (OutputStream != expectedStream) return;

            lock (streamLock)
            {
                if (OutputStream != expectedStream) return;
                OutputStream = newStream;
            }

            expectedStream.Complete();
            OnPropertyChanged(nameof(Output));
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected abstract ProcessingStream CreateStream();
    }
}