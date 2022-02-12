using System;
using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Correction
{
    internal sealed class CorrectionStrategyManager : ICorrectionStrategyManager
    {
        private ICorrectionStrategy strategy;

        public bool CanManage => strategy is IStatefulCorrectionStrategy;
        public void LoadState()
        {
            (strategy as IStatefulCorrectionStrategy)?.Load();
        }

        public void SaveState()
        {
            (strategy as IStatefulCorrectionStrategy)?.Save();
        }

        public void ResetState()
        {
            (strategy as IStatefulCorrectionStrategy)?.Reset();
        }

        public void SetStrategy(ICorrectionStrategy value)
        {
            this.strategy = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}