//From Xamarin.Forms https://github.com/xamarin/Xamarin.Forms
using System;
namespace XamarinFormsMvvmAdaptor.Helpers
{
	/// <summary>
	/// Interface for <see cref="Xamarin.Forms.MessagingCenter"/> to avoid
    /// unnecessary `using Xamarin.Forms;` reference in your ViewModels
    /// Associates a callback on subscribers with a specific message name
	/// </summary>
	public interface IMessagingCenter
	{
		/// <summary>
		/// Sends a named message with the specified arguments
		/// </summary>
		void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class;

		/// <summary>
		/// Sends a named message that has no arguments
		/// </summary>
		void Send<TSender>(TSender sender, string message) where TSender : class;

		/// <summary>
		/// Run the callback on subscriber in response to parameterised messages that are named message and that are created by source.
		/// </summary>
		void Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, TSender source = null) where TSender : class;

		/// <summary>
		/// Run the callback on subscriber in response to messages that are named message and that are created by source.
		/// </summary>
		void Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, TSender source = null) where TSender : class;

		/// <summary>
		/// Unsubscribes from the specified parameterless subscriber messages
		/// </summary>
		void Unsubscribe<TSender, TArgs>(object subscriber, string message) where TSender : class;

		/// <summary>
		/// Unsubscribes a subscriber from the specified messages that come from the specified sender
		/// </summary>
		void Unsubscribe<TSender>(object subscriber, string message) where TSender : class;

		#region Custom Overloads - without Sender
		void UnfilteredSubscribe<TArgs>(object subscriber, string message, Action<object,TArgs> callback);
		void UnfilteredSubscribe(object subscriber, string message, Action<object> callback);
		//void AnonymousSend<TArgs>(string message, TArgs args);
		//void AnonymousSend(string message);
		void UnfilteredUnsubscribe<TArgs>(object subscriber, string message);
		void UnfilteredUnsubscribe(object subscriber, string message);
		#endregion
	}
}
