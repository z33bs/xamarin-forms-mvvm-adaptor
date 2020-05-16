using System;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <summary>
    /// Weakly subscribe to events
    /// </summary>
    public interface IWeakEventHandler<TEventArgs> where TEventArgs : EventArgs
    {
        /// <summary>
        /// Invokes the callback
        /// </summary>
        void Handler(object sender, TEventArgs e);
    }
}