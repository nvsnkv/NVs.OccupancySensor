﻿using System;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Denoising.Denoisers
{
    internal sealed class FastNlMeansColoredDenoiser : IDenoisingStrategy
    {
        private readonly IFastNlMeansColoredDenoisingSettings settings;

        public FastNlMeansColoredDenoiser([NotNull] IFastNlMeansColoredDenoisingSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public Image<Gray, byte> Denoise([NotNull] Image<Gray, byte> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var result = new Image<Gray, byte>(source.Width, source.Height);
            CvInvoke.FastNlMeansDenoisingColored(source, result, settings.H, settings.HColor, settings.TemplateWindowSize, settings.SearchWindowSize);
            return result;
        }
    }
}