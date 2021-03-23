using System;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using NVs.OccupancySensor.CV.Correction;
using NVs.OccupancySensor.CV.Settings.Correction;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    [Collection("Run Exclusively")]
    public sealed class StaticMaskCorrectionStrategyShould : IDisposable
    {
        private static readonly string MaskPath = "mask.bmp";
        private readonly StaticMaskCorrectionStrategy strategy = new StaticMaskCorrectionStrategy(new StaticMaskSettings(MaskPath));
        private readonly Image<Gray, byte> mask = new Image<Gray, byte>(2, 2)
        {
            [0, 0] = new Gray(255),
            [0, 1] = new Gray(255),
            [1, 0] = new Gray(0),
            [1, 1] = new Gray(0)
        };

        [Fact]
        public void ApplyTheMaskToTheInputImage()
        {
            var image = new Image<Gray, byte>(2, 2)
            {
                [0, 0] = new Gray(0),
                [0, 1] = new Gray(255),
                [1, 0] = new Gray(255),
                [1, 1] = new Gray(0)
            };

            mask.Save(MaskPath);
            strategy.Load();

            var result = strategy.Apply(image);

            Assert.Equal(new Gray(0), result[0, 0]);
            Assert.Equal(new Gray(255), result[0, 1]);
            Assert.Equal(new Gray(0), result[1, 0]);
            Assert.Equal(new Gray(0), result[1, 1]);
        }

        [Fact]
        public void SaveMask()
        {
            mask.Save(MaskPath);
            strategy.Load();

            File.Delete(MaskPath);
            Assert.False(File.Exists(MaskPath));

            strategy.Save();
            Assert.True(File.Exists(MaskPath));

            var savedMask = new Image<Gray, byte>(MaskPath);
            Assert.Equal(0, savedMask.AbsDiff(mask).CountNonzero()[0]);
        }

        [Fact]
        public void UseSolidMaskIfNothingWasLoaded()
        {
            var image = new Image<Gray, byte>(2, 2)
            {
                [0, 0] = new Gray(0),
                [0, 1] = new Gray(255),
                [1, 0] = new Gray(255),
                [1, 1] = new Gray(0)
            };

            var result = strategy.Apply(image);

            Assert.Equal(image, result);
        }

        [Fact]
        public void UseSolidMaskAfterReset()
        {
            var image = new Image<Gray, byte>(2, 2)
            {
                [0, 0] = new Gray(0),
                [0, 1] = new Gray(255),
                [1, 0] = new Gray(255),
                [1, 1] = new Gray(0)
            };

            mask.Save(MaskPath);
            strategy.Load();
            strategy.Reset();

            var result = strategy.Apply(image);

            Assert.Equal(image, result);
        }

        [Fact]
        public void AdjustsMaskIfResetRequested()
        {
            var image = new Image<Gray, byte>(2, 2)
            {
                [0, 0] = new Gray(255),
                [0, 1] = new Gray(0),
                [1, 0] = new Gray(0),
                [1, 1] = new Gray(0)
            };

            strategy.Apply(image);
            strategy.Reset();

            var counter = 0;
            while (counter++ < 100000)
            {
                strategy.Apply(image);
            }

            var result = strategy.Apply(image);
            Assert.Equal(0, result.GetAverage().Intensity);
        }

        [Fact]
        public void StopMaskAdjustmentWhenSaveRequested()
        {
            var image = new Image<Gray, byte>(2, 2)
            {
                [0, 0] = new Gray(255),
                [0, 1] = new Gray(0),
                [1, 0] = new Gray(0),
                [1, 1] = new Gray(0)
            };

            strategy.Apply(image);
            strategy.Reset();
            strategy.Apply(image);
            strategy.Save();

            var counter = 0;
            while (counter++ < 100000)
            {
                strategy.Apply(image);
            }

            var result = strategy.Apply(image);
            Assert.Equal(255, result[0, 0].Intensity);
            Assert.Equal(0, result[0, 1].Intensity);
            Assert.Equal(0, result[1, 0].Intensity);
            Assert.Equal(0, result[1, 1].Intensity);
        }

        public void Dispose()
        {
            if (File.Exists(MaskPath))
            {
                File.Delete(MaskPath);
            }
        }
    }
}