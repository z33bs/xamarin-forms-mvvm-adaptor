//Adapted from Xamarin.Forms https://github.com/xamarin/Xamarin.Forms
//Modifications marked with //GA

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace XamarinFormsMvvmAdaptor.Helpers
{
	public sealed class SafeCommand<T> : SafeCommand
	{
		public SafeCommand(Action<T> execute,
						   Action<Exception>? onException = null,
						   bool onBackgroundThread = true)
					: base(o =>
					{
						if (IsValidParameter(o))
						{
							execute((T)o);
						}
					},onException,onBackgroundThread)
		{
			if (execute == null)
			{
				throw new ArgumentNullException(nameof(execute));
			}
		}
		public SafeCommand(Action<T> execute,
						   Func<T, bool> canExecute,
						   Action<Exception>? onException = null,
						   bool onBackgroundThread = true)
					: base(o =>
					{
						if (IsValidParameter(o))
						{
							execute((T)o);
						}
					}
					, o => IsValidParameter(o) && canExecute((T)o)
					, onException, onBackgroundThread)
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
	public class SafeCommand : ICommand
	{
		readonly Func<object, bool> _canExecute;
		readonly Action<object> _execute;
		readonly Action<Exception>? _onException; //GA add
		readonly WeakEventManager _weakEventManager = new WeakEventManager();
		readonly bool _onBackgroundThread; //GA add

		public SafeCommand(Action<object> execute,
						   Action<Exception>? onException = null,
						   bool onBackgroundThread = true)
		{
			if (execute == null)
				throw new ArgumentNullException(nameof(execute));
			_execute = execute;
			_onException = onException;
			_onBackgroundThread = onBackgroundThread;
		}
		public SafeCommand(Action execute,
						   Action<Exception>? onException = null,
						   bool onBackgroundThread = true)
						   : this(o => execute(), onException, onBackgroundThread)
		{
			if (execute == null)
				throw new ArgumentNullException(nameof(execute));
		}
		public SafeCommand(Action<object> execute,
						   Func<object, bool> canExecute,
						   Action<Exception>? onException = null,
						   bool onBackgroundThread = true)
						   : this(execute, onException, onBackgroundThread)
		{
			if (canExecute == null)
				throw new ArgumentNullException(nameof(canExecute));
			_canExecute = canExecute;
		}
		public SafeCommand(Action execute,
						   Func<bool> canExecute,
						   Action<Exception>? onException = null,
						   bool onBackgroundThread = true)
						   : this(o => execute(), o => canExecute(), onException, onBackgroundThread)
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
            //GA change logic
            if (_onBackgroundThread)
            {
				if (Thread.CurrentThread.IsBackground)
					_execute.SafeInvoke(parameter, _onException);
				else
					Task
						.Run(() => _execute(parameter))
						.SafeTask(_onException)
						.GetAwaiter()
						.GetResult();
            }
			else
				_execute.SafeInvoke(parameter, _onException);
		}
		public void ChangeCanExecute()
		{
			_weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(CanExecuteChanged));
		}
	}
}