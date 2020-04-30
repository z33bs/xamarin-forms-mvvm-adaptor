﻿// Adapted from Brandon Minnick's AsyncAwaitBestPractices
// https://github.com/brminnick/AsyncAwaitBestPractices/tree/3a9522e651a8c5842172cb5c6cc5bf47de9d86af
// Modifications flagged with //GA

//Refactor with base class to be more maintainable
//Don't need separate for viewmodel
//Most common use
//Explore convenience overloads (intellisense)

using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <summary>
    /// An implementation of IAsyncCommand. Allows Commands to safely be used asynchronously with Task.
    /// </summary>
    public class SafeCommand<T> : ISafeCommand<T>
    {
        readonly Func<T, Task> _executeAsync;
        readonly Func<object?, bool> _canExecute; //was object?
        readonly Action<Exception>? _onException;
        readonly WeakEventManager _weakEventManager = new WeakEventManager();
        readonly TaskScheduler _scheduler; //GA add
        bool _mustRunOnCurrentSyncContext;
        readonly Action<T> _execute;
        IViewModelBase _viewModel;

        static bool IsValidParameter(object o)
        {
            if (o != null)
            {
                // The parameter isn't null, so we don't have to worry whether null is a valid option
                return o is T;
            }
            var t = typeof(T);
            // The parameter is null. Is T Nullable?
            if (Nullable.GetUnderlyingType(t) != null)
            {
                return true;
            }
            // Not a Nullable, if it's a value type then null is not valid
            return !t.GetTypeInfo().IsValueType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:TaskExtensions.MVVM.AsyncCommand`1"/> class.
        /// </summary>
        /// <param name="executeFunction">The Function executed when Execute or ExecuteAsync is called. This does not check canExecute before executing and will execute even if canExecute is false</param>
        /// <param name="canExecute">The Function that verifies whether or not AsyncCommand should execute.</param>
        /// <param name="onException">If an exception is thrown in the Task, <c>onException</c> will execute. If onException is null, the exception will be re-thrown</param>
        public SafeCommand(
            Func<T, Task> executeFunction,
            Func<T, bool>? canExecute = null,
            Action<Exception>? onException = null,
            bool mustRunOnCurrentSyncContext = false)
            : this(
                  o =>
                    {
                        if (canExecute is null)
                            return true;

                        return IsValidParameter(o) && canExecute((T)o);
                    },
                  onException,
                  mustRunOnCurrentSyncContext)
{
            _executeAsync = executeFunction ?? throw new ArgumentNullException(nameof(executeFunction), $"{nameof(executeFunction)} cannot be null");
        }

        public SafeCommand(
            Func<T, Task> executeFunction,
            IViewModelBase viewModel,
            Func<T, bool>? canExecute = null,
            Action<Exception>? onException = null,
            bool mustRunOnCurrentSyncContext = false) : this(executeFunction,canExecute,onException,mustRunOnCurrentSyncContext)
        {
            _viewModel = viewModel;
        }


        [EditorBrowsable(EditorBrowsableState.Never)] //Designed for Testing purposes only
        public SafeCommand(
            Func<T, Task> executeFunction,
            TaskScheduler scheduler,
            Func<T, bool>? canExecute,
            Action<Exception>? onException) : this(executeFunction, canExecute, onException)
        {
            _scheduler = scheduler;
        }

        public SafeCommand(
            Action<T> executeAction,
            Func<T, bool>? canExecute = null,
            Action<Exception>? onException = null,
            bool mustRunOnCurrentSyncContext = false)
            : this(
                  o =>
                    {
                        if (canExecute is null)
                            return true;

                        return IsValidParameter(o) && canExecute((T)o);
                    },
                  onException,
                  mustRunOnCurrentSyncContext)
        {
            _execute = executeAction ?? throw new ArgumentNullException(nameof(executeAction), $"{nameof(executeAction)} cannot be null");
        }

        public SafeCommand(
            Action<T> executeAction,
            IViewModelBase viewModel,
            Func<T, bool>? canExecute = null,
            Action<Exception>? onException = null,
            bool mustRunOnCurrentSyncContext = false) : this(executeAction,canExecute,onException,mustRunOnCurrentSyncContext)
        {
            _viewModel = viewModel;
        }


        SafeCommand(
            Func<object?, bool>? canExecute = null,
            Action<Exception>? onException = null,
            bool mustRunOnCurrentSyncContext = false)
        {
            _canExecute = canExecute ?? (_ => true);
            _onException = onException;
            _mustRunOnCurrentSyncContext = mustRunOnCurrentSyncContext;
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

        bool _isBusy;
        bool IsBusy
        {
            get
            {
                if (_viewModel?.IsBusy ?? false)
                    return true;

                return _isBusy;
            }
            set
            {
                _isBusy = value;
                if (_viewModel != null) _viewModel.IsBusy = value;
            }
        }
        /// <summary>
        /// Executes the Command as a Task
        /// </summary>
        /// <returns>The executed Task</returns>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
		Task InternalExecuteAsync(T parameter)
        {
            if (IsBusy)
                return Task.CompletedTask;

            IsBusy = true;

            if (_scheduler != null)
                return
                    Task.Factory
                    .StartNew(() => _executeAsync(parameter).GetAwaiter().GetResult(),
                                    CancellationToken.None,
                                    TaskCreationOptions.DenyChildAttach,
                                    _scheduler)
                    .SafeTask(_onException, _scheduler) //Handles exception if faulted
                    .ContinueWith(t => IsBusy = false,
                                    CancellationToken.None,
                                    TaskContinuationOptions.None,
                                    _scheduler);

            if (_mustRunOnCurrentSyncContext)
                return
                    _executeAsync(parameter)
                    .SafeTask(_onException)
                    .ContinueWith(t => IsBusy = false);

            return
                Task.Run(() => _executeAsync(parameter).GetAwaiter().GetResult())
                               .SafeTask(_onException)
                               .ContinueWith(t => IsBusy = false);
        }

        void InternalExecute(T parameter)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                if (_mustRunOnCurrentSyncContext)
                    _execute.SafeInvoke(parameter, _onException);

                else
                    Task.Run(() => _execute.SafeInvoke(parameter, _onException)).GetAwaiter().GetResult();
            }
            finally { IsBusy = false; }
        }

        public void Execute(object parameter)
        {
            switch (parameter)
            {
                case T validParameter:
                    if (_execute != null)
                        InternalExecute(validParameter);
                    else
                        InternalExecuteAsync(validParameter);
                    break;

#pragma warning disable CS8604 // Possible null reference argument.
                case null when !typeof(T).GetTypeInfo().IsValueType || Nullable.GetUnderlyingType(typeof(T)) != null:
                    if (_execute != null)
                        InternalExecute((T)parameter);
                    else
                        InternalExecuteAsync((T)parameter);
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
    public class SafeCommand : ISafeCommand
    {
        readonly Func<Task> _executeAsync;
        readonly Func<object?, bool> _canExecute;
        readonly Action<Exception>? _onException;
        readonly WeakEventManager _weakEventManager = new WeakEventManager();
        readonly TaskScheduler _scheduler; //GA add
        bool _mustRunOnCurrentSyncContext;
        readonly Action<object> _execute;
        IViewModelBase _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:TaskExtensions.MVVM.AsyncCommand`1"/> class.
        /// </summary>
        /// <param name="executeFunction">The Function executed when Execute or ExecuteAsync is called. This does not check canExecute before executing and will execute even if canExecute is false</param>
        /// <param name="canExecute">The Function that verifies whether or not AsyncCommand should execute.</param>
        /// <param name="onException">If an exception is thrown in the Task, <c>onException</c> will execute. If onException is null, the exception will be re-thrown</param>
        public SafeCommand(
            Func<Task> executeFunction,
            Func<object?, bool>? canExecute = null,
            Action<Exception>? onException = null,
            bool mustRunOnCurrentSyncContext = false)
            : this(canExecute, onException, mustRunOnCurrentSyncContext)
        {
            _executeAsync = executeFunction ?? throw new ArgumentNullException(nameof(executeFunction), $"{nameof(executeFunction)} cannot be null");
        }

        public SafeCommand(
            Func<Task> executeFunction,
            IViewModelBase viewModel,
            Func<object?, bool>? canExecute = null,
            Action<Exception>? onException = null,
            bool mustRunOnCurrentSyncContext = false)
            : this(executeFunction ,canExecute, onException, mustRunOnCurrentSyncContext)
        {
            _viewModel = viewModel;
        }


        [EditorBrowsable(EditorBrowsableState.Never)] //Designed for Testing purposes only
        public SafeCommand(
            Func<Task> execute,
            TaskScheduler scheduler,
            Func<object?, bool>? canExecute,
            Action<Exception>? onException) : this(execute, canExecute, onException)
        {
            _scheduler = scheduler;
        }

        public SafeCommand(
            Func<Task> execute,
            IViewModelBase viewModel,
            TaskScheduler scheduler,
            Func<object?, bool>? canExecute,
            Action<Exception>? onException)
            : this(execute, scheduler, canExecute, onException)
        {
            _viewModel = viewModel;
        }

        #region Action overloads

        //Warning! If using lambda expression, the compiler will choose the Func<Task> overload over Action delegate. It's logic is that a delegate with a return type is a better match than a delegate without a return type. To be explicit, either abstract the lamda into a separate method or specify the parameter name before the lambda executeAction: () => {}
        public SafeCommand(
            Action executeAction,
            Func<bool>? canExecute = null,
            Action<Exception>? onException = null,
            bool mustRunOnCurrentSyncContext = false)
            : this(
                  o => executeAction(),
                  o =>
                       {
                           if (canExecute is null)
                               return true;
                           else
                               return canExecute();
                       },
                  onException,
                  mustRunOnCurrentSyncContext)
        {
            if (executeAction == null)
                throw new ArgumentNullException(nameof(executeAction));
        }

        public SafeCommand(
            Action executeAction,
            IViewModelBase viewModel,
            Func<bool>? canExecute = null,
            Action<Exception>? onException = null,
            bool mustRunOnCurrentSyncContext = false)
            : this(executeAction: executeAction,canExecute,onException,mustRunOnCurrentSyncContext)
        {
            _viewModel = viewModel;
        }


        public SafeCommand(
            Action<object> executeAction,
            Func<object?, bool>? canExecute = null,
            Action<Exception>? onException = null,
            bool mustRunOnCurrentSyncContext = false)
            : this(canExecute, onException, mustRunOnCurrentSyncContext)
        {
            _execute = executeAction ?? throw new ArgumentNullException(nameof(executeAction), $"{nameof(executeAction)} cannot be null");
        }

        public SafeCommand(
            Action<object> executeAction,
            IViewModelBase viewModel,
            Func<object?, bool>? canExecute = null,
            Action<Exception>? onException = null,
            bool mustRunOnCurrentSyncContext = false)
            : this(executeAction: executeAction, canExecute, onException, mustRunOnCurrentSyncContext)
        {
            _viewModel = viewModel;
        }

        #endregion

        SafeCommand(
            Func<object?, bool>? canExecute = null,
            Action<Exception>? onException = null,
            bool mustRunOnCurrentSyncContext = false)
        {
            _canExecute = canExecute ?? (_ => true);
            _onException = onException;
            _mustRunOnCurrentSyncContext = mustRunOnCurrentSyncContext;
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

        bool _isBusy;
        bool IsBusy
        {
            get
            {
                if (_viewModel?.IsBusy ?? false)
                    return true;

                return _isBusy;
            }
            set
            {
                _isBusy = value;
                if (_viewModel != null) _viewModel.IsBusy = value;
            }
        }
        /// <summary>
        /// Executes the Command as a Task
        /// </summary>
        /// <returns>The executed Task</returns>
        Task InternalExecuteAsync()
        {
            if (IsBusy)
                return Task.CompletedTask;
            
            IsBusy = true;

            if (_scheduler != null)
                return
                    Task.Factory
                    .StartNew(() => _executeAsync().GetAwaiter().GetResult(),
                                    CancellationToken.None,
                                    TaskCreationOptions.DenyChildAttach,
                                    _scheduler)
                    .SafeTask(_onException, _scheduler) //Handles exception if faulted
                    .ContinueWith(t => { IsBusy = false; },
                                    CancellationToken.None,
                                    TaskContinuationOptions.None,
                                    _scheduler);

            if (_mustRunOnCurrentSyncContext)
                return
                    _executeAsync()
                    .SafeTask(_onException)
                    .ContinueWith(t => { IsBusy = false; });

            return
                Task.Run(() => _executeAsync().GetAwaiter().GetResult())
                               .SafeTask(_onException)
                               .ContinueWith(t => { IsBusy = false; });
        }

        void InternalExecute()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                if (_mustRunOnCurrentSyncContext)
                    _execute.SafeInvoke(null, _onException);

                else
                    Task.Run(() =>
                            _execute.SafeInvoke(null, _onException))
                        .GetAwaiter()
                        .GetResult();
            }
            finally { IsBusy = false; }
        }

        public void Execute(object parameter)
        {
            if (_execute != null)
                InternalExecute();
            else
                InternalExecuteAsync();
        }
    }
}