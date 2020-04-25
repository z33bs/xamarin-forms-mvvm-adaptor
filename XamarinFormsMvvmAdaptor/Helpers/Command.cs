using System;
using System.Reflection;
using System.Windows.Input;

namespace XamarinFormsMvvmAdaptor.Helpers
{
	public sealed class Command<T> : Command
	{
		public Command(Action<T> execute, Action<Exception>? onException = null)
					: base(o =>
					{
						if (IsValidParameter(o))
						{
							execute((T)o);
						}
					},onException)
		{
			if (execute == null)
			{
				throw new ArgumentNullException(nameof(execute));
			}
		}
		public Command(Action<T> execute, Func<T, bool> canExecute, Action<Exception>? onException = null)
					: base(o =>
					{
						if (IsValidParameter(o))
						{
							execute((T)o);
						}
					}
					, o => IsValidParameter(o) && canExecute((T)o)
					, onException)
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
	public class Command : ICommand
	{
		readonly Func<object, bool> _canExecute;
		readonly Action<object> _execute;
		readonly Action<Exception>? _onException;
		readonly WeakEventManager _weakEventManager = new WeakEventManager();
		public Command(Action<object> execute, Action<Exception>? onException = null)
		{
			if (execute == null)
				throw new ArgumentNullException(nameof(execute));
			_execute = execute;
			_onException = onException;
		}
		public Command(Action execute, Action<Exception>? onException = null) : this(o => execute(),onException)
		{
			if (execute == null)
				throw new ArgumentNullException(nameof(execute));
		}
		public Command(Action<object> execute, Func<object, bool> canExecute, Action<Exception>? onException = null) : this(execute, onException)
		{
			if (canExecute == null)
				throw new ArgumentNullException(nameof(canExecute));
			_canExecute = canExecute;
		}
		public Command(Action execute, Func<bool> canExecute, Action<Exception>? onException = null) : this(o => execute(), o => canExecute(), onException)
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
			try
			{
				_execute(parameter);
			}
			catch(Exception ex)
            {
				SafeFireAndForgetExtensions.HandleException(ex, _onException);
            }
		}
		public void ChangeCanExecute()
		{
			_weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(CanExecuteChanged));
		}
	}
}