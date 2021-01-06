namespace NVs.OccupancySensor.CV.Utils
{
    internal static class LogUtils
    {
        public static string GetHashString(this object obj)
        {
            return obj == null ? "null" : obj.GetHashCode().ToString("X");
        }
    }
}