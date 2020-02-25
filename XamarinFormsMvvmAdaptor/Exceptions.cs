using System;
namespace XamarinFormsMvvmAdaptor
{
    /// <summary>The <see cref="NavController"/> is not initialized</summary>
    public class NotInitializedException : Exception
    {
        /// <summary>The <see cref="NavController"/> is not initialized</summary>
        public NotInitializedException()
        { }
        /// <summary>The <see cref="NavController"/> is not initialized</summary>
        public NotInitializedException(string message) : base(message)
        { }
        /// <summary>The <see cref="NavController"/> is not initialized</summary>
        public NotInitializedException(string message, Exception innerException) : base(message, innerException)
        { }
    }

    /// <summary>The <see cref="Xamarin.Forms.Page"/>'s BindingContext is not set</summary>
    public class BindingContextNotSetException : Exception
    {
        /// <summary>The <see cref="Xamarin.Forms.Page"/>'s BindingContext is not set</summary>
        public BindingContextNotSetException()
        { }
    }

    /// <summary>The ViewModel does not implement <see cref="IAdaptorViewModel"/></summary>
    public class NotIAdaptorViewModelException : Exception
    {
        /// <summary>The ViewModel does not implement <see cref="IAdaptorViewModel"/></summary>
        public NotIAdaptorViewModelException()
        { }
    }
    
}
