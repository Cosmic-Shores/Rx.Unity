using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DefaultNamespace;
using UnityEngine;

public class Entrypoint : MonoBehaviour
{
    private CompositeDisposable _subs = new CompositeDisposable();
    
    void Start()
    {
        Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(Output).AddTo(_subs);
    }

    private void OnDestroy()
    {
        _subs.Dispose();
    }

    void Output(long _) => Debug.Log($"OUTPUT @ {DateTime.Now}");
}
