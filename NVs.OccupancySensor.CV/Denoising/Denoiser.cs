using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Denoising.Denoisers;
using NVs.OccupancySensor.CV.Utils;

namespace NVs.OccupancySensor.CV.Denoising
{
    internal sealed class Denoiser : IDenoiser
    {
        private readonly ProcessingLock processingLock = new ProcessingLock();
        private readonly object streamLock = new object();
        private readonly ILogger<Denoiser> logger;
        private readonly IDenoiserFactory factory;
        
        private volatile DenoisingStream output;
        private IDenoisingSettings settings;

        public Denoiser([NotNull] IDenoiserFactory factory, [NotNull] IDenoisingSettings settings, [NotNull] ILogger<Denoiser> logger)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            output = new DenoisingStream(factory.Create(settings.Algorithm), CancellationToken.None, logger);
        }

        public void OnCompleted()
        {
            logger.LogInformation("Stream Completed. Setting Output to null.");
            output.Complete();
        }

        public void OnError(Exception error)
        {
            logger.LogWarning($"Error received! Setting output to null.{Environment.NewLine}, Exception:{error}");
            output.Error(error);
            output.Complete();
        }

        public void OnNext([NotNull] Image<Rgb, byte> value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            logger.LogInformation("New frame received...");

            if (!processingLock.Acquire())
            {
                logger.LogWarning("Previously started operation is still in progress, the frame will be dropped!");
                return;
            }

            try
            {
                if (output.Completed)
                {
                    Reset();
                }

                output.Process(value);
                logger.LogInformation("Noise filter applied.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to apply noise filter!");
                throw;
            }
            finally
            {
                processingLock.Release();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IObservable<Image<Rgb, byte>> Output => output;

        [NotNull]
        public IDenoisingSettings Settings
        {
            get => settings;
            set
            {
                if (Equals(value ?? throw new ArgumentNullException(nameof(value)), settings)) return;
                settings = value;
                OnPropertyChanged();
            }
        }

        public void Reset()
        {
            var stream = new DenoisingStream(factory.Create(Settings.Algorithm), CancellationToken.None, logger);
            ReplaceStream(output, stream);
        }

        private void ReplaceStream(DenoisingStream expectedStream, DenoisingStream newStream)
        {
            if (output != expectedStream) return;

            lock (streamLock)
            {
                if (output != expectedStream) return;
                output = newStream;
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}