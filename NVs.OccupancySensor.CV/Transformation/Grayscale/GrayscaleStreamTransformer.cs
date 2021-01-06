using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Transformation.Grayscale
{
    internal sealed class GrayscaleStreamTransformer : IGrayscaleStreamTransformer
    {
        private static readonly IReadOnlyList<IObservable<Image<Gray, byte>>> EmptyList = new List<IObservable<Image<Gray, byte>>>().AsReadOnly();
        private readonly ILogger<GrayscaleStreamTransformer> logger;
        private readonly List<IGrayscaleTransform> transforms;

        public GrayscaleStreamTransformer(ILogger<GrayscaleStreamTransformer> logger, IList<IGrayscaleTransform> transforms)
        {
            if (transforms is null)
            {
                throw new ArgumentNullException(nameof(transforms));
            }

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.transforms = transforms.Select((t, i) => new TransformWrapper(t, logger, i))
                .Cast<IGrayscaleTransform>()
                .ToList();

            OutputStreams = EmptyList;
        }

        public IObservable<Image<Rgb, byte>> InputStream {get; private set; }

        public IReadOnlyList<IObservable<Image<Gray, byte>>> OutputStreams  {get; private set;}

        public void Dispose()
        {
            foreach(var transform in transforms)
            {
                transform.Dispose();
            }
        }

        public void RebuildStreams(IObservable<Image<Rgb, byte>> input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var streams = new List<IObservable<Image<Gray, byte>>>();
            var stream = input.Select(ConvertToGrayscale);
            streams.Add(stream);

            for (var i=0; i < transforms.Count; i++) 
            {
                stream = stream.Select(transforms[i].Apply);
                streams.Add(stream);
            }
            
            OutputStreams = streams.AsReadOnly();
            InputStream = input;
        }

        private Image<Gray, byte> ConvertToGrayscale(Image<Rgb, byte> input)
        {
            Image<Gray,byte> result;
            logger.LogInformation("Converting new image to grayscale");
            
            try
            {
                result = input.Convert<Gray, byte>();
                logger.LogInformation("Image successfully converted.");
            }
            catch(Exception e) 
            {
                logger.LogError(e, "Failed to convert image to grayscale!");
                throw;
            }

            return result;
        }

        private class TransformWrapper : IGrayscaleTransform 
        {
            private readonly IGrayscaleTransform transform;

            private readonly ILogger<GrayscaleStreamTransformer> logger;

            private readonly int order;

            public TransformWrapper(IGrayscaleTransform transform, ILogger<GrayscaleStreamTransformer> logger, int order)
            {
                this.transform = transform ?? throw new ArgumentNullException(nameof(transform));
                this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                this.order = order;
            }


            public Image<Gray, byte> Apply(Image<Gray, byte> input)
            {
                if (input is null)
                {
                    throw new ArgumentNullException(nameof(input));
                }

                logger.LogInformation($"Applying transform [{order}]...");
                Image<Gray,byte> result;
                try
                {
                    result = transform.Apply(input);
                    logger.LogInformation($"Transform [{order}] applied");
                }
                catch(Exception e)
                {
                    logger.LogError(e, $"Failed to apply transform [{order}]");
                    throw;
                }

                return result;
            }

            public void Dispose()
            {
                logger.LogInformation($"Disposing transform [{order}]...");
                transform.Dispose();
                logger.LogInformation($"Transform [{order}] disposed");
            }
        }
    }
}
