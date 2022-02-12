using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace NVs.OccupancySensor.CV.Utils
{
    public static class ObservableExtensions
    {
        public static IObservable<TOut> ToObservable<TOut>(this INotifyPropertyChanged source, string propertyName, Func<TOut> selector)
        {
            PropertyChangedEventHandler? handler = null;
            return Observable.FromEventPattern<PropertyChangedEventArgs>(
                    h => source.PropertyChanged += handler = (o, e) => h(o, e),
                    h => source.PropertyChanged -= handler
                ).Where(s => propertyName.Equals(s.EventArgs.PropertyName))
                .Select(_ => selector());
        }
    }
}