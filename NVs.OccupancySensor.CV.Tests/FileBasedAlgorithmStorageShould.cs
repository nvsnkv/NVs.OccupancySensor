using NVs.OccupancySensor.CV.Transformation.Background;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class FileBasedAlgorithmStorageShould
    {
        private const string DataDir = "test_algorithms";

        [Fact]
        public void ReturnNullWhenAlgorithmDoesNotExists()
        {
            var storage = new FileBasedAlgorithmStorage(DataDir);
            Assert.Null(storage.GetAlgorithm("some random name"));
        }
        
        [Fact]
        public void SaveAndLoadPreviouslySavedAlgorithm()
        {
            var name = "algo1";
            var content = "algo1";
            
            var storage = new FileBasedAlgorithmStorage(DataDir);
            storage.SaveAlgorithm(name, content);
            
            Assert.Equal(content, storage.GetAlgorithm(name));
        }
    }
}