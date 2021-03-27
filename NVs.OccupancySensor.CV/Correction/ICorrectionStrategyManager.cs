using System.Net.Http.Headers;

namespace NVs.OccupancySensor.CV.Correction
{
    public interface ICorrectionStrategyManager
    {
        bool CanManage { get; }

        void LoadState();

        void SaveState();

        void ResetState();

        void SetStrategy(ICorrectionStrategy value);
    }
}