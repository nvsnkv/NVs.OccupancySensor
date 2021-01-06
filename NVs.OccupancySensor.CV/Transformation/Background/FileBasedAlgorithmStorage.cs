using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Transformation.Background
{
    internal sealed class FileBasedAlgorithmStorage : IAlgorithmModelStorage
    {
        private readonly DirectoryInfo dataDir;

        public FileBasedAlgorithmStorage([NotNull] string dataPath)
        {
            dataDir = new DirectoryInfo(dataPath ?? throw new ArgumentNullException(nameof(dataPath)));
            if (!dataDir.Exists)
            {
                dataDir.Create();
            }
        }

        public string GetAlgorithm([NotNull] string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            var file = dataDir.GetFiles(name).FirstOrDefault();
            if (!(file?.Exists ?? false))
            {
                return null;
            }

            using (var reader = new StreamReader(file.OpenRead()))
            {
                return reader.ReadToEnd();
            }
        }

        public void SaveAlgorithm([NotNull] string name, [NotNull] string content)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (content == null) throw new ArgumentNullException(nameof(content));
            
            using(var writer = new StreamWriter(File.OpenWrite(Path.Combine(dataDir.FullName, name))))
            {
                writer.Write(content);
                writer.Flush();
            }
        }
    }
}