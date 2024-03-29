﻿using System;
using Emgu.CV;
using Emgu.CV.Structure;
using NVs.OccupancySensor.CV.Utils;

namespace NVs.OccupancySensor.CV.Correction
{
    public interface ICorrector : IObserver<Image<Gray,byte>>
    {
    IObservable<Image<Gray, byte>> Output { get; }

    IStatistics Statistics { get; }

    void Reset();
    }
}