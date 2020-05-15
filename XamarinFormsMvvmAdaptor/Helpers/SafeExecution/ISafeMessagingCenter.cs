using System;
using System.Threading.Tasks;

// Adapted from Xamarin.Forms https://github.com/xamarin/Xamarin.Forms

namespace XamarinFormsMvvmAdaptor.Helpers
{
	/// <summary>
    /// Associates a callback on subscribers with a specific message name
	/// </summary>
	public interface ISafeMessagingCenter
	{
		/// <summary>
		/// Sends a named message
		/// </summary>
		void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class;
		///<inheritdoc cref="Send{TSender, TArgs}(TSender, string, TArgs)"/>
		void Send<TSender>(TSender sender, string message) where TSender : class;

		/// <summary>
		/// Unsubscribes from the specified subscriber
		/// </summary>
		void Unsubscribe<TSender, TArgs>(object subscriber, string message) where TSender : class;
		///<inheritdoc cref="Unsubscribe{TSender, TArgs}(object, string)"/>
		void Unsubscribe<TSender>(object subscriber, string message) where TSender : class;
		///<inheritdoc cref="Unsubscribe{TSender, TArgs}(object, string)"/>
		void UnsubscribeAnySender<TArgs>(object subscriber, string message);
		///<inheritdoc cref="Unsubscribe{TSender, TArgs}(object, string)"/>
		void UnsubscribeAnySender(object subscriber, string message);

		#region Subscribe
		#region Actions
		/// <summary>
		/// Subscribe to a specified message, and register a callback to execute when the message is recieved
		/// </summary>
		/// <typeparam name="TSender">Sender Type. Callback will only execute if recieved
        /// from sender of type <typeparamref name="TSender"/></typeparam>
		/// <typeparam name="TArgs">Arguments Type. Callback expects arguements of type <typeparamref name="TArgs"/></typeparam>
		/// <param name="subscriber">The subscriber. Usually <c>this</c>.</param>
		/// <param name="message">Will only execute the callback when the specified message is recieved</param>
		/// <param name="callback">Callback to execute</param>
        /// <param name="asyncCallback">Asynchronous callback to execute</param>
		/// <param name="onException">Callback to execute if Exception is caught</param>
		/// <param name="source">Instance of the source that will send the message.
        /// If specified, callback will only execute if the sender is from the
        /// specified source. If not, callback will execute if the sender's type is equal to <typeparamref name="TSender"/></param>
        /// <param name="isBlocking">If specified, will block execution of the callback if the callback is already busy executing.</param>
        /// <param name="viewModel">If specified will update <see cref="IViewModelBase"/>'s <c>IsBusy</c></param>
        /// property, and block the callback's execution if its already busy.
		void Subscribe<TSender, TArgs>(
			object subscriber,
			string message,
			Action<TSender,TArgs> callback,
			IViewModelBase viewModel = null,
			Action<Exception> onException = null,
			TSender source = null,
			bool isBlocking = true) where TSender : class;


		void Subscribe<TSender>(
			object subscriber,
			string message,
			Action<TSender> callback,
			IViewModelBase viewModel = null,
			Action<Exception> onException = null,
			TSender source = null,
			bool isBlocking = true) where TSender : class;

		void SubscribeAnySender<TArgs>(
			object subscriber,
			string message,
			Action<object, TArgs> callback,
			IViewModelBase viewModel = null,
			Action<Exception> onException = null,
			bool isBlocking = true);

		void SubscribeAnySender(
			object subscriber,
			string message,
			Action<object> callback,
			IViewModelBase viewModel = null,
			Action<Exception> onException = null,
			bool isBlocking = true);

		#endregion
		#region Functions
		void Subscribe<TSender, TArgs>(
			object subscriber,
			string message,
			Func<TSender, TArgs, Task> asyncCallback,
			IViewModelBase viewModel = null,
			Action<Exception> onException = null,
			TSender source = null,
			bool isBlocking = true) where TSender : class;

		void Subscribe<TSender>(
			object subscriber,
			string message,
			Func<TSender, Task> asyncCallback,
			IViewModelBase viewModel = null,
			Action<Exception> onException = null,
			TSender source = null,
			bool isBlocking = true) where TSender : class;

		void SubscribeAnySender<TArgs>(
			object subscriber,
			string message,
			Func<object, TArgs, Task> asyncCallback,
			IViewModelBase viewModel = null,
			Action<Exception> onException = null,
			bool isBlocking = true);

		void SubscribeAnySender(
			object subscriber,
			string message,
			Func<object, Task> asyncCallback,
			IViewModelBase viewModel = null,
			Action<Exception> onException = null,
			bool isBlocking = true);

        #endregion
        #endregion
    }
}
