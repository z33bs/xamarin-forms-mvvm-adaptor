using System;

namespace XamarinFormsMvvmAdaptor
{
    public interface IWeakEventHandler<TEventArgs> where TEventArgs : EventArgs
    {
        void Handler(object sender, TEventArgs e);
    }
}