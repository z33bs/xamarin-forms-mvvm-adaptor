using System;
using System.Runtime.CompilerServices;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    public interface IWeakEventManager
    {
        void AddEventHandler<TEventArgs>(EventHandler<TEventArgs> handler, [CallerMemberName] string eventName = null) where TEventArgs : EventArgs;
        void AddEventHandler(EventHandler handler, [CallerMemberName] string eventName = null);
        void HandleEvent(object sender, object args, string eventName);
        void RemoveEventHandler<TEventArgs>(EventHandler<TEventArgs> handler, [CallerMemberName] string eventName = null) where TEventArgs : EventArgs;
        void RemoveEventHandler(EventHandler handler, [CallerMemberName] string eventName = null);
    }
}