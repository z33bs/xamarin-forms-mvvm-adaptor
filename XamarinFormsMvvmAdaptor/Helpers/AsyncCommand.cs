﻿// Adapted from Brandon Minnick's AsyncAwaitBestPractices
// https://github.com/brminnick/AsyncAwaitBestPractices/tree/3a9522e651a8c5842172cb5c6cc5bf47de9d86af
// Modifications flagged with //GA

using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <summary>
    /// An implementation of IAsyncCommand. Allows Commands to safely be used asynchronously with Task.
    /// </summary>
    public class AsyncCommand<T> : IAsyncCommand<T>
    {
        readonly Func<T, Task> _execute;
        readonly Func<object?, bool> _canExecute;
        readonly Action<Exception>? _onException;
        //GA _continueOnCapturedContext redundant due to SafeFireAndForget modification
        readonly WeakEventManager _weakEventManager = new WeakEventManager();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:TaskExtensions.MVVM.AsyncCommand`1"/> class.
        /// </summary>
        /// <param name="execute">The Function executed when Execute or ExecuteAsync is called. This does not check canExecute before executing and will execute even if canExecute is false</param>
        /// <param name="canExecute">The Function that verifies whether or not AsyncCommand should execute.</param>
        /// <param name="onException">If an exception is thrown in the Task, <c>onException</c> will execute. If onException is null, the exception will be re-thrown</param>
        public AsyncCommand(Func<T, Task> execute,
                            Func<object?, bool>? canExecute = null,
                            Action<Exception>? onException = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute), $"{nameof(execute)} cannot be null");
            _canExecute = canExecute ?? (_ => true);
            _onException = onException;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => _weakEventManager.AddEventHandler(value);
            remove => _weakEventManager.RemoveEventHandler(value);
        }

        /// <summary>
        /// Determines whether the command can execute in its current state
        /// </summary>
        /// <returns><c>true</c>, if this command can be executed; otherwise, <c>false</c>.</returns>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public bool CanExecute(object? parameter) => _canExecute(parameter);

        /// <summary>
        /// Raises the CanExecuteChanged event.
        /// </summary>
        public void RaiseCanExecuteChanged() => _weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(CanExecuteChanged));

        /// <summary>
        /// Executes the Command as a Task
        /// </summary>
        /// <returns>The executed Task</returns>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public Task ExecuteAsync(T parameter) => _execute(parameter);

        void ICommand.Execute(object parameter)
        {
            switch (parameter)
            {
                case T validParameter:
                    ExecuteAsync(validParameter).SafeFireAndForget(_onException);
                    break;

#pragma warning disable CS8604 // Possible null reference argument.
                case null when !typeof(T).GetTypeInfo().IsValueType:
                    ExecuteAsync((T)parameter).SafeFireAndForget(_onException);
                    break;
#pragma warning restore CS8604 // Possible null reference argument.

                case null:
                    throw new InvalidCommandParameterException(typeof(T));

                default:
                    throw new InvalidCommandParameterException(typeof(T), parameter.GetType());
            }
        }
    }

    /// <summary>
    /// An implementation of IAsyncCommand. Allows Commands to safely be used asynchronously with Task.
    /// </summary>
    public class AsyncCommand : IAsyncCommand
    {
        readonly Func<Task> _execute;
        readonly Func<object?, bool> _canExecute;
        readonly Action<Exception>? _onException;
        //GA _continueOnCapturedContext redundant due to SafeFireAndForget modification
        readonly WeakEventManager _weakEventManager = new WeakEventManager();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:TaskExtensions.MVVM.AsyncCommand`1"/> class.
        /// </summary>
        /// <param name="execute">The Function executed when Execute or ExecuteAsync is called. This does not check canExecute before executing and will execute even if canExecute is false</param>
        /// <param name="canExecute">The Function that verifies whether or not AsyncCommand should execute.</param>
        /// <param name="onException">If an exception is thrown in the Task, <c>onException</c> will execute. If onException is null, the exception will be re-thrown</param>
        public AsyncCommand(Func<Task> execute,
                            Func<object?, bool>? canExecute = null,
                            Action<Exception>? onException = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute), $"{nameof(execute)} cannot be null");
            _canExecute = canExecute ?? (_ => true);
            _onException = onException;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => _weakEventManager.AddEventHandler(value);
            remove => _weakEventManager.RemoveEventHandler(value);
        }

        /// <summary>
        /// Determines whether the command can execute in its current state
        /// </summary>
        /// <returns><c>true</c>, if this command can be executed; otherwise, <c>false</c>.</returns>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public bool CanExecute(object? parameter) => _canExecute(parameter);

        /// <summary>
        /// Raises the CanExecuteChanged event.
        /// </summary>
        public void RaiseCanExecuteChanged() => _weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(CanExecuteChanged));

        /// <summary>
        /// Executes the Command as a Task
        /// </summary>
        /// <returns>The executed Task</returns>
        public Task ExecuteAsync() => _execute();

        void ICommand.Execute(object parameter) => _execute().SafeFireAndForget(_onException);
    }
}