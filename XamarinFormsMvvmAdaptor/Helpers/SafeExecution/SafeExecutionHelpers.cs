using System;

// Adapted from Brandon Minnick's AsyncAwaitBestPractices
// https://github.com/brminnick/AsyncAwaitBestPractices, 
// which in turn was inspired by John Thiriet's blog post,
//"Removing Async Void": https://johnthiriet.com/removing-async-void/

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <summary>
    /// Affects the behaviour of <see cref="SafeCommand"/>,
    /// <see cref="SafeTaskExtensions"/> and <see cref="SafeActionExtensions"/>
    /// </summary>
    public static class SafeExecutionHelpers
    {
        internal static bool _shouldAlwaysRethrowException;

        /// <summary>
        /// The default action to execute when an exception is caught by
        /// <see cref="SafeTaskExtensions"/>, <see cref="SafeActionExtensions"/>
        /// , and <see cref="SafeCommand"/>
        /// </summary>
        public static Action<Exception>? DefaultExceptionHandler { get; private set; }

        /// <summary>
        /// Initialize SafeExecutionHelpers without a <see cref="DefaultExceptionHandler"/>.
        ///
        /// Warning! If no <see cref="DefaultExceptionHandler"/> is set and no
        /// <c>onException</c> provided in the Safe Task/Action/Command, the exception
        /// will not be caught and you risk "unsafe" execution.
        /// </summary>
        /// <param name="shouldAlwaysRethrowException">If set to <c>true</c>, after the
        /// exception has been caught and handled, the exception will always be rethrown.
        /// Warning: When <c>true</c>, there is no way to catch this exception
        /// and it will always result in a crash. Recommended only for debugging purposes.
        /// </param>
        public static void Initialize(bool shouldAlwaysRethrowException = false)
            => _shouldAlwaysRethrowException = shouldAlwaysRethrowException;

        /// <summary>
        /// Initialize SafeExecutionHelpers with a <see cref="DefaultExceptionHandler"/>
        /// </summary>
        /// <param name="shouldAlwaysRethrowException">If set to <c>true</c>, after the
        /// exception has been caught and handled, the exception will always be rethrown.
        /// Warning: When <c>true</c>, there is no way to catch this exception
        /// and it will always result in a crash. Recommended only for debugging purposes.
        /// </param>
        /// <param name="defaultOnException"> The default action to execute
        /// when an exception is caught by
        /// <see cref="SafeTaskExtensions"/>, <see cref="SafeActionExtensions"/>
        /// , and <see cref="SafeCommand"/></param>
        public static void Initialize(Action<Exception>? defaultOnException, bool shouldAlwaysRethrowException = false)
            => _shouldAlwaysRethrowException = shouldAlwaysRethrowException;

        /// <summary>
        /// Removes the <see cref="DefaultExceptionHandler"/>
        /// </summary>
        public static void RemoveDefaultExceptionHandler()
            => DefaultExceptionHandler = null;

        /// <summary>
        /// Set the default action for SafeFireAndForget to handle every exception
        /// </summary>
        /// <param name="onException">If an exception is thrown in the Task using SafeFireAndForget, <c>onException</c> will execute</param>
        public static void SetDefaultExceptionHandler(Action<Exception> onException)
        {
            if (onException is null)
                throw new ArgumentNullException(nameof(onException));

            DefaultExceptionHandler = onException;
        }

        /// <summary>
        /// Invoke the given <paramref name="onException"/> as well as the
        /// <see cref="DefaultExceptionHandler"/> if it was Initialized
        /// </summary>
        public static void HandleException<TException>(TException exception, Action<TException>? onException)
            where TException : Exception
        {
            DefaultExceptionHandler?.Invoke(exception);
            onException?.Invoke(exception);
        }

        /// <summary>
        /// Invoke the 
        /// <see cref="DefaultExceptionHandler"/> if it was Initialized
        /// </summary>
        public static void HandleException<TException>(TException exception)
            where TException : Exception
            => HandleException(exception, null);
    }
}
