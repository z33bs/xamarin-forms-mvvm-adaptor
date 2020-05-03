using System;
using System.ComponentModel;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <inheritdoc/>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class SafeAction : ISafeAction
    {
        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void SafeInvoke<TException>(
            Action<object?> action,
            object parameter,
            Action<TException>? onException)
            where TException : Exception
        {
            try
            {
                action(parameter);
            }
            catch (TException ex)
            when (SafeExecutionHelpers.DefaultExceptionHandler != null
                    || onException != null)
            {
                SafeExecutionHelpers.HandleException(ex, onException);
            }
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void SafeInvoke<TException>(Action action, Action<TException>? onException)
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
