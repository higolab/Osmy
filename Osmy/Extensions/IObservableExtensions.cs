using System;
using System.Linq;
using System.Reactive.Linq;

namespace Osmy.Extensions
{
    public static class IObservableExtensions
    {
        public static IDisposable Subscribe<T>(this IObservable<T> observable, ValueChangeObserver<T> action)
        {
            return observable
                .Zip(observable.Skip(1), (x, y) => new { OldValue = x, NewValue = y })
                .Subscribe(t => action(t.OldValue, t.NewValue));
        }

        public delegate void ValueChangeObserver<T>(T oldValue, T newValue);
    }
}
