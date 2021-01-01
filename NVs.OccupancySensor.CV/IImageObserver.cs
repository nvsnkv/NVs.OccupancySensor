﻿using System;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV
{
    public interface IImageObserver : IObserver<Image<Rgb, float>>
    {
        Task<Image<Rgb, float>> GetImage();
    }
}