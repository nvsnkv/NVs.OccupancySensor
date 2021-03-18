using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Detection.Correction
{
    public interface ICorrectionStrategy
    {
        Image<Gray, byte> Apply(Image<Gray, byte> source);
    }

    public interface IStatefulCorrectionStrategy : ICorrectionStrategy 
    {
        void Load();
        
        void Save();

        bool IsLearning  { get; }

        void Reset();
    }

}