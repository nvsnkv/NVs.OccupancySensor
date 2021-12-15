using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace NVs.OccupancySensor.CV.Tests.Utils
{
    internal class PropertyChangedLogger
    {
        public readonly Dictionary<object, IDictionary<DateTime, string>> Notifications = new();

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