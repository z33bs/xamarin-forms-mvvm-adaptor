using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <inheritdoc/>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class SafeTask : ISafeTask
    {
        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public Task SafeContinueWith<TException>(Task task, Action<TException>? onException, TaskScheduler scheduler = null) where TException : Exception
        {
            if (SafeExecutionHelpers.DefaultExceptionHandler != null || onException != null)
                task.ContinueWith(
                        t =>
                        {
                            SafeExecutionHelpers.HandleException(t.Exception.InnerException as TException, onException);
                            if (SafeExecutionHelpers._shouldAlwaysRethrowException)
                                Device.BeginInvokeOnMainThread(() => throw t.Exception.InnerException);
                        }
                        , CancellationToken.None
                        , TaskContinuationOptions.OnlyOnFaulted
                        , scheduler ?? TaskScheduler.FromCurrentSynchronizationContext()); //todo check doesn't fail .Current might be more robust

            return task;
        }
    }
}
