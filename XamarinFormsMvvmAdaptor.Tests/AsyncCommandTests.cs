using System;
using Xunit;
using XamarinFormsMvvmAdaptor.Helpers;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Input;

namespace XamarinFormsMvvmAdaptor.Tests
{
    [Collection("SafeTests")]
    public class AsyncCommandTests
    {
        const int Delay = 500;
        Task NoParameterTask() => Task.Delay(Delay);
        Task IntParameterTask(int delay) => Task.Delay(delay);
        Task StringParameterTask(string text) => Task.Delay(Delay);

        protected bool CanExecuteTrue(object? parameter) => true;
        protected bool CanExecuteFalse(object? parameter) => false;

        #region AsyncCommand
        [Fact]
        public void AsyncCommand_NullExecuteParameter()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 //Cannot convert null literal to non-nullable reference type
            Assert.Throws<ArgumentNullException>(() => new AsyncCommand(null));
#pragma warning restore CS8625
        }

        [Fact]
        public void AsyncCommandT_NullExecuteParameter()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 //Cannot convert null literal to non-nullable reference type
            Assert.Throws<ArgumentNullException>(() => new AsyncCommand<object>(null));
#pragma warning restore CS8625
        }

        [Theory]
        [InlineData(500)]
        [InlineData(0)]
        public async Task AsyncCommand_ExecuteAsync_IntParameter_Test(int parameter)
        {
            //Arrange
            AsyncCommand<int> command = new AsyncCommand<int>(IntParameterTask);

            //Act
            await command.ExecuteAsync(parameter);

            //Assert

        }

        [Theory]
        [InlineData("Hello")]
        [InlineData(default)]
        public async Task AsyncCommand_ExecuteAsync_StringParameter_Test(string parameter)
        {
            //Arrange
            AsyncCommand<string> command = new AsyncCommand<string>(StringParameterTask);

            //Act
            await command.ExecuteAsync(parameter);

            //Assert

        }

        [Fact]
        public void AsyncCommand_Parameter_CanExecuteTrue_Test()
        {
            //Arrange
            AsyncCommand<int> command = new AsyncCommand<int>(IntParameterTask, CanExecuteTrue);

            //Act

            //Assert

            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void AsyncCommand_Parameter_CanExecuteFalse_Test()
        {
            //Arrange
            AsyncCommand<int> command = new AsyncCommand<int>(IntParameterTask, CanExecuteFalse);

            //Act

            //Assert
            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void AsyncCommand_NoParameter_CanExecuteTrue_Test()
        {
            //Arrange
            AsyncCommand command = new AsyncCommand(NoParameterTask, CanExecuteTrue);

            //Act

            //Assert
            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void AsyncCommand_NoParameter_CanExecuteFalse_Test()
        {
            //Arrange
            AsyncCommand command = new AsyncCommand(NoParameterTask, CanExecuteFalse);

            //Act

            //Assert
            Assert.False(command.CanExecute(null));
        }


        [Fact]
        public void AsyncCommand_CanExecuteChanged_Test()
        {
            //Arrange
            bool canCommandExecute = false;
            bool didCanExecuteChangeFire = false;

            AsyncCommand command = new AsyncCommand(NoParameterTask, commandCanExecute);
            command.CanExecuteChanged += handleCanExecuteChanged;

            void handleCanExecuteChanged(object? sender, EventArgs e) => didCanExecuteChangeFire = true;
            bool commandCanExecute(object? parameter) => canCommandExecute;

            Assert.False(command.CanExecute(null));

            //Act
            canCommandExecute = true;

            //Assert
            Assert.True(command.CanExecute(null));
            Assert.False(didCanExecuteChangeFire);

            //Act
            command.RaiseCanExecuteChanged();

            //Assert
            Assert.True(didCanExecuteChangeFire);
            Assert.True(command.CanExecute(null));
        }
        #endregion
        #region GA - Thread Tests <T>
        [Theory]
        [InlineData("Hello")]
        public void ExecuteAsyncT_InForeground_RunsOnNewThread(string parameter)
        {
            //Arrange
            int callingThreadId = -1, executingThreadId = -1;

            Task MockTask(string text)
            {
                executingThreadId = Thread.CurrentThread.ManagedThreadId;
                return Task.Delay(Delay);
            }

            ICommand command = new AsyncCommand<string>(MockTask);

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

            Task MockTask(string text)
            {
                isTpThread_Executing = Thread.CurrentThread.IsThreadPoolThread;
                return Task.Delay(Delay);
            }

            ICommand command = new AsyncCommand<string>(MockTask);

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

            Task MockTask(string text)
            {
                taskThreadId = Thread.CurrentThread.ManagedThreadId;
                return Task.Delay(Delay);
            }

            ICommand command = new AsyncCommand<string>(MockTask);


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

            Task MockTask(string text)
            {
                isTpThread_Executing = Thread.CurrentThread.IsThreadPoolThread;
                return Task.Delay(Delay);
            }

            ICommand command = new AsyncCommand<string>(MockTask);

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

            Task MockTask(string text)
            {
                executingThreadId = Thread.CurrentThread.ManagedThreadId;
                return Task.Delay(Delay);
            }

            ICommand command = new AsyncCommand<string>(MockTask, null, null, false);

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
            Task MockTask(string text)
            {
                isExecutingThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread;
                return Task.Delay(Delay);
            }

            var thread = new Thread(new ThreadStart(() =>
            {
                ICommand command = new AsyncCommand<string>(MockTask, null, null, false);

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

            Task MockTask(string text)
            {
                taskThreadId = Thread.CurrentThread.ManagedThreadId;
                return Task.Delay(Delay);
            }
            ICommand command = new AsyncCommand<string>(MockTask,null,null,false);


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

            Task MockTask(string text)
            {
                isTpThread_Executing = Thread.CurrentThread.IsThreadPoolThread;
                return Task.Delay(Delay);
            }

            ICommand command = new AsyncCommand<string>(MockTask,null,null,false);

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

            Task MockTask()
            {
                executingThreadId = Thread.CurrentThread.ManagedThreadId;
                return Task.Delay(Delay);
            }

            ICommand command = new AsyncCommand(MockTask);

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

            Task MockTask()
            {
                isTpThread_Executing = Thread.CurrentThread.IsThreadPoolThread;
                return Task.Delay(Delay);
            }

            ICommand command = new AsyncCommand(MockTask);

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

            Task MockTask()
            {
                taskThreadId = Thread.CurrentThread.ManagedThreadId;
                return Task.Delay(Delay);
            }

            ICommand command = new AsyncCommand(MockTask);


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

            Task MockTask()
            {
                isTpThread_Executing = Thread.CurrentThread.IsThreadPoolThread;
                return Task.Delay(Delay);
            }

            ICommand command = new AsyncCommand(MockTask);

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

            Task MockTask()
            {
                executingThreadId = Thread.CurrentThread.ManagedThreadId;
                return Task.Delay(Delay);
            }

            ICommand command = new AsyncCommand(MockTask, null, null, false);

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
            Task MockTask()
            {
                isExecutingThreadPoolThread = Thread.CurrentThread.IsThreadPoolThread;
                return Task.Delay(Delay);
            }

            var thread = new Thread(new ThreadStart(() =>
            {
                ICommand command = new AsyncCommand(MockTask, null, null, false);

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

            Task MockTask()
            {
                taskThreadId = Thread.CurrentThread.ManagedThreadId;
                return Task.Delay(Delay);
            }
            ICommand command = new AsyncCommand(MockTask, null, null, false);


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

            Task MockTask()
            {
                isTpThread_Executing = Thread.CurrentThread.IsThreadPoolThread;
                return Task.Delay(Delay);
            }

            ICommand command = new AsyncCommand(MockTask, null, null, false);

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