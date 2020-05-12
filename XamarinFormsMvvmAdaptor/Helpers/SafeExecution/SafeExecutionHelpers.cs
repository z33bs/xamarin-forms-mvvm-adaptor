using System;
using System.ComponentModel;
using Xamarin.Forms;

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
    public class SafeExecutionHelpers : ISafeExecutionHelpers
    {
        #region For Unit Testing

        private static readonly ISafeExecutionHelpers defaultImplementation = new SafeExecutionHelpers();

        /// <summary>
        /// For unit testing / mocking
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetImplementation(ISafeExecutionHelpers implementation)
            => Instance = implementation;


        /// <summary>
        /// For unit testing / mocking
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RevertToDefaultImplementation()
            => Instance = defaultImplementation;

        #endregion
        /// <summary>
        /// Singleton Instance
        /// </summary>
        public static ISafeExecutionHelpers Instance { get; private set; } = defaultImplementation;

        static bool _shouldAlwaysRethrowException;

        /// <summary>
        /// The default action to execute when an exception is caught by
        /// <see cref="SafeTaskExtensions"/>, <see cref="SafeActionExtensions"/>
        /// , and <see cref="SafeCommand"/>
        /// </summary>
        public static Action<Exception>? DefaultExceptionHandler //{ get; private set; }
            => Instance.DefaultExceptionHandler;

        Action<Exception>? _defaultExceptionHandler;
        /// <inheritdoc/>
        Action<Exception>? ISafeExecutionHelpers.DefaultExceptionHandler
            => _defaultExceptionHandler;

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
            => Instance.Initialize(shouldAlwaysRethrowException);
        /// <inheritdoc/>
        void ISafeExecutionHelpers.Initialize(bool shouldAlwaysRethrowException)
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
            => Instance.Initialize(defaultOnException, shouldAlwaysRethrowException);
        /// <inheritdoc/>
        void ISafeExecutionHelpers.Initialize(Action<Exception>? defaultOnException, bool shouldAlwaysRethrowException)
        {
            _shouldAlwaysRethrowException = shouldAlwaysRethrowException;
            _defaultExceptionHandler = defaultOnException;
        }


        /// <summary>
        /// Removes the <see cref="DefaultExceptionHandler"/>
        /// </summary>
        public static void RemoveDefaultExceptionHandler()
            => Instance.RemoveDefaultExceptionHandler();
        /// <inheritdoc/>
        void ISafeExecutionHelpers.RemoveDefaultExceptionHandler()
            => _defaultExceptionHandler = null;

        /// <summary>
        /// Set the default action for SafeFireAndForget to handle every exception
        /// </summary>
        /// <param name="onException">If an exception is thrown in the Task using SafeFireAndForget, <c>onException</c> will execute</param>
        public static void SetDefaultExceptionHandler(Action<Exception> onException)
            => Instance.SetDefaultExceptionHandler(onException);
        /// <inheritdoc/>
        void ISafeExecutionHelpers.SetDefaultExceptionHandler(Action<Exception> onException)
        {
            if (onException is null)
                throw new ArgumentNullException(nameof(onException));

            _defaultExceptionHandler = onException;
        }

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
        public static void HandleException<TException>(Exception exception, Action<TException>? onException)
            where TException : Exception
            => Instance.HandleException(exception, onException);
        /// <inheritdoc/>
        void ISafeExecutionHelpers.HandleException<TException>(Exception exception, Action<TException>? onException)
        {
            if (exception is InvalidCommandParameterException)
                throw exception; //internal exception from SafeCommand

            if (onException != null && exception is TException)
                onException.Invoke(exception as TException);
            else
                DefaultExceptionHandler?.Invoke(exception);

            if (_shouldAlwaysRethrowException)
                Device.BeginInvokeOnMainThread(() => throw exception);
        }

        /// <summary>
        /// Invoke the 
        /// <see cref="DefaultExceptionHandler"/> if it was Initialized
        /// </summary>
        public static void HandleException(Exception exception)
            => Instance.HandleException(exception);
        /// <inheritdoc/>
        void ISafeExecutionHelpers.HandleException(Exception exception)
            => Instance.HandleException<Exception>(exception, null);
    }
}
