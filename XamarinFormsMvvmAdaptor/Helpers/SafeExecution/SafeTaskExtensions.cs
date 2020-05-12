using System;
using System.ComponentModel;
using System.Threading.Tasks;

// Adapted from Brandon Minnick's AsyncAwaitBestPractices
// https://github.com/brminnick/AsyncAwaitBestPractices, 
// which in turn was inspired by John Thiriet's blog post,
//"Removing Async Void": https://johnthiriet.com/removing-async-void/

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <summary>
    /// Extension methods for <see cref="Task"/> enabling the safe execution and handling of errors 
    /// </summary> 
    public static class SafeTaskExtensions
    {
        /// <summary>
        /// Handles exceptions for the given Task with <see cref="SafeExecutionHelpers.DefaultExceptionHandler"/>
        /// </summary>
        /// <typeparam name="TException">If an exception is thrown of a different type, it will not be handled</typeparam>
        /// <param name="onException">In addition to the <see cref="SafeExecutionHelpers.DefaultExceptionHandler"/>
        /// , <paramref name="onException"/> will execute if an Exception is thrown.</param>
        public static Task SafeContinueWith<TException>(this Task task, Action<TException>? onException)
            where TException : Exception
            => Implementation.SafeContinueWith(task, onException);

        ///<inheritdoc cref="SafeFireAndForget{TException}(Task, Action{TException}?)"/>
        public static void SafeFireAndForget(this Task task, Action<Exception>? onException)
            => Implementation.SafeContinueWith(task, onException);

        /// <summary>
        /// Handles exceptions for the given Task with <see cref="SafeExecutionHelpers.DefaultExceptionHandler"/>
        /// </summary>
        /// <typeparam name="TException">If an exception is thrown of a different type, it will not be handled</typeparam>
        /// <param name="onException">In addition to the <see cref="SafeExecutionHelpers.DefaultExceptionHandler"/>
        /// , <paramref name="onException"/> will execute if an Exception is thrown.</param>
        public static void SafeFireAndForget<TException>(this Task task, Action<TException>? onException)
            where TException : Exception => Implementation.SafeContinueWith(task, onException);

        #region For Unit Testing

        private static readonly ISafeTask defaultImplementation = new SafeTask();

        /// <summary>
        /// For unit testing / mocking
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ISafeTask Implementation { private get; set; } = defaultImplementation;

        /// <summary>
        /// For unit testing / mocking
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void RevertToDefaultImplementation() => Implementation = defaultImplementation;

        ///<inheritdoc cref="SafeContinueWith{TException}(Task, Action{TException}?, TaskScheduler)"/>
        public static Task SafeContinueWith(this Task task, Action<Exception>? onException)
            => Implementation.SafeContinueWith(task, onException);

        /// <summary>
        /// For unit testing / mocking
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Task SafeContinueWith<TException>(this Task task, Action<TException>? onException, TaskScheduler scheduler = null)
            where TException : Exception
            => Implementation.SafeContinueWith(task, onException, scheduler);

        /// <summary>
        /// For unit testing / mocking
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Task SafeContinueWith(this Task task, Action<Exception>? onException, TaskScheduler scheduler = null)
            => Implementation.SafeContinueWith(task, onException, scheduler);

        /// <summary>
        /// For unit testing / mocking
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void SafeFireAndForget(this Task task, Action<Exception>? onException = null, TaskScheduler scheduler = null) => Implementation.SafeContinueWith(task,onException,scheduler);

        /// <summary>
        /// For unit testing / mocking
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void SafeFireAndForget<TException>(this Task task, Action<TException>? onException = null, TaskScheduler scheduler = null) where TException : Exception => Implementation.SafeContinueWith(task,onException, scheduler);
        #endregion
    }
}