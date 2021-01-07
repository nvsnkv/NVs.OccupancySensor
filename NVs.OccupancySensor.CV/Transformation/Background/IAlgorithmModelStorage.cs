namespace NVs.OccupancySensor.CV.Transformation.Background
{
    public interface IAlgorithmModelStorage
    {
        string GetAlgorithm(string name);
        
        void SaveAlgorithm(string name, string content);
    }
}