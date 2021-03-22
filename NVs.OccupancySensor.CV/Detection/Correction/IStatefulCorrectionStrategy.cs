namespace NVs.OccupancySensor.CV.Detection.Correction
{
    public interface IStatefulCorrectionStrategy : ICorrectionStrategy 
    {
        void Load();
        
        void Save();

        void Reset();
    }
}