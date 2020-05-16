using System;
using System.Runtime.CompilerServices;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <summary>
    /// Weak event manager to subscribe and unsubscribe from events.
    /// </summary>
    public interface IWeakEventManager
    {
        /// <summary>
        /// Add an event handler to the manager.
        /// </summary>
        /// <typeparam name="TEventArgs">Event handler of T</typeparam>
        /// <param name="handler">Handler of the event</param>
        /// <param name="eventName">Name to use in the dictionary. Should be unique.</param>
        void AddEventHandler<TEventArgs>(EventHandler<TEventArgs> handler, [CallerMemberName] string eventName = null) where TEventArgs : EventArgs;
        /// <summary>
        /// Add an event handler to the manager.
        /// </summary>
        /// <param name="handler">Handler of the event</param>
        /// <param name="eventName">Name to use in the dictionary. Should be unique.</param>
        void AddEventHandler(EventHandler handler, [CallerMemberName] string eventName = null);
        /// <summary>
        /// Handle an event
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">Arguments for the event</param>
        /// <param name="eventName">Name of the event.</param>
        void HandleEvent(object sender, object args, string eventName);
        /// <summary>
        /// Remove an event handler.
        /// </summary>
        /// <typeparam name="TEventArgs">Type of the EventArgs</typeparam>
        /// <param name="handler">Handler to remove</param>
        /// <param name="eventName">Event name to remove</param>
        void RemoveEventHandler<TEventArgs>(EventHandler<TEventArgs> handler, [CallerMemberName] string eventName = null) where TEventArgs : EventArgs;
        /// <summary>
        /// Remove an event handler.
        /// </summary>
        /// <param name="handler">Handler to remove</param>
        /// <param name="eventName">Event name to remove</param>
        void RemoveEventHandler(EventHandler handler, [CallerMemberName] string eventName = null);
    }
}