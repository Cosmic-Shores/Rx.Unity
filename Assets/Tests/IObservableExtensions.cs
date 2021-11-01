using System;
using System.Reactive.Disposables;

namespace DefaultNamespace
{
    public static class IObservableExtensions
    {
        public static IDisposable AddTo(this IDisposable disposable, CompositeDisposable compositeDisposable)
        {
            compositeDisposable.Add(disposable);
            return disposable;
        }
    }
}