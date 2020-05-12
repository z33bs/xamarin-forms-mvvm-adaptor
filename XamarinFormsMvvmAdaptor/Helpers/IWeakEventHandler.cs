using System;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    public interface IWeakEventHandler<TEventArgs> where TEventArgs : EventArgs
    {
        void Handler(object sender, TEventArgs e);
    }
}