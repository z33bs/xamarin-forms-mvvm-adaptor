using System;
using System.ComponentModel;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <summary>
    /// For unit testing and mocking of <see cref="SafeActionExtensions"/>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SafeAction : ISafeAction
    {
        /// <summary>
        /// For unit testing and mocking of <see cref="SafeActionExtensions"/>
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SafeInvoke<TException>(
            Action<object> action,
            object parameter,
            Action<TException> onException)
            where TException : Exception
        {
            try
            {
                action(parameter);
            }
            catch (TException ex)
            {
                SafeExecutionHelpers.HandleException(ex, onException);
            }
        }

        /// <summary>
        /// For unit testing and mocking of <see cref="SafeActionExtensions"/>
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SafeInvoke<TException>(Action action, Action<TException> onException)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException ex) when (SafeExecutionHelpers.DefaultExceptionHandler != null || onException != null)
            {
                SafeExecutionHelpers.HandleException(ex, onException);
            }
        }
    }
}
