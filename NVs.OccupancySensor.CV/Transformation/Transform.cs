using System;
using Emgu.CV;
using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Transformation
{
    sealed class Transform<TColorIn, TDepthIn, TColorOut, TDepthOut> : ITypedTransform
        where TColorIn : struct, IColor
        where TDepthIn : new()
        where TColorOut : struct, IColor
        where TDepthOut : new()
    {
        private readonly Func<Image<TColorIn, TDepthIn>, Image<TColorOut, TDepthOut>> transform;

        public Transform([NotNull] Func<Image<TColorIn, TDepthIn>, Image<TColorOut, TDepthOut>> transform)
        {
            this.transform = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        public Type InType { get; } = typeof(Image<TColorIn, TDepthIn>);

        public Type OutType { get; } = typeof(Image<TColorOut, TDepthOut>);

        public object Apply(object input)
        {
            return transform((Image<TColorIn, TDepthIn>)input);
        }

        public ITransform Copy()
        {
            return new Transform<TColorIn, TDepthIn, TColorOut, TDepthOut>(transform);
        }

        public void Dispose()
        {
        }
    }
}