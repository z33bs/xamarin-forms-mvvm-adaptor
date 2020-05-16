using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <summary>
    /// For unit testing and mocking of <see cref="SafeTaskExtensions"/>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SafeTask : ISafeTask
    {
        /// <summary>
        /// For unit testing and mocking of <see cref="SafeTaskExtensions"/>
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Task SafeContinueWith<TException>(Task task, Action<TException> onException, TaskScheduler scheduler = null) where TException : Exception
        {
            task.ContinueWith(
                    t => SafeExecutionHelpers
                        .HandleException<TException>(t.Exception.InnerException, onException)
                    , CancellationToken.None
                    , TaskContinuationOptions.OnlyOnFaulted
                    , scheduler ?? TaskScheduler.Default);

            return task;
        }
    }
}
