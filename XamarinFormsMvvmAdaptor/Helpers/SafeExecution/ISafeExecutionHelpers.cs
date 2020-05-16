using System;
namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <summary>
    /// Core functionality for the collection of &quot;Safe&quot; delgate invokations 
    /// </summary>
    public interface ISafeExecutionHelpers
    {
        /// <summary>
        /// The default action to execute when an exception is caught by
        /// <see cref="SafeTaskExtensions"/>, <see cref="SafeActionExtensions"/>
        /// , and <see cref="SafeCommand"/>
        /// </summary>
        Action<Exception> DefaultExceptionHandler { get; }
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
        void Initialize(bool shouldAlwaysRethrowException = false);
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
        void Initialize(
            Action<Exception> defaultOnException,
            bool shouldAlwaysRethrowException = false);
        /// <summary>
        /// Removes the <see cref="DefaultExceptionHandler"/>
        /// </summary>
        void RemoveDefaultExceptionHandler();
        /// <summary>
        /// Set the default action for SafeFireAndForget to handle every exception
        /// </summary>
        /// <param name="onException">If an exception is thrown in the Task using SafeFireAndForget, <c>onException</c> will execute</param>
        void SetDefaultExceptionHandler(Action<Exception> onException);
        /// <summary>
        /// Invoke the given <paramref name="onException"/> callback if
        /// exception is of type <typeparamref name="TException"/>.
        /// If <paramref name="onException"/> is not executed, the
        /// <see cref="DefaultExceptionHandler"/> will be called
        /// if it was Initialized
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="exception"></param>
        /// <param name="onException"></param>
        void HandleException<TException>(
            Exception exception,
            Action<TException> onException)
            where TException : Exception;
        /// <summary>
        /// Invoke the 
        /// <see cref="DefaultExceptionHandler"/> if it was Initialized
        /// </summary>
        void HandleException(Exception exception);
    }
}
