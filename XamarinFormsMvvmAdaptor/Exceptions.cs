using System;
namespace XamarinFormsMvvmAdaptor
{
    /// <summary>The <see cref="NavController"/> is not initialized</summary>
    public class NotInitializedException : Exception
    {
        /// <summary>The <see cref="NavController"/> is not initialized</summary>
        public NotInitializedException(string message) : base(message)
        { }
        /// <summary>The <see cref="NavController"/> is not initialized</summary>
        public NotInitializedException(string message, Exception innerException) : base(message, innerException)
        { }
    }    
}
