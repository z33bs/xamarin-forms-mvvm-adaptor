using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    ///<inheritdoc/>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class SafeTask : ISafeTask
    {
        ///<inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public Task SafeContinueWith<TException>(Task task, Action<TException>? onException, TaskScheduler scheduler = null) where TException : Exception
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
