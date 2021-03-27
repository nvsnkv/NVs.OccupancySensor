using Moq;
using NVs.OccupancySensor.CV.Correction;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class StrategyManagerShould
    {
        private Mock<IStatefulCorrectionStrategy> strategy = new Mock<IStatefulCorrectionStrategy>();
        private CorrectionStrategyManager manager = new CorrectionStrategyManager();

        [Fact]
        public void NotBeAbleToManageStatelessStrategies()
        {
            manager.SetStrategy(new Mock<ICorrectionStrategy>().Object);
            Assert.False(manager.CanManage);
        }

        [Fact]
        public void BeAbleToManageStatefulStrategies()
        {
            manager.SetStrategy(strategy.Object);
            Assert.True(manager.CanManage);
        }

        [Fact]
        public void LoadStrategyState()
        {
            strategy.Setup(s => s.Load()).Verifiable("Load was not called!");

            manager.SetStrategy(strategy.Object);
            manager.LoadState();

            strategy.Verify();
        }

        [Fact]
        public void SaveStrategyState()
        {
            strategy.Setup(s => s.Save()).Verifiable("Save was not called!");

            manager.SetStrategy(strategy.Object);
            manager.SaveState();

            strategy.Verify();
        }

        [Fact]
        public void ResetStrategyState()
        {
            strategy.Setup(s => s.Reset()).Verifiable("Reset was not called!");

            manager.SetStrategy(strategy.Object);
            manager.ResetState();

            strategy.Verify();
        }
    }
}