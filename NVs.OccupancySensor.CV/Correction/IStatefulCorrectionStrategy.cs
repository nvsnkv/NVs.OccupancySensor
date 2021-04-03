namespace NVs.OccupancySensor.CV.Correction
{
    public interface IStatefulCorrectionStrategy : ICorrectionStrategy 
    {
        void Load();
        
        void Save();

        void Reset();
    }
}