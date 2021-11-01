// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information. 

using System;
using System.Reactive.Disposables;

namespace ReactiveTests
{
    public partial class NullErrorObservable<T> : IObservable<T>
    {
        public static NullErrorObservable<T> Instance = new NullErrorObservable<T>();

        private NullErrorObservable()
        {
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            observer.OnError(null);
            return Disposable.Empty;
        }
    }
}
