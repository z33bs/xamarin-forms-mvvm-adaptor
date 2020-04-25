using System;
using Xunit;
using XamarinFormsMvvmAdaptor.Helpers;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Input;

namespace XamarinFormsMvvmAdaptor.Tests
{
    [Collection("SafeTests")]
	public class SafeCommandTests
	{
        const int Delay = 100;

        [Fact]
		public void Constructor()
		{
			var cmd = new SafeCommand(() => { });
			Assert.True(cmd.CanExecute(null));
		}

		[Fact]
		public void ThrowsWithNullConstructor()
		{
			Assert.Throws<ArgumentNullException>(() => new SafeCommand((Action)null));
		}

		[Fact]
		public void ThrowsWithNullParameterizedConstructor()
		{
			Assert.Throws<ArgumentNullException>(() => new SafeCommand((Action<object>)null));
		}

		[Fact]
		public void ThrowsWithNullCanExecute()
		{
			Assert.Throws<ArgumentNullException>(() => new SafeCommand(() => { }, null));
		}

		[Fact]
		public void ThrowsWithNullParameterizedCanExecute()
		{
			Assert.Throws<ArgumentNullException>(() => new SafeCommand(o => { }, null));
		}

		[Fact]
		public void ThrowsWithNullExecuteValidCanExecute()
		{
			Assert.Throws<ArgumentNullException>(() => new SafeCommand(null, () => true));
		}

		[Fact]
		public void Execute()
		{
			bool executed = false;
			var cmd = new SafeCommand(() => executed = true);

			cmd.Execute(null);
			Assert.True(executed);
		}

		[Fact]
		public void ExecuteParameterized()
		{
			object executed = null;
			var cmd = new SafeCommand(o => executed = o);

			var expected = new object();
			cmd.Execute(expected);

			Assert.Equal(expected, executed);
		}

