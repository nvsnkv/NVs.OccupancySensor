using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Denoising;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class DenoiserShould
    {
        private readonly Mock<ILogger<Denoiser>> logger = new Mock<ILogger<Denoiser>>();

        //TODO
    }
}