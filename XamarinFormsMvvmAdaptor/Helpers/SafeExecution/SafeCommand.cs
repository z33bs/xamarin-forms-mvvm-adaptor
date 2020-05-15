using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

// Built on the foundation of Brandon Minnick's
// AsyncAwaitBestPractices
// https://github.com/brminnick/AsyncAwaitBestPractices, 
// which in turn was inspired by John Thiriet's blog post,
// https://johnthiriet.com/mvvm-going-async-with-async-command

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <summary>
    /// An implementation of <see cref="ICommand"/>.
    /// Allows Commands to safely be used asynchronously with Task,
    /// catching exceptions, and offering exception handling.
    /// Runs the Command on a background thread while giving
    /// the option to run from current synchronisation context.
    /// Sets <see cref="IViewModelBase.IsBusy"/> to <c>true</c>
    /// while Command is executing. 
    /// </summary>
    public class SafeCommand<T> : SafeCommand
    {
        /// <summary>
        /// Initializes an instance of the <see cref="ICommand" class/>
        /// </summary>
        /// <param name="executeFunction">The function executed when
        /// <see cref="ICommand.Execute(object)"/> is called.</param>
        /// <param name="viewModel"> Option to pass the calling ViewModel
        /// via <c>this</c>. Will set <see cref="IViewModelBase.IsBusy"/>
        /// to <c>true</c> while <paramref name="executeFunction"/> is
        /// executing</param>
        /// <param name="canExecute">The function that verifies whether or
        /// not <see cref="SafeCommand"/>
        /// should execute</param>
        /// <param name="onException">If an exception is thrown,
        /// <paramref name="onException"/> will execute.</param>
        /// <param name="mustRunOnCurrentSyncContext"><c>false</c> by default
        /// , causing <paramref name="executeFunction"/> to run on a background
        /// thread. If set to <c>true</c>, will run on the current thread.
        /// In Xamarin.Forms this will be the Main (UI) thread.</param>
        public SafeCommand(
            Func<T, Task> executeFunction,
            IViewModelBase viewModel = null,
            Action<Exception>? onException = null,
            Func<T, bool>? canExecute = null,
            bool mustRunOnCurrentSyncContext = false,
            bool isBlocking = true)
            : base(
                  internalExecuteFunction: o =>
                  {
                      if (!IsValidParameter(o))
                          throw new InvalidCommandParameterException(typeof(T));

                      return executeFunction((T)o);
                  },
                  viewModel,
                  onException,
                  o =>
                  {
                      if (canExecute is null)
                          return true;

                      return IsValidParameter(o) && canExecute((T)o);
                  },
                  mustRunOnCurrentSyncContext,
                  isBlocking)
        {
            if (executeFunction is null)
                throw new ArgumentNullException(nameof(executeFunction)
                    , $"{nameof(executeFunction)} cannot be null");
        }

        /// <summary>
        /// Initializes an instance of the <see cref="ICommand" class/>
        /// </summary>
        /// <param name="executeFunction">The function executed when
        /// <see cref="ICommand.Execute(object)"/> is called.</param>
        public SafeCommand(Func<T, Task> executeFunction)
            : this(executeFunction, null, null, null, false, true)
        { }

        /// <summary>
        /// Initializes an instance of the <see cref="ICommand" class/>
        /// </summary>
        /// <remarks>Warning! If <paramref name="executeAction"/> is set to a lambda expression <c>() => { }</c>,
        /// the compiler will choose the wrong <c>Func<T,Task></c> overload instead.
        /// To be explicit, either abstract the lambda into a separate method
        /// (where it will be obvious to the compiler, or specify the parameter name before the lambda
        /// as so: <c>executeAction: () => { }</c>.
        /// The compiler's logic is that a delegate with a return type is a better match
        /// than a delegate without a return type.
        /// </remarks>
        /// <param name="executeAction">The function executed when
        /// <see cref="ICommand.Execute(object)"/> is called.</param>
        /// <param name="viewModel"> Option to pass the calling ViewModel
        /// via <c>this</c>. Will set <see cref="IViewModelBase.IsBusy"/>
        /// to <c>true</c> while <paramref name="executeAction"/> is
        /// executing</param>
        /// <param name="canExecute">The function that verifies whether or
        /// not <see cref="SafeCommand"/>
        /// should execute</param>
        /// <param name="onException">If an exception is thrown,
        /// <paramref name="onException"/> will execute.</param>
        /// <param name="mustRunOnCurrentSyncContext"><c>false</c> by default
        /// , causing <paramref name="executeAction"/> to run on a background
        /// thread. If set to <c>true</c>, will run on the current thread.
        /// In Xamarin.Forms this will be the Main (UI) thread.</param>
        public SafeCommand(
            Action<T> executeAction,
            IViewModelBase viewModel = null,
            Action<Exception>? onException = null,
            Func<T, bool>? canExecute = null,
            bool mustRunOnCurrentSyncContext = false,
            bool isBlocking = true)
            : base(
                  internalExecuteAction: o =>
                  {
                      if (!IsValidParameter(o))
                          throw new InvalidCommandParameterException(typeof(T));

                      executeAction((T)o);
                  },
                  viewModel,
                  onException,
                  o =>
                  {
                      if (canExecute is null)
                          return true;

                      return IsValidParameter(o) && canExecute((T)o);
                  },
                  mustRunOnCurrentSyncContext,
                  isBlocking)
        {
            if (executeAction is null)
                throw new ArgumentNullException(nameof(executeAction)
                    , $"{nameof(executeAction)} cannot be null");
        }

        /// <summary>
        /// Initializes an instance of the <see cref="ICommand" class/>
        /// </summary>
        /// <remarks>Warning! If <paramref name="executeAction"/> is set to a lambda expression <c>() => { }</c>,
        /// the compiler will choose the wrong <c>Func<T,Task></c> overload instead.
        /// To be explicit, either abstract the lambda into a separate method
        /// (where it will be obvious to the compiler, or specify the parameter name before the lambda
        /// as so: <c>executeAction: () => { }</c>.
        /// The compiler's logic is that a delegate with a return type is a better match
        /// than a delegate without a return type.
        /// </remarks>
        /// <param name="executeAction">The function executed when
        /// <see cref="ICommand.Execute(object)"/> is called.</param>
        public SafeCommand(Action<T> executeAction)
            : this(executeAction, null, null, null, false, true)
        { }

        /// <summary>
        /// For Unit Testing. Command runs using the specified <see cref="TaskScheduler"/>
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public SafeCommand(
            Func<T, Task> executeFunction,
            TaskScheduler scheduler,
            IViewModelBase viewModel = null,
            Action<Exception>? onException = null,
            Func<T, bool>? canExecute = null,
            //bool mustRunOnCurrentSyncContext is moot
            bool isBlocking = true
            )
            : base(
                  o =>
                  {
                      if (!IsValidParameter(o))
                          throw new InvalidCommandParameterException(typeof(T));

                      return executeFunction((T)o);
                  },
                  scheduler,
                  viewModel,
                  onException,
                  o =>
                  {
                      if (canExecute is null)
                          return true;

                      return IsValidParameter(o) && canExecute((T)o);
                  },
                  isBlocking)
        {
            if (executeFunction is null)
                throw new ArgumentNullException(nameof(executeFunction)
                    , $"{nameof(executeFunction)} cannot be null");
        }

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
    }

    /// <summary>
    /// An implementation of <see cref="ICommand"/>.
    /// Allows Commands to safely be used asynchronously with Task,
    /// catching exceptions, and offering exception handling.
    /// Runs the Command on a background thread while giving
    /// the option to run from current synchronisation context.
    /// Sets <see cref="IViewModelBase.IsBusy"/> to <c>true</c>
    /// while Command is executing. 
    /// </summary>
    public class SafeCommand : ISafeCommand
    {
        readonly Func<object, Task> _executeAsync;
        readonly Func<object?, bool> _canExecute;
        readonly Action<Exception>? _onException;
        readonly WeakEventManager _weakEventManager = new WeakEventManager();
        readonly TaskScheduler _scheduler;
        readonly bool _mustRunOnCurrentSyncContext;
        readonly Action<object> _execute;

        readonly IViewModelBase _viewModel;
        readonly bool _isBlocking;


        /// <summary>
        /// Initializes an instance of the <see cref="ICommand" class/>
        /// </summary>
        /// <param name="executeFunction">The function executed when
        /// <see cref="ICommand.Execute(object)"/> is called.</param>
        /// <param name="viewModel"> Option to pass the calling ViewModel
        /// via <c>this</c>. Will set <see cref="IViewModelBase.IsBusy"/>
        /// to <c>true</c> while <paramref name="executeFunction"/> is
        /// executing</param>
        /// <param name="canExecute">The function that verifies whether or
        /// not <see cref="SafeCommand"/>
        /// should execute</param>
        /// <param name="onException">If an exception is thrown,
        /// <paramref name="onException"/> will execute.</param>
        /// <param name="mustRunOnCurrentSyncContext"><c>false</c> by default
        /// , causing <paramref name="executeFunction"/> to run on a background
        /// thread. If set to <c>true</c>, will run on the current thread.
        /// In Xamarin.Forms this will be the Main (UI) thread.</param>
        public SafeCommand(Func<Task> executeFunction,
            IViewModelBase viewModel = null,
            Action<Exception>? onException = null,
            Func<bool>? canExecute = null,
            bool mustRunOnCurrentSyncContext = false,
            bool isBlocking = true)
            : this(
                  internalExecuteFunction:
                    o => executeFunction(),
                  viewModel,
                  onException,
                  o => canExecute?.Invoke() ?? true,
                  mustRunOnCurrentSyncContext,
                  isBlocking)
        {
            if (executeFunction is null)
                throw new ArgumentNullException(nameof(executeFunction)
                    , $"{nameof(executeFunction)} cannot be null");
        }

        /// <summary>
        /// Initializes an instance of the <see cref="ICommand" class/>
        /// </summary>
        /// <param name="executeFunction">The function executed when
        /// <see cref="ICommand.Execute(object)"/> is called.</param>
        public SafeCommand(Func<Task> executeFunction)
            : this(executeFunction, null, null, null, false, true)
        { }


        /// <summary>
        /// Initializes an instance of the <see cref="ICommand" class/>
        /// </summary>
        /// <remarks>Warning! If <paramref name="executeAction"/> is set to a lambda expression <c>() => { }</c>,
        /// the compiler will choose the wrong <c>Func<T,Task></c> overload instead.
        /// To be explicit, either abstract the lambda into a separate method
        /// (where it will be obvious to the compiler, or specify the parameter name before the lambda
        /// as so: <c>executeAction: () => { }</c>.
        /// The compiler's logic is that a delegate with a return type is a better match
        /// than a delegate without a return type.
        /// </remarks>
        /// <param name="executeAction">The function executed when
        /// <see cref="ICommand.Execute(object)"/> is called.</param>
        /// <param name="viewModel"> Option to pass the calling ViewModel
        /// via <c>this</c>. Will set <see cref="IViewModelBase.IsBusy"/>
        /// to <c>true</c> while <paramref name="executeAction"/> is
        /// executing</param>
        /// <param name="canExecute">The function that verifies whether or
        /// not <see cref="SafeCommand"/>
        /// should execute</param>
        /// <param name="onException">If an exception is thrown,
        /// <paramref name="onException"/> will execute.</param>
        /// <param name="mustRunOnCurrentSyncContext"><c>false</c> by default
        /// , causing <paramref name="executeAction"/> to run on a background
        /// thread. If set to <c>true</c>, will run on the current thread.
        /// In Xamarin.Forms this will be the Main (UI) thread.</param>
        public SafeCommand(
            Action executeAction,
            IViewModelBase viewModel = null,
            Action<Exception>? onException = null,
            Func<bool>? canExecute = null,
            bool mustRunOnCurrentSyncContext = false,
            bool isBlocking = true)
            : this(
                  internalExecuteAction:
                    o => executeAction(),
                  viewModel,
                  onException,
                  o =>
                  {
                      if (canExecute is null)
                          return true;
                      else
                          return canExecute();
                  },
                  mustRunOnCurrentSyncContext,
                  isBlocking)
        {
            if (executeAction == null)
                throw new ArgumentNullException(nameof(executeAction));
        }

        /// <summary>
        /// Initializes an instance of the <see cref="ICommand" class/>
        /// </summary>
        /// <remarks>Warning! If <paramref name="executeAction"/> is set to a lambda expression <c>() => { }</c>,
        /// the compiler will choose the wrong <c>Func<T,Task></c> overload instead.
        /// To be explicit, either abstract the lambda into a separate method
        /// (where it will be obvious to the compiler, or specify the parameter name before the lambda
        /// as so: <c>executeAction: () => { }</c>.
        /// The compiler's logic is that a delegate with a return type is a better match
        /// than a delegate without a return type.
        /// </remarks>
        /// <param name="executeAction">The function executed when
        /// <see cref="ICommand.Execute(object)"/> is called.</param>
        public SafeCommand(Action executeAction)
            : this(executeAction: executeAction, null, null, null, false, true)
        { }

        /// <summary>
        /// For Unit Testing. Command runs using the specified <see cref="TaskScheduler"/>
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] //Designed for Testing purposes only
        public SafeCommand(
            Func<object?, Task> executeFunction,
            TaskScheduler scheduler,
            IViewModelBase viewModel = null,
            Action<Exception>? onException = null,
            Func<object?, bool>? canExecute = null,
            //mustRunOnCurrentSyncContext is moot
            bool isBlocking = true
            )
            : this(internalExecuteFunction: executeFunction, viewModel, onException, canExecute, false, isBlocking)
        {
            _scheduler = scheduler
                ?? throw new ArgumentNullException(nameof(scheduler)
                        , $"{nameof(scheduler)} cannot be null");
        }

        #region Internal Constructors

        protected SafeCommand(
            Func<object, Task> internalExecuteFunction,
            IViewModelBase viewModel,
            Action<Exception>? onException,
            Func<object?, bool>? canExecute,
            bool mustRunOnCurrentSyncContext,
            bool isBlocking)
            : this(viewModel, onException, baseCanExecute: canExecute, mustRunOnCurrentSyncContext, isBlocking)
        {
            _executeAsync = internalExecuteFunction
                ?? throw new ArgumentNullException(nameof(internalExecuteFunction)
                        , $"{nameof(internalExecuteFunction)} cannot be null");
        }

        protected SafeCommand(
            Action<object> internalExecuteAction,
            IViewModelBase viewModel,
            Action<Exception>? onException,
            Func<object?, bool>? canExecute,
            bool mustRunOnCurrentSyncContext,
            bool isBlocking)
            : this(viewModel, onException, baseCanExecute: canExecute, mustRunOnCurrentSyncContext, isBlocking)
        {
            _execute = internalExecuteAction
                ?? throw new ArgumentNullException(nameof(internalExecuteAction)
                        , $"{nameof(internalExecuteAction)} cannot be null");
        }

        private SafeCommand(
            IViewModelBase viewModel,
            Action<Exception>? onException,
            Func<object?, bool>? baseCanExecute,
            bool mustRunOnCurrentSyncContext,
            bool isBlocking)
        {
            _isBlocking = isBlocking;
            _viewModel = viewModel;
            _canExecute = baseCanExecute ?? (_ => true);
            _onException = onException;
            _mustRunOnCurrentSyncContext = mustRunOnCurrentSyncContext;
        }
        #endregion

        #region ICommand implementation

        ///<inheritdoc/>
        public event EventHandler CanExecuteChanged
        {
            add => _weakEventManager.AddEventHandler(value);
            remove => _weakEventManager.RemoveEventHandler(value);
        }

        ///<inheritdoc/>
        public bool CanExecute(object? parameter) => _canExecute(parameter);

        ///<inheritdoc/>
        public void Execute(object parameter)
        {
            if (_execute != null)
                InternalExecute(parameter);
            else
                InternalExecuteAsync(parameter);
        }

        #endregion

        ///<inheritdoc/>
        public void RaiseCanExecuteChanged() => _weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(CanExecuteChanged));

        ///<inheritdoc/>
        public async Task RawExecuteAsync(object parameter)
        {
            if (_execute != null)
                _execute(parameter);
            else
                await _executeAsync(parameter);
        }

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

        Task InternalExecuteAsync(object parameter)
        {
            if (IsBusy)
                return Task.CompletedTask;

            if (_isBlocking)
                IsBusy = true;

            if (_scheduler != null)
                return
                    Task.Factory
                    .StartNew(() => _executeAsync(parameter).GetAwaiter().GetResult(),
                                    CancellationToken.None,
                                    TaskCreationOptions.DenyChildAttach,
                                    _scheduler)
                    .SafeContinueWith(_onException, _scheduler) //Handles exception if faulted
                    .ContinueWith(t => IsBusy = false,
                                    CancellationToken.None,
                                    TaskContinuationOptions.None,
                                    _scheduler);

            if (_mustRunOnCurrentSyncContext)
                return
                    _executeAsync(parameter)
                    .SafeContinueWith(_onException)
                    .ContinueWith(t => IsBusy = false);

            return //run on TaskPool
                Task.Run(() => _executeAsync(parameter).GetAwaiter().GetResult())
                               .SafeContinueWith(_onException)
                               .ContinueWith(t => IsBusy = false);
        }

        void InternalExecute(object parameter)
        {
            if (IsBusy)
                return;

            if (_isBlocking)
                IsBusy = true;

            try
            {
                if (_mustRunOnCurrentSyncContext)
                    _execute.SafeInvoke(parameter, _onException);

                else //run on TaskPool
                    Task.Run(() => _execute.SafeInvoke(parameter, _onException)).GetAwaiter().GetResult();
            }
            finally { IsBusy = false; }
        }
    }
}