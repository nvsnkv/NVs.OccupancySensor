using System;
using System.Collections.Generic;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Transformation
{
    sealed class ImageTransformBuilder
    {
        private readonly Func<ILogger<ImageTransformer>> loggerFactory;

        private readonly IList<ITypedTransform> transforms = new List<ITypedTransform>();

        public ImageTransformBuilder([NotNull] Func<ILogger<ImageTransformer>> loggerFactory)
        {
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public ImageTransformBuilder Append<TColorIn, TDepthIn, TColorOut, TDepthOut>(
            [NotNull] Func<Image<TColorIn, TDepthIn>, Image<TColorOut, TDepthOut>> transformFunc)
            where TColorIn : struct, IColor
            where TDepthIn : new()
            where TColorOut : struct, IColor
            where TDepthOut : new()
        {
            if (transformFunc == null) throw new ArgumentNullException(nameof(transformFunc));
            var transform = new Transform<TColorIn, TDepthIn, TColorOut, TDepthOut>(transformFunc);

            var previousTransform = transforms.LastOrDefault();
            if ((previousTransform?.OutType ?? transform.InType) == transform.InType)
            {
                transforms.Add(transform);
            }
            else
            {
                throw new InvalidOperationException(
                    "Unable to chain transforms! result type does not match input type!")
                {
                    Data =
                    {
                        { "Current result type", previousTransform?.OutType },
                        { "Requested input type", transform.InType }
                    }
                };
            }

            return this;
        }

        public IImageTransformer ToTransformer()
        {
            var outType = transforms.LastOrDefault()?.OutType;
            if (outType != typeof(Image<Rgb, byte>))
            {
                throw new InvalidOperationException($"Current result type does differs from {typeof(Image<Rgb, int>)}!")
                {
                    Data = { { "OutType", outType } }
                };
            }
            var transformer = new ImageTransformer(loggerFactory(), transforms.ToList().AsReadOnly());
            transforms.Clear();
            return transformer;
        }
    }
}