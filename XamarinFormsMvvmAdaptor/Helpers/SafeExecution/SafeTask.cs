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
                            SafeExecutionHelpers.HandleException<TException>(t.Exception.InnerException, onException);
                            //todo move this to HandleException but ensure runs even if not handlers to handle
                            if (SafeExecutionHelpers._shouldAlwaysRethrowException)
                                Device.BeginInvokeOnMainThread(() => throw t.Exception.InnerException);
                        }
                        , CancellationToken.None
                        , TaskContinuationOptions.OnlyOnFaulted
                        , scheduler ?? TaskScheduler.Default);

            return task;
        }
    }
}
