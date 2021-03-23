using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Utils.Flow
{
    internal abstract class Stage<TIn, TOut> : IObserver<TIn> where TIn: class
    {
        private readonly ProcessingLock processingLock = new ProcessingLock();
        private readonly object streamLock = new object();
        protected ILogger Logger;
        protected volatile ProcessingStream<TIn, TOut> OutputStream;
        protected readonly Counter Counter;

        public IObservable<TOut> Output => OutputStream;

        protected Stage()
        {
            Counter = new Counter();
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

        public void OnNext([NotNull] TIn value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            Logger.LogInformation("New frame received...");

            if (!processingLock.Acquire())
            {
                Logger.LogWarning("Previously started operation is still in progress, the frame will be dropped!");
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

        public IStatistics Statistics => Counter;

        public virtual event PropertyChangedEventHandler PropertyChanged;

        protected void ReplaceStream(ProcessingStream<TIn, TOut> expectedStream, ProcessingStream<TIn, TOut> newStream)
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

        protected abstract ProcessingStream<TIn, TOut> CreateStream();
    }
}