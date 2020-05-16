using System;
using System.ComponentModel;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <summary>
    /// For unit testing and mocking of <see cref="SafeActionExtensions"/>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ISafeAction
    {
        /// <summary>
        /// For unit testing and mocking of <see cref="SafeActionExtensions"/>
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        void SafeInvoke<TException>(Action<object> action, object parameter, Action<TException> onException) where TException : Exception;

        /// <summary>
        /// For unit testing and mocking of <see cref="SafeActionExtensions"/>
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        void SafeInvoke<TException>(Action action, Action<TException> onException) where TException : Exception;
    }
}