using System;
using Xunit;
using XamarinFormsMvvmAdaptor.Helpers;
using System.Threading;
using System.Windows.Input;

namespace XamarinFormsMvvmAdaptor.Tests
{
    [Collection("SafeTests")]
	public class IntegratedAsyncCommandTests
	{
        [Fact]
		public void Constructor()
		{
			var cmd = new AsyncCommand(() => { });
			Assert.True(cmd.CanExecute(null));
		}

		[Fact]
		public void ThrowsWithNullConstructor()
		{
			Assert.Throws<ArgumentNullException>(() => new AsyncCommand(executeAction: (Action)null));
		}

		[Fact]
		public void ThrowsWithNullParameterizedConstructor()
		{
			Assert.Throws<ArgumentNullException>(() => new AsyncCommand((Action<object>)null));
		}

		[Fact]
		public void Execute_WithNullExecuteAndValidCanExecute_Throws()
		{
			Assert.Throws<ArgumentNullException>(() => new AsyncCommand(executeAction: null, () => true));
		}

		[Fact]
		public void Execute()
		{
			bool executed = false;
			var cmd = new AsyncCommand(() => executed = true);

			cmd.Execute(null);
			Assert.True(executed);
		}

		[Fact]
		public void ExecuteParameterized()
		{
			object executed = null;
			var cmd = new AsyncCommand<object>(executeAction: o => executed = o);

			var expected = new object();
			cmd.Execute(expected);

			Assert.Equal(expected, executed);
		}

