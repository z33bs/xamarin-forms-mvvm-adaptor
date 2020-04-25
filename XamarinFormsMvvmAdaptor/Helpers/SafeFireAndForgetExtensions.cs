// Adapted from Brandon Minnick's AsyncAwaitBestPractices
// https://github.com/brminnick/AsyncAwaitBestPractices/tree/3a9522e651a8c5842172cb5c6cc5bf47de9d86af
// Modifications flagged with //GA

using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <summary>
    /// Extension methods for System.Threading.Tasks.Task and System.Threading.Tasks.ValueTask
    /// </summary> 
    public static class SafeFireAndForgetExtensions
    {
        static Action<Exception>? _onException;
        static bool _shouldAlwaysRethrowException;

        /// <summary>
        /// Safely execute the Task without waiting for it to complete before moving to the next line of code; commonly known as "Fire And Forget". Inspired by John Thiriet's blog post, "Removing Async Void": https://johnthiriet.com/removing-async-void/.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="onException">If an exception is thrown in the Task, <c>onException</c> will execute. If onException is null, the exception will be re-thrown</param>
        public static void SafeFireAndForget(this Task task, in Action<Exception>? onException = null) => HandleSafeFireAndForget(task, onException);

        /// <summary>
        /// Safely execute the Task without waiting for it to complete before moving to the next line of code; commonly known as "Fire And Forget". Inspired by John Thiriet's blog post, "Removing Async Void": https://johnthiriet.com/removing-async-void/.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="onException">If an exception is thrown in the Task, <c>onException</c> will execute. If onException is null, the exception will be re-thrown</param>
        /// <typeparam name="TException">Exception type. If an exception is thrown of a different type, it will not be handled</typeparam>
        public static void SafeFireAndForget<TException>(this Task task, in Action<TException>? onException = null) where TException : Exception => HandleSafeFireAndForget(task, onException);

        /// <summary>
        /// Initialize SafeFireAndForget
        ///
        /// Warning: When <c>true</c>, there is no way to catch this exception and it will always result in a crash. Recommended only for debugging purposes.
        /// </summary>
        /// <param name="shouldAlwaysRethrowException">If set to <c>true</c>, after the exception has been caught and handled, the exception will always be rethrown.</param>
        public static void Initialize(in bool shouldAlwaysRethrowException = false) => _shouldAlwaysRethrowException = shouldAlwaysRethrowException;

        /// <summary>
        /// Remove the default action for SafeFireAndForget
        /// </summary>
        public static void RemoveDefaultExceptionHandling() => _onException = null;

        /// <summary>
        /// Set the default action for SafeFireAndForget to handle every exception
        /// </summary>
        /// <param name="onException">If an exception is thrown in the Task using SafeFireAndForget, <c>onException</c> will execute</param>
        public static void SetDefaultExceptionHandling(in Action<Exception> onException)
        {
            if (onException is null)
                throw new ArgumentNullException(nameof(onException));

            _onException = onException;
        }

        static void HandleSafeFireAndForget<TException>(Task task, Action<TException>? onException) where TException : Exception
        {
            //GA Modified to use ContinueWith instead of async/await
            task.SafeTask(onException);
        }
        
        //GA Add
        public static Task SafeTask<TException>(this Task task, Action<TException>? onException) where TException : Exception
        {
            if (_onException != null || onException != null)
                task.ContinueWith(
                        t =>
                        {
                            HandleException(t.Exception.InnerException as TException, onException);
                            if (_shouldAlwaysRethrowException)
                                Device.BeginInvokeOnMainThread(() => throw t.Exception.InnerException);
                        }
                        , TaskContinuationOptions.OnlyOnFaulted);

            return task;
        }

        //GA Add
        public static void SafeInvoke<TException>(this Action<object> action, object parameter, in Action<TException>? onException) where TException : Exception
        {
            try
            {
                action(parameter);
            }
            catch (TException ex) when (_onException != null || onException != null)
            {
                HandleException(ex, onException);
            }
        }

        static void HandleException<TException>(in TException exception, in Action<TException>? onException) where TException : Exception
        {
            _onException?.Invoke(exception);
            onException?.Invoke(exception);
        }

        #region Overloads
        /// <summary>
        /// Safely execute the Task without waiting for it to complete before moving to the next line of code; commonly known as "Fire And Forget". Inspired by John Thiriet's blog post, "Removing Async Void": https://johnthiriet.com/removing-async-void/.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="onException">If an exception is thrown in the Task, <c>onException</c> will execute. If onException is null, the exception will be re-thrown</param>
        public static void SafeFireAndForget(this Task task, Action<Exception>? onException) => task.SafeFireAndForget(in onException);

        /// <summary>
        /// Safely execute the Task without waiting for it to complete before moving to the next line of code; commonly known as "Fire And Forget". Inspired by John Thiriet's blog post, "Removing Async Void": https://johnthiriet.com/removing-async-void/.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="onException">If an exception is thrown in the Task, <c>onException</c> will execute. If onException is null, the exception will be re-thrown</param>
        /// <typeparam name="TException">Exception type. If an exception is thrown of a different type, it will not be handled</typeparam>
        public static void SafeFireAndForget<TException>(this Task task, Action<TException>? onException) where TException : Exception => task.SafeFireAndForget(in onException);

        /// <summary>
        /// Initialize SafeFireAndForget
        ///
        /// Warning: When <c>true</c>, there is no way to catch this exception and it will always result in a crash. Recommended only for debugging purposes.
        /// </summary>
        /// <param name="shouldAlwaysRethrowException">If set to <c>true</c>, after the exception has been caught and handled, the exception will always be rethrown.</param>
        public static void Initialize(bool shouldAlwaysRethrowException) => Initialize(in shouldAlwaysRethrowException);

        /// <summary>
        /// Set the default action for SafeFireAndForget to handle every exception
        /// </summary>
        /// <param name="onException">If an exception is thrown in the Task using SafeFireAndForget, <c>onException</c> will execute</param>
        public static void SetDefaultExceptionHandling(Action<Exception> onException) => SetDefaultExceptionHandling(in onException);
        #endregion
    }
}