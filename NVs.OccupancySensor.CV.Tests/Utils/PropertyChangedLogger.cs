using System;
using System.Collections.Generic;
using System.ComponentModel;
using Emgu.CV.Aruco;

namespace NVs.OccupancySensor.CV.Tests.Utils
{
    internal class PropertyChangedLogger
    {
        public readonly Dictionary<object, IDictionary<DateTime, string>> Notifications = new Dictionary<object, IDictionary<DateTime, string>>();

        public void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!Notifications.ContainsKey(sender))
            {
                Notifications[sender] = new Dictionary<DateTime, string>();
            }
            
            Notifications[sender].Add(DateTime.Now, e.PropertyName);
        }
    }
}