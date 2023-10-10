namespace NetPs.Socket
{
    using System;
    using System.Reactive.Disposables;

    public interface IDisposables
    {
        CompositeDisposable Disposables { get; }
        void AddDispose(IDisposable disposable);
    }
}
