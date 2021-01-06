using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Transformation.Grayscale
{
    internal sealed class GrayscaleStreamTransformerBuilder
    {
        private readonly List<IGrayscaleTransform> transforms = new List<IGrayscaleTransform>();
        private readonly Func<ILogger<GrayscaleStreamTransformer>> loggerFunc;

        public GrayscaleStreamTransformerBuilder(Func<ILogger<GrayscaleStreamTransformer>> loggerFunc)
        {
            this.loggerFunc = loggerFunc ?? throw new System.ArgumentNullException(nameof(loggerFunc));
        }

        public GrayscaleStreamTransformerBuilder Append(Func<Image<Gray, byte>, Image<Gray, byte>> transformFunc)
        {
            if (transformFunc is null)
            {
                throw new ArgumentNullException(nameof(transformFunc));
            }

            var transform = new GenericGrayscaleTransform(transformFunc);
            return Append(transform);
        }

        public GrayscaleStreamTransformerBuilder Append(IGrayscaleTransform transform)
        {
            if (transform is null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            transforms.Add(transform);

            return this;
        }

        public GrayscaleStreamTransformerBuilder Synchronized()
        {
            if (transforms.Count == 0)
            {
                throw new InvalidOperationException("There is no transform to wrap in synchronized transformation!");
            }

            transforms[transforms.Count - 1] = new SynchronizedTransform(transforms[transforms.Count - 1]);

            return this;
        }

        public IGrayscaleStreamTransformer ToTransformer()
        {
            return new GrayscaleStreamTransformer(loggerFunc(), transforms);
        }

        private class GenericGrayscaleTransform : IGrayscaleTransform
        {
            private readonly Func<Image<Gray, byte>, Image<Gray, byte>> func;

            public GenericGrayscaleTransform(Func<Image<Gray, byte>, Image<Gray, byte>> func) => this.func = func ?? throw new ArgumentNullException(nameof(func));

            public Image<Gray, byte> Apply(Image<Gray, byte> input)
            {
                return func(input);
            }

            public void Dispose()
            {
            }
        }
    }
}