		[Fact]
		public void ExecuteWithCanExecute()
		{
			bool executed = false;
			var cmd = new AsyncCommand(() => executed = true, () => true);

			cmd.Execute(null);
			Assert.True(executed);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void CanExecute(bool expected)
		{
			bool canExecuteRan = false;
			var cmd = new AsyncCommand(() => { }, () => {
				canExecuteRan = true;
				return expected;
			});

			Assert.Equal(expected, cmd.CanExecute(null));
			Assert.True(canExecuteRan);
		}

		[Fact]
		public void ChangeCanExecute()
		{
			bool signaled = false;
			var cmd = new AsyncCommand(() => { });

			cmd.CanExecuteChanged += (sender, args) => signaled = true;

			cmd.RaiseCanExecuteChanged();
			Assert.True(signaled);
		}

		[Fact]
		public void GenericThrowsWithNullExecute()
		{
			Assert.Throws<ArgumentNullException>(() => new AsyncCommand<string>(null));
		}

		[Fact]
		public void GenericThrowsWithNullExecuteAndCanExecuteValid()
		{
			Assert.Throws<ArgumentNullException>(() => new AsyncCommand<string>(null, s => true));
		}

		[Fact]
		public void GenericExecute()
		{
			string result = null;
			var cmd = new AsyncCommand<string>(s => result = s);

			cmd.Execute("Foo");
			Assert.Equal("Foo", result);
		}

		[Fact]
		public void GenericExecuteWithCanExecute()
		{
			string result = null;
			var cmd = new AsyncCommand<string>(s => result = s, s => true);

			cmd.Execute("Foo");
			Assert.Equal("Foo", result);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void GenericCanExecute(bool expected)
		{
			string result = null;
			var cmd = new AsyncCommand<string>(s => { }, s => {
				result = s;
				return expected;
			});

			Assert.Equal(expected, cmd.CanExecute("Foo"));
			Assert.Equal("Foo", result);
		}

		class FakeParentContext
		{
		}

		// ReSharper disable once ClassNeverInstantiated.Local
		class FakeChildContext
		{
		}

		[Fact]
		public void CanExecuteReturnsFalseIfParameterIsWrongReferenceType()
		{
			var command = new AsyncCommand<FakeChildContext>(executeAction: context => { }, context => true);

			Assert.False(command.CanExecute(new FakeParentContext()), "the parameter is of the wrong type");
		}

		[Fact]
		public void CanExecuteReturnsFalseIfParameterIsWrongValueType()
		{
			var command = new AsyncCommand<int>(executeAction: context => { }, context => true);

			Assert.False(command.CanExecute(10.5), "the parameter is of the wrong type");
		}

		[Fact]
		public void CanExecuteUsesParameterIfReferenceTypeAndSetToNull()
		{
			var command = new AsyncCommand<FakeChildContext>(context => { }, context => true);

			Assert.True(command.CanExecute(null), "null is a valid value for a reference type");
		}

		[Fact]
		public void CanExecuteUsesParameterIfNullableAndSetToNull()
		{
			var command = new AsyncCommand<int?>(context => { }, context => true);

			Assert.True(command.CanExecute(null), "null is a valid value for a Nullable<int> type");
		}

		[Fact]
		public void CanExecute_ValueTypeAndSetToNull_IgnoresParameter()
		{
			var command = new AsyncCommand<int>(executeAction: context => { }, context => true);

			Assert.False(command.CanExecute(null));
		}

		[Fact]
		public void Execute_ParameterIsWrongReferenceType_ThrowsInvalidCommandParameterException()
		{
			int executions = 0;
			var Command = new AsyncCommand<FakeChildContext>(executeAction: context => executions += 1);

			Assert.Throws<InvalidCommandParameterException>(() => Command.Execute(new FakeParentContext()));
			Assert.True(executions == 0); //, "the Command should not have executed");
		}

		[Fact]
		public void Execute_ParameterIsWrongValueType_ThrowsInvalidCommandParameterException()
		{
			int executions = 0;
			var Command = new AsyncCommand<int>(executeAction: context => executions += 1);

			Assert.Throws<InvalidCommandParameterException>(() => Command.Execute(10.5));
			Assert.True(executions == 0);// "the Command should not have executed");
		}

		[Fact]
		public void ExecuteRunsIfReferenceTypeAndSetToNull()
		{
			int executions = 0;
			var Command = new AsyncCommand<FakeChildContext>(context => executions += 1);

			var exception = Record.Exception(() => Command.Execute(null));
			Assert.Null(exception);
			//"null is a valid value for a reference type");
			Assert.True(executions == 1, "the Command should have executed");
		}

		[Fact]
		public void Execute_NullableAndSetToNull_Runs()
		{
			int executions = 0;
			var Command = new AsyncCommand<int?>(executeAction: context => executions += 1);

			var exception = Record.Exception(() => Command.Execute(null));
			//"null is a valid value for a Nullable<int> type");
			Assert.Null(exception);
			Assert.True(executions == 1);// "the Command should have executed");
		}

		[Fact]
		public void Execute_ValueTypeAndSetToNull_ThrowsInvalidCommandParameterException()
		{
			int executions = 0;
			var Command = new AsyncCommand<int>(executeAction: context => executions += 1);

			Assert.Throws<InvalidCommandParameterException>(() => Command.Execute(null));
			Assert.True(executions == 0, "the Command should not have executed");
		}

        #region GA Safe Exception Tests
		[Fact]
		public void Execute_TargetThrows_ThrowsException()
        {
			var command = new AsyncCommand(executeAction:() => throw new Exception());

            Assert.Throws<Exception>(() => command.Execute(null));
        }

		[Fact]
		public void Execute_WithOnException_TargetThrows_ExceptionHandled()
		{
			bool isHandled = false;
			void onException(Exception ex) { isHandled = true; }
			var command = new AsyncCommand(executeAction: () => throw new Exception(),null, onException);

			command.Execute(null);

			Assert.True(isHandled);
		}
		#endregion

		#region GA - Thread Tests on Command<T>
		[Theory]
		[InlineData("Hello")]
		public void ExecuteT_RunsOnNewThread(string parameter)
		{
			ICommand command = new AsyncCommand<string>(MockTask);

			Thread callingThread = null, executingThread = null;

			void MockTask(string text)
			{
				executingThread = Thread.CurrentThread;
			}

			var thread = new Thread(new ThreadStart(() =>
			{
				callingThread = Thread.CurrentThread;
				command.Execute(parameter);
			}));

			thread.Start();
			thread.Join();

			Assert.NotEqual(callingThread.ManagedThreadId, executingThread.ManagedThreadId);
		}

		[Theory]
		[InlineData("Hello")]
		public void ExecuteT_RunsOnTaskPool(string parameter)
		{
			ICommand command = new AsyncCommand<string>(MockTask);

			Thread callingThread = null, executingThread = null;

			void MockTask(string text)
			{
				executingThread = Thread.CurrentThread;
			}

			var thread = new Thread(new ThreadStart(() =>
			{
				callingThread = Thread.CurrentThread;
				command.Execute(parameter);
			}));


			thread.Start();
			thread.Join();

			Assert.False(callingThread.IsThreadPoolThread);
			Assert.True(executingThread.IsThreadPoolThread);
		}


		[Theory]
		[InlineData("Hello")]
		public void ExecuteT_MustRunOnCurrentSyncContextTrue_RunsOnCurrentThread(string parameter)
		{
			ICommand command = new AsyncCommand<string>(MockTask, null, null, true);

			Thread callingThread = null, executingThread = null;

			void MockTask(string text)
			{
				executingThread = Thread.CurrentThread;
			}

			var thread = new Thread(new ThreadStart(() =>
			{
				callingThread = Thread.CurrentThread;
				command.Execute(parameter);
			}));

			thread.Start();
			thread.Join();

			Assert.Equal(callingThread.ManagedThreadId, executingThread.ManagedThreadId);
		}

		[Theory]
		[InlineData("Hello")]
		public void ExecuteT_MustRunOnCurrentSyncContextTrue_NotRunOnTaskPool(string parameter)
		{
			ICommand command = new AsyncCommand<string>(MockTask, null, null, true);

			Thread callingThread = null, executingThread = null;

			void MockTask(string text)
			{
				executingThread = Thread.CurrentThread;
			}

			var thread = new Thread(new ThreadStart(() =>
			{
				callingThread = Thread.CurrentThread;
				command.Execute(parameter);
			}));

			thread.Start();
			thread.Join();

			Assert.False(executingThread.IsThreadPoolThread);
			Assert.False(callingThread.IsThreadPoolThread);
		}

		#endregion

		#region GA - Thread Tests on AsyncCommand

		[Fact]
		public void Execute_RunsOnNewThread()
		{
			ICommand command = new AsyncCommand(MockTask);

			Thread callingThread = null, executingThread = null;

			void MockTask()
			{
				executingThread = Thread.CurrentThread;
			}

			var thread = new Thread(new ThreadStart(() =>
			{
				callingThread = Thread.CurrentThread;
				command.Execute(null);
			}));

			thread.Start();
			thread.Join();

			Assert.NotEqual(callingThread.ManagedThreadId, executingThread.ManagedThreadId);
		}

		[Fact]
		public void Execute_RunsOnTaskPool()
		{
			ICommand command = new AsyncCommand(MockTask);

			Thread callingThread = null, executingThread = null;

			void MockTask()
			{
				executingThread = Thread.CurrentThread;
			}

			var thread = new Thread(new ThreadStart(() =>
			{
				callingThread = Thread.CurrentThread;
				command.Execute(null);
			}));

			thread.Start();
			thread.Join();

			Assert.False(callingThread.IsThreadPoolThread);
			Assert.True(executingThread.IsThreadPoolThread);
		}

		[Fact]
		public void Execute_MustRunOnCurrentSyncContextTrue_RunsOnCurrentThread()
		{
			ICommand command = new AsyncCommand(MockTask, null, null, true);

			Thread callingThread = null, executingThread = null;

			void MockTask()
			{
				executingThread = Thread.CurrentThread;
			}

			var thread = new Thread(new ThreadStart(() =>
			{
				callingThread = Thread.CurrentThread;
				command.Execute(null);
			}));

			thread.Start();
			thread.Join();

			Assert.Equal(callingThread.ManagedThreadId, executingThread.ManagedThreadId);
		}

		[Fact]
		public void Execute_MustRunOnCurrentSyncContextTrue_NotRunOnTaskPool()
		{
			ICommand command = new AsyncCommand(MockTask, null, null, true);

			Thread callingThread = null, executingThread = null;

			void MockTask()
			{
				executingThread = Thread.CurrentThread;
			}

			var thread = new Thread(new ThreadStart(() =>
			{
				callingThread = Thread.CurrentThread;
				command.Execute(null);
			}));

			thread.Start();
			thread.Join();

			Assert.False(executingThread.IsThreadPoolThread);
			Assert.False(callingThread.IsThreadPoolThread);
		}
		#endregion

		#region Monkey Tests
		[Fact]
		public void Execute_CalledTwice_SecondExecuteWaitsForFirstToComplete()
		{
			//Runs synchronously
			int times = 0;
			void MockTask(string text)
			{
				Thread.Sleep(10);
				times++;
			}

			ICommand command = new AsyncCommand<string>(MockTask);

			command.Execute("test");
			command.Execute("test");

			Assert.Equal(2, times);
		}

		[Fact]
		public void Execute_CalledTwice_FiresTwiceIfNotBusy()
		{
			int times = 0;
			void MockTask(string text)
			{
				times++;
			}
			var dts = new DeterministicTaskScheduler(shouldThrowExceptions: false);
			ICommand command = new AsyncCommand<string>(MockTask);

			command.Execute("test");
			dts.RunTasksUntilIdle();

			command.Execute("test");
			dts.RunTasksUntilIdle();

			Assert.Equal(2, times);
		}

		[Fact]
		public void Execute_OnExceptionSet_SecondCallAfterException_Executes()
		{
			int times = 0;
			bool isHandled = false;
			void MockTask(string text)
			{
				Thread.Sleep(10);
				if (times++ == 0)
					throw new Exception(); //Throws only on first try
			}


			ICommand command = new AsyncCommand<string>(MockTask,null, (ex) => { isHandled = true; });

			command.Execute("test"); //should throw

			command.Execute("test");

			Assert.True(isHandled);
			Assert.Equal(2, times);
		}

		[Fact]
		public void Execute_SecondCallAfterException_Executes()
		{
			int times = 0;
			void MockTask(string text)
			{
				if (times++ == 0)
					throw new Exception(); //Throws only on first try
			}

			ICommand command = new AsyncCommand<string>(MockTask);

			Assert.Throws<Exception>(()=>command.Execute("test"));

			command.Execute("test");

			Assert.Equal(2, times);
		}

		#endregion


	}
}