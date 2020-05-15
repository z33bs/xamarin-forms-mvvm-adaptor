using System;
using System.ComponentModel;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    public static class SafeActionExtensions
    {
        private static readonly ISafeAction defaultImplementation = new SafeAction();

        /// <summary>
        /// For unit testing / mocking
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ISafeAction Implementation { private get; set; } = defaultImplementation;

        /// <summary>
        /// For unit testing / mocking
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RevertToDefaultImplementation()
        {
            Implementation = defaultImplementation;
        }

        /// <summary>
        /// Execute the Action, handling exceptions with <see cref="SafeExecutionHelpers.DefaultExceptionHandler"/>
        /// </summary>
        /// <typeparam name="TException">If an exception is thrown of a different type, it will not be handled</typeparam>
        /// <param name="onException">In addition to the <see cref="SafeExecutionHelpers.DefaultExceptionHandler"/>
        /// , <paramref name="onException"/> will execute if an Exception is thrown.</param>
        public static void SafeInvoke<TException>(
            this Action<object?> action,
            object parameter,
            Action<TException>? onException)
            where TException : Exception
        => Implementation.SafeInvoke(action, parameter, onException);

        /// <summary>
        /// Execute the Action, handling exceptions with <see cref="SafeExecutionHelpers.DefaultExceptionHandler"/>
        /// </summary>
        /// <typeparam name="TException">If an exception is thrown of a different type, it will not be handled</typeparam>
        /// <param name="onException">In addition to the <see cref="SafeExecutionHelpers.DefaultExceptionHandler"/>
        /// , <paramref name="onException"/> will execute if an Exception is thrown.</param>
        public static void SafeInvoke<TException>(
            this Action action,
            Action<TException>? onException)
            where TException : Exception
        => Implementation.SafeInvoke(action, onException);

        /// <summary>
        /// Execute the Action, handling exceptions with <see cref="SafeExecutionHelpers.DefaultExceptionHandler"/>
        /// </summary>
        /// <param name="onException">In addition to the <see cref="SafeExecutionHelpers.DefaultExceptionHandler"/>
        /// , <paramref name="onException"/> will execute if an Exception is thrown.</param>
        public static void SafeInvoke(
            this Action<object?> action,
            object parameter,
            Action<Exception>? onException)
        => Implementation.SafeInvoke(action, parameter, onException);

        /// <summary>
        /// Execute the Action, handling exceptions with <see cref="SafeExecutionHelpers.DefaultExceptionHandler"/>
        /// </summary>
        /// <param name="onException">In addition to the <see cref="SafeExecutionHelpers.DefaultExceptionHandler"/>
        /// , <paramref name="onException"/> will execute if an Exception is thrown.</param>
        public static void SafeInvoke(
            this Action action,
            Action<Exception>? onException)
        => Implementation.SafeInvoke(action, onException);
    }
}