		[Fact]
		public void ExecuteWithCanExecute()
		{
			bool executed = false;
			var cmd = new SafeCommand(() => executed = true, () => true);

			cmd.Execute(null);
			Assert.True(executed);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void CanExecute(bool expected)
		{
			bool canExecuteRan = false;
			var cmd = new SafeCommand(() => { }, () => {
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
			var cmd = new SafeCommand(() => { });

			cmd.CanExecuteChanged += (sender, args) => signaled = true;

			cmd.ChangeCanExecute();
			Assert.True(signaled);
		}

		[Fact]
		public void GenericThrowsWithNullExecute()
		{
			Assert.Throws<ArgumentNullException>(() => new SafeCommand<string>(null));
		}

		[Fact]
		public void GenericThrowsWithNullExecuteAndCanExecuteValid()
		{
			Assert.Throws<ArgumentNullException>(() => new SafeCommand<string>(null, s => true));
		}

		[Fact]
		public void GenericThrowsWithValidExecuteAndCanExecuteNull()
		{
			Assert.Throws<ArgumentNullException>(() => new SafeCommand<string>(s => { }, null));
		}

		[Fact]
		public void GenericExecute()
		{
			string result = null;
			var cmd = new SafeCommand<string>(s => result = s);

			cmd.Execute("Foo");
			Assert.Equal("Foo", result);
		}

		[Fact]
		public void GenericExecuteWithCanExecute()
		{
			string result = null;
			var cmd = new SafeCommand<string>(s => result = s, s => true);

			cmd.Execute("Foo");
			Assert.Equal("Foo", result);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void GenericCanExecute(bool expected)
		{
			string result = null;
			var cmd = new SafeCommand<string>(s => { }, s => {
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
			var command = new SafeCommand<FakeChildContext>(context => { }, context => true);

			Assert.False(command.CanExecute(new FakeParentContext()), "the parameter is of the wrong type");
		}

		[Fact]
		public void CanExecuteReturnsFalseIfParameterIsWrongValueType()
		{
			var command = new SafeCommand<int>(context => { }, context => true);

			Assert.False(command.CanExecute(10.5), "the parameter is of the wrong type");
		}

		[Fact]
		public void CanExecuteUsesParameterIfReferenceTypeAndSetToNull()
		{
			var command = new SafeCommand<FakeChildContext>(context => { }, context => true);

			Assert.True(command.CanExecute(null), "null is a valid value for a reference type");
		}

		[Fact]
		public void CanExecuteUsesParameterIfNullableAndSetToNull()
		{
			var command = new SafeCommand<int?>(context => { }, context => true);

			Assert.True(command.CanExecute(null), "null is a valid value for a Nullable<int> type");
		}

		[Fact]
		public void CanExecuteIgnoresParameterIfValueTypeAndSetToNull()
		{
			var command = new SafeCommand<int>(context => { }, context => true);

			Assert.False(command.CanExecute(null), "null is not a valid valid for int");
		}

		[Fact]
		public void ExecuteDoesNotRunIfParameterIsWrongReferenceType()
		{
			int executions = 0;
			var Command = new SafeCommand<FakeChildContext>(context => executions += 1);

//			Assert.DoesNotThrow(() => Command.Execute(new FakeParentContext()), "the Command should not execute, so no exception should be thrown");
			var exception = Record.Exception(() => Command.Execute(new FakeParentContext()));
			Assert.Null(exception);

			Assert.True(executions == 0, "the Command should not have executed");
		}

		[Fact]
		public void ExecuteDoesNotRunIfParameterIsWrongValueType()
		{
			int executions = 0;
			var Command = new SafeCommand<int>(context => executions += 1);

//			Assert.DoesNotThrow(() => Command.Execute(10.5), "the Command should not execute, so no exception should be thrown");
			var exception = Record.Exception(() => Command.Execute(10.5));
			Assert.Null(exception);

			Assert.True(executions == 0, "the Command should not have executed");
		}

		[Fact]
		public void ExecuteRunsIfReferenceTypeAndSetToNull()
		{
			int executions = 0;
			var Command = new SafeCommand<FakeChildContext>(context => executions += 1);

			var exception = Record.Exception(() => Command.Execute(null));
			Assert.Null(exception);
			//"null is a valid value for a reference type");
			Assert.True(executions == 1, "the Command should have executed");
		}

		[Fact]
		public void ExecuteRunsIfNullableAndSetToNull()
		{
			int executions = 0;
			var Command = new SafeCommand<int?>(context => executions += 1);

			var exception = Record.Exception(() => Command.Execute(null));
			//"null is a valid value for a Nullable<int> type");
			Assert.Null(exception);
			Assert.True(executions == 1, "the Command should have executed");
		}

		[Fact]
		public void ExecuteDoesNotRunIfValueTypeAndSetToNull()
		{
			int executions = 0;
			var Command = new SafeCommand<int>(context => executions += 1);

			var exception = Record.Exception(() => Command.Execute(null));
			Assert.Null(exception);
			Assert.True(executions == 0, "the Command should not have executed");
		}

        #region GA Safe Exception Tests
		[Fact]
		public void Execute_TargetThrows_ThrowsException()
        {
			var command = new SafeCommand(() => throw new Exception());

            Assert.Throws<Exception>(() => command.Execute(null));
        }

		[Fact]
		public void Execute_ParameterOnExceptionSet_TargetThrows_ExceptionHandled()
		{
			bool isHandled = false;
			void onException(Exception ex) { isHandled = true; }
			var command = new SafeCommand(() => throw new Exception(),onException);

			command.Execute(null);

			Assert.True(isHandled);
		}
        #endregion

        #region GA - Thread Tests <T>
        [Theory]
        [InlineData("Hello")]
        public void ExecuteT_InForeground_RunsOnNewThread(string parameter)
        {
            //Arrange
            int callingThreadId = -1, executingThreadId = -1;

            void MockMethod(string text)
            {
                executingThreadId = Thread.CurrentThread.ManagedThreadId;
                Thread.Sleep(Delay);
            }

            ICommand command = new SafeCommand<string>(MockMethod);

            //Act
            var thread = new Thread(new ThreadStart(() =>
            {
                callingThreadId = Thread.CurrentThread.ManagedThreadId;

                command.Execute(parameter);
            }));

            thread.Start();
            Thread.Sleep(Delay);
            thread.Join();

            //Assert
            Assert.NotEqual(callingThreadId, executingThreadId);
        }

        [Theory]
        [InlineData("Hello")]
        public void ExecuteAsyncT_InForeground_RunsOnTaskPool(string parameter)
        {
            bool isTpThread_Calling = true, isTpThread_Executing = false;

            void MockMethod(string text)
            {
                isTpThread_Executing = Thread.CurrentThread.IsThreadPoolThread;
                Thread.Sleep(Delay);
            }

            ICommand command = new SafeCommand<string>(MockMethod);

            var thread = new Thread(new ThreadStart(() =>
            {
                isTpThread_Calling = Thread.CurrentThread.IsThreadPoolThread;
                command.Execute(parameter);
            }));

            thread.Start();
            Thread.Sleep(Delay);
            thread.Join();

            Assert.False(isTpThread_Calling);
            Assert.True(isTpThread_Executing);
        }

        [Theory]
        [InlineData("Hello")]
        public void ExecuteAsyncT_InBackground_RunsOnCurrentThread(string parameter)
        {
            //Arrange
            int callingThreadId = -1, taskThreadId = -1;

            void MockMethod(string text)
            {
                taskThreadId = Thread.CurrentThread.ManagedThreadId;
                Thread.Sleep(Delay);
            }

            ICommand command = new SafeCommand<string>(MockMethod);


            //Act
            var thread = new Thread(new ThreadStart(() =>
            {
                callingThreadId = Thread.CurrentThread.ManagedThreadId;

                command.Execute(parameter);
            }))
            { IsBackground = true };

            thread.Start();
            Thread.Sleep(Delay);
            thread.Join();
            //Assert
            Assert.Equal(callingThreadId, taskThreadId);
        }

        [Theory]
        [InlineData("Hello")]
        public void ExecuteAsyncT_InBackground_NotRunOnTaskPool(string parameter)
        {
            bool isTpThread_Calling = true, isTpThread_Executing = true;

            void MockMethod(string text)
            {
                isTpThread_Executing = Thread.CurrentThread.IsThreadPoolThread;
                Thread.Sleep(Delay);
            }

            ICommand command = new SafeCommand<string>(MockMethod);

            var thread = new Thread(new ThreadStart(() =>
            {
                isTpThread_Calling = Thread.CurrentThread.IsThreadPoolThread;
                command.Execute(parameter);
            }))
            { IsBackground = true };

            thread.Start();
            Thread.Sleep(Delay);
            thread.Join();

            Assert.False(isTpThread_Calling);
            Assert.False(isTpThread_Executing);
        }
        #endregion

        #region GA - Thread Tests <T> With onBackgroundThread = false
        [Theory]
        [InlineData("Hello")]
        public void ExecuteAsyncT_ParameterOnBackgoundIsFalse_InForeground_RunsOnCurrentThread(string parameter)
        {
            //Arrange
            int callingThreadId = -1, executingThreadId = -1;

            void MockMethod(string text)
            {
                executingThreadId = Thread.CurrentThread.ManagedThreadId;
                Thread.Sleep(Delay);
            }

            ICommand command = new SafeCommand<string>(MockMethod, null, false);

            //Act
            var thread = new Thread(new ThreadStart(() =>
            {
                callingThreadId = Thread.CurrentThread.ManagedThreadId;

                command.Execute(parameter);

            }));

            thread.Start();
            Thread.Sleep(Delay);
            thread.Join();

            //Assert
            Assert.Equal(callingThreadId, executingThreadId);
        }

        [Theory]
        [InlineData("Hello")]
        public void ExecuteAsyncT_ParameterOnBackgoundIsFalse_InForeground_NotRunOnTaskPool(string parameter)
        {
            bool isCallingThreadPoolThread = true, isExecutingThreadPoolThread = false;
            void MockMethod(string text)
            {
                isExecutingThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread;
                Thread.Sleep(Delay);
            }

            var thread = new Thread(new ThreadStart(() =>
            {
                ICommand command = new SafeCommand<string>(MockMethod, null, false);

                isCallingThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread;
                command.Execute(parameter);
            }));

            thread.Start();
            Thread.Sleep(Delay);
            thread.Join();

            Assert.False(isExecutingThreadPoolThread);
            Assert.False(isCallingThreadPoolThread);
        }

        [Theory]
        [InlineData("Hello")]
        public void ExecuteAsyncT_ParameterOnBackgoundIsFalse_InBackground_RunsOnCurrentThread(string parameter)
        {
            //Arrange
            int callingThreadId = -1, taskThreadId = -1;

            void MockMethod(string text)
            {
                taskThreadId = Thread.CurrentThread.ManagedThreadId;
                Thread.Sleep(Delay);
            }
            ICommand command = new SafeCommand<string>(MockMethod, null, false);


            //Act
            var thread = new Thread(new ThreadStart(() =>
            {
                callingThreadId = Thread.CurrentThread.ManagedThreadId;

                command.Execute(parameter);
            }))
            { IsBackground = true };

            thread.Start();
            Thread.Sleep(Delay);
            thread.Join();
            //Assert
            Assert.Equal(callingThreadId, taskThreadId);
        }

        [Theory]
        [InlineData("Hello")]
        public void ExecuteAsyncT_ParameterOnBackgoundIsFalse_InBackground_NotRunOnTaskPool(string parameter)
        {
            bool isTpThread_Calling = true, isTpThread_Executing = true;

            void MockMethod(string text)
            {
                isTpThread_Executing = Thread.CurrentThread.IsThreadPoolThread;
                Thread.Sleep(Delay);
            }

            ICommand command = new SafeCommand<string>(MockMethod, null, false);

            var thread = new Thread(new ThreadStart(() =>
            {
                isTpThread_Calling = Thread.CurrentThread.IsThreadPoolThread;
                command.Execute(parameter);
            }))
            { IsBackground = true };

            thread.Start();
            Thread.Sleep(Delay);
            thread.Join();

            Assert.False(isTpThread_Calling);
            Assert.False(isTpThread_Executing);
        }

        #endregion
        //
        #region GA - Thread Tests

        [Fact]
        public void ExecuteAsync_InForeground_RunsOnNewThread()
        {
            //Arrange
            int callingThreadId = -1, executingThreadId = -1;

            void MockMethod()
            {
                executingThreadId = Thread.CurrentThread.ManagedThreadId;
                Thread.Sleep(Delay);
            }

            ICommand command = new SafeCommand(MockMethod);

            //Act
            var thread = new Thread(new ThreadStart(() =>
            {
                callingThreadId = Thread.CurrentThread.ManagedThreadId;

                command.Execute(null);
            }));

            thread.Start();
            Thread.Sleep(Delay);
            thread.Join();

            //Assert
            Assert.NotEqual(callingThreadId, executingThreadId);
        }

        [Fact]
        public void ExecuteAsync_InForeground_RunsOnTaskPool()
        {
            bool isTpThread_Calling = true, isTpThread_Executing = false;

            void MockMethod()
            {
                isTpThread_Executing = Thread.CurrentThread.IsThreadPoolThread;
                Thread.Sleep(Delay);
            }

            ICommand command = new SafeCommand(MockMethod);

            var thread = new Thread(new ThreadStart(() =>
            {
                isTpThread_Calling = Thread.CurrentThread.IsThreadPoolThread;
                command.Execute(null);
            }));

            thread.Start();
            Thread.Sleep(Delay);
            thread.Join();

            Assert.False(isTpThread_Calling);
            Assert.True(isTpThread_Executing);
        }

        [Fact]
        public void ExecuteAsync_InBackground_RunsOnCurrentThread()
        {
            //Arrange
            int callingThreadId = -1, taskThreadId = -1;

            void MockMethod()
            {
                taskThreadId = Thread.CurrentThread.ManagedThreadId;
                Thread.Sleep(Delay);
            }

            ICommand command = new SafeCommand(MockMethod);


            //Act
            var thread = new Thread(new ThreadStart(() =>
            {
                callingThreadId = Thread.CurrentThread.ManagedThreadId;

                command.Execute(null);
            }))
            { IsBackground = true };

            thread.Start();
            Thread.Sleep(Delay);
            thread.Join();
            //Assert
            Assert.Equal(callingThreadId, taskThreadId);
        }

        [Fact]
        public void ExecuteAsync_InBackground_NotRunOnTaskPool()
        {
            bool isTpThread_Calling = true, isTpThread_Executing = true;

            void MockMethod()
            {
                isTpThread_Executing = Thread.CurrentThread.IsThreadPoolThread;
                Thread.Sleep(Delay);
            }

            ICommand command = new SafeCommand(MockMethod);

            var thread = new Thread(new ThreadStart(() =>
            {
                isTpThread_Calling = Thread.CurrentThread.IsThreadPoolThread;
                command.Execute(null);
            }))
            { IsBackground = true };

            thread.Start();
            Thread.Sleep(Delay);
            thread.Join();

            Assert.False(isTpThread_Calling);
            Assert.False(isTpThread_Executing);
        }
        #endregion

        #region GA - Thread Tests With onBackgroundThread = false
        [Fact]
        public void ExecuteAsync_ParameterOnBackgoundIsFalse_InForeground_RunsOnCurrentThread()
        {
            //Arrange
            int callingThreadId = -1, executingThreadId = -1;

            void MockMethod()
            {
                executingThreadId = Thread.CurrentThread.ManagedThreadId;
                Thread.Sleep(Delay);
            }

            ICommand command = new SafeCommand(MockMethod, null, false);

            //Act
            var thread = new Thread(new ThreadStart(() =>
            {
                callingThreadId = Thread.CurrentThread.ManagedThreadId;

                command.Execute(null);

            }));

            thread.Start();
            Thread.Sleep(Delay);
            thread.Join();

            //Assert
            Assert.Equal(callingThreadId, executingThreadId);
        }

        [Fact]
        public void ExecuteAsync_ParameterOnBackgoundIsFalse_InForeground_NotRunOnTaskPool()
        {
            bool isCallingThreadPoolThread = true, isExecutingThreadPoolThread = false;
            void MockMethod()
            {
                isExecutingThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread;
                Thread.Sleep(Delay);
            }

            var thread = new Thread(new ThreadStart(() =>
            {
                ICommand command = new SafeCommand(MockMethod, null, false);

                isCallingThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread;
                command.Execute(null);
            }));

            thread.Start();
            Thread.Sleep(Delay);
            thread.Join();

            Assert.False(isExecutingThreadPoolThread);
            Assert.False(isCallingThreadPoolThread);
        }

        [Fact]
        public void ExecuteAsync_ParameterOnBackgoundIsFalse_InBackground_RunsOnCurrentThread()
        {
            //Arrange
            int callingThreadId = -1, taskThreadId = -1;

            void MockMethod()
            {
                taskThreadId = Thread.CurrentThread.ManagedThreadId;
                Thread.Sleep(Delay);
            }
            ICommand command = new SafeCommand(MockMethod, null, false);


            //Act
            var thread = new Thread(new ThreadStart(() =>
            {
                callingThreadId = Thread.CurrentThread.ManagedThreadId;

                command.Execute(null);
            }))
            { IsBackground = true };

            thread.Start();
            Thread.Sleep(Delay);
            thread.Join();
            //Assert
            Assert.Equal(callingThreadId, taskThreadId);
        }

        [Fact]
        public void ExecuteAsync_ParameterOnBackgoundIsFalse_InBackground_NotRunOnTaskPool()
        {
            bool isTpThread_Calling = true, isTpThread_Executing = true;

            void MockMethod()
            {
                isTpThread_Executing = Thread.CurrentThread.IsThreadPoolThread;
                Thread.Sleep(Delay);
            }

            ICommand command = new SafeCommand(MockMethod, null, false);

            var thread = new Thread(new ThreadStart(() =>
            {
                isTpThread_Calling = Thread.CurrentThread.IsThreadPoolThread;
                command.Execute(null);
            }))
            { IsBackground = true };

            thread.Start();
            Thread.Sleep(Delay);
            thread.Join();

            Assert.False(isTpThread_Calling);
            Assert.False(isTpThread_Executing);
        }

        #endregion

    }
}