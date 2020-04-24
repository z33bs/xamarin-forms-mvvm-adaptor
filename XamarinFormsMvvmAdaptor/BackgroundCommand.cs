using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace XamarinFormsMvvmAdaptor
{
    public sealed class BackgroundCommand<T> : BackgroundCommand
    {
        public BackgroundCommand(Action<T> execute)
            : base(o =>
            {
                if (IsValidParameter(o))
                {
                    execute((T)o);
                }
            })
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }
        }

        public BackgroundCommand(Func<T,Task> execute)
        
    : base(o =>
    {
        if (IsValidParameter(o))
        {
            execute((T)o);
        }
    })
        {
                if (execute == null)
                {
                    throw new ArgumentNullException(nameof(execute));
                }
            }

            public BackgroundCommand(Action<T> execute, Func<T, bool> canExecute)
            : base(o =>
            {
                if (IsValidParameter(o))
                {
                    execute((T)o);
                }
            }, o => IsValidParameter(o) && canExecute((T)o))
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            if (canExecute == null)
                throw new ArgumentNullException(nameof(canExecute));
        }

        public BackgroundCommand(Func<T,Task> execute, Func<T, bool> canExecute)
    : base(o =>
    {
        if (IsValidParameter(o))
        {
            execute((T)o);
        }
    }, o => IsValidParameter(o) && canExecute((T)o))
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            if (canExecute == null)
                throw new ArgumentNullException(nameof(canExecute));
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

    public class BackgroundCommand : ICommand
    {
        readonly Func<object, bool> _canExecute;
        readonly Action<object> _execute;
        protected readonly Func<Task> _executeAsync;
        readonly WeakEventManager _weakEventManager = new WeakEventManager();

        public BackgroundCommand(Action<object> execute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = execute;
        }

        public BackgroundCommand(Func<Task> execute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _executeAsync = execute;
        }

        public BackgroundCommand(Action execute) : this(o => execute())
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
        }

        public BackgroundCommand(Action<object> execute, Func<object, bool> canExecute) : this(execute)
        {
            if (canExecute == null)
                throw new ArgumentNullException(nameof(canExecute));

            _canExecute = canExecute;
        }

        public BackgroundCommand(Func<Task> execute, Func<object, bool> canExecute) : this(execute)
        {
            if (canExecute == null)
                throw new ArgumentNullException(nameof(canExecute));

            _canExecute = canExecute;
        }

        public BackgroundCommand(Action execute, Func<bool> canExecute) : this(o => execute(), o => canExecute())
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            if (canExecute == null)
                throw new ArgumentNullException(nameof(canExecute));
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute(parameter);

            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { _weakEventManager.AddEventHandler(value); }
            remove { _weakEventManager.RemoveEventHandler(value); }
        }

        public void Execute(object parameter)
        {
                if (_executeAsync is null)
            {
                if (Thread.CurrentThread.IsBackground)
                {
                    try
                    {
                        _execute(parameter);
                    } catch(Exception ex)
                    {
                        SafeFireAndForgetExtensions.HandleException(ex, null);
                    }
                }
                else
                    Task.Run(() => _execute(parameter)).SafeFireAndForget();// (ex) => Console.WriteLine("BackgroundCommand caught exception:"+ex.Message));
            }
            else
            {
                if (Thread.CurrentThread.IsBackground)
                    _executeAsync().SafeFireAndForget();
                else
                    Task.Run(_executeAsync).SafeFireAndForget();
            }
        }

        public void ChangeCanExecute()
        {
            _weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(CanExecuteChanged));
        }
    }
}