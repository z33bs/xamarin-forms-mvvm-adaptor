using System;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <summary>
    /// For unit testing and mocking of <see cref="SafeActionExtensions"/>
    /// </summary>
    public interface ISafeAction
    {
        /// <summary>
        /// For unit testing and mocking of <see cref="SafeActionExtensions"/>
        /// </summary>
        void SafeInvoke<TException>(Action<object> action, object parameter, Action<TException> onException) where TException : Exception;

        /// <summary>
        /// For unit testing and mocking of <see cref="SafeActionExtensions"/>
        /// </summary>
        void SafeInvoke<TException>(Action action, Action<TException> onException) where TException : Exception;
    }
}