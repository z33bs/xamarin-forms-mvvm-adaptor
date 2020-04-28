﻿using System;
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
        const int Delay = 50;
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
        public void AsyncCommand_ExecuteAsync_IntParameter_Test(int parameter)
        {
            //Arrange
            var dts = new DeterministicTaskScheduler();

            ICommand command = new AsyncCommand<int>(IntParameterTask,dts,null,null);

            //Act
            command.Execute(parameter);
            dts.RunTasksUntilIdle();

            //Assert

        }

        [Theory]
        [InlineData("Hello")]
        [InlineData(default)]
        public void AsyncCommand_ExecuteAsync_StringParameter_Test(string parameter)
        {
            //Arrange
            var dts = new DeterministicTaskScheduler();
            ICommand command = new AsyncCommand<string>(StringParameterTask,dts,null,null);

            //Act
             command.Execute(parameter);
            dts.RunTasksUntilIdle();

            //Assert

        }

        [Fact]
        public void CanExecuteT_NullParameterWithNonNullableValueType_False()
        {
            //Arrange
            AsyncCommand<int> command = new AsyncCommand<int>(IntParameterTask, o => CanExecuteTrue(o));

            //Act

            //Assert

            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void AsyncCommand_Parameter_CanExecuteFalse_Test()
        {
            //Arrange
            AsyncCommand<int> command = new AsyncCommand<int>(IntParameterTask, o=>CanExecuteFalse(o));

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
        #region GA - Thread Tests on AsyncCommand<T>
        [Theory]
        [InlineData("Hello")]
        public void ExecuteAsyncT_RunsOnNewThread(string parameter)
        {
            ICommand command = new AsyncCommand<string>(MockTask);
            bool isExecuting = true;
            Thread callingThread = null, executingThread = null;

            async Task MockTask(string text)
            {                
                await Task.Delay(Delay);
                executingThread = Thread.CurrentThread;
                isExecuting = false;
            }

            var thread = new Thread(new ThreadStart(() =>
            {
                callingThread = Thread.CurrentThread;
                command.Execute(parameter);
            }));

            thread.Start();
            while (isExecuting)
                Thread.Sleep(Delay/25);

            //Assert
            Assert.NotEqual(callingThread.ManagedThreadId, executingThread.ManagedThreadId);
        }

        [Theory]
        [InlineData("Hello")]
        public void ExecuteAsyncT_RunsOnTaskPool(string parameter)
        {
            ICommand command = new AsyncCommand<string>(MockTask);

            bool isExecuting = true;
            Thread callingThread = null, executingThread = null;

            async Task MockTask(string text)
            {
                await Task.Delay(Delay);
                executingThread = Thread.CurrentThread;
                isExecuting = false;
            }

            var thread = new Thread(new ThreadStart(() =>
            {
                callingThread = Thread.CurrentThread;
                command.Execute(parameter);
            }));

            thread.Start();
            while (isExecuting)
                Thread.Sleep(Delay/25);

            Assert.False(callingThread.IsThreadPoolThread);
            Assert.True(executingThread.IsThreadPoolThread);
        }


        [Theory]
        [InlineData("Hello")]
        public void ExecuteAsyncT_MustRunOnCurrentSyncContextTrue_RunsOnCurrentThread(string parameter)
        {
            ICommand command = new AsyncCommand<string>(MockTask, null, null, true);

            bool isExecuting = true;
            Thread callingThread = null, executingThread = null;

            async Task MockTask(string text)
            {
                executingThread = Thread.CurrentThread;
                await Task.Delay(Delay);                
                isExecuting = false;
            }

            var thread = new Thread(new ThreadStart(() =>
            {
                callingThread = Thread.CurrentThread;
                command.Execute(parameter);
            }));

            thread.Start();
            while (isExecuting)
                Thread.Sleep(Delay / 25);

            Assert.Equal(callingThread.ManagedThreadId, executingThread.ManagedThreadId);
        }

        [Theory]
        [InlineData("Hello")]
        public void ExecuteAsyncT_MustRunOnCurrentSyncContextTrue_NotRunOnTaskPool(string parameter)
        {
            ICommand command = new AsyncCommand<string>(MockTask, null, null, true);

            bool isExecuting = true;
            Thread callingThread = null, executingThread = null;

            async Task MockTask(string text)
            {
                executingThread = Thread.CurrentThread;
                await Task.Delay(Delay);                
                isExecuting = false;
            }

            var thread = new Thread(new ThreadStart(() =>
            {
                callingThread = Thread.CurrentThread;
                command.Execute(parameter);
            }));

            thread.Start();
            while (isExecuting)
                Thread.Sleep(Delay / 25);

            Assert.False(executingThread.IsThreadPoolThread);
            Assert.False(callingThread.IsThreadPoolThread);
        }

        #endregion
        
        #region GA - Thread Tests on AsyncCommand

        [Fact]
        public void ExecuteAsync_RunsOnNewThread()
        {
            ICommand command = new AsyncCommand(MockTask);

            bool isExecuting = true;
            Thread callingThread = null, executingThread = null;

            async Task MockTask()
            {
                await Task.Delay(Delay);
                executingThread = Thread.CurrentThread;
                isExecuting = false;
            }

            var thread = new Thread(new ThreadStart(() =>
            {
                callingThread = Thread.CurrentThread;
                command.Execute(null);
            }));

            thread.Start();
            while (isExecuting)
                Thread.Sleep(Delay / 25);

            Assert.NotEqual(callingThread.ManagedThreadId, executingThread.ManagedThreadId);
        }
        //Marker
        [Fact]
        public void ExecuteAsync_RunsOnTaskPool()
        {
            ICommand command = new AsyncCommand(MockTask);

            bool isExecuting = true;
            Thread callingThread = null, executingThread = null;

            async Task MockTask()
            {
                await Task.Delay(Delay);
                executingThread = Thread.CurrentThread;
                isExecuting = false;
            }

            var thread = new Thread(new ThreadStart(() =>
            {
                callingThread = Thread.CurrentThread;
                command.Execute(null);
            }));

            thread.Start();
            while (isExecuting)
                Thread.Sleep(Delay / 25);

            Assert.False(callingThread.IsThreadPoolThread);
            Assert.True(executingThread.IsThreadPoolThread);
        }

        [Fact]
        public void ExecuteAsync_MustRunOnCurrentSyncContextTrue_RunsOnCurrentThread()
        {
            ICommand command = new AsyncCommand(MockTask, null, null, true);

            bool isExecuting = true;
            Thread callingThread = null, executingThread = null;

            async Task MockTask()
            {
                executingThread = Thread.CurrentThread;
                await Task.Delay(Delay).ConfigureAwait(true);                
                isExecuting = false;
            }

            var thread = new Thread(new ThreadStart(() =>
            {
                callingThread = Thread.CurrentThread;
                command.Execute(null);
            }));

            thread.Start();
            while (isExecuting)
                Thread.Sleep(Delay / 25);

            Assert.Equal(callingThread.ManagedThreadId, executingThread.ManagedThreadId);
        }

        [Fact]
        public void ExecuteAsync_MustRunOnCurrentSyncContextTrue_NotRunOnTaskPool()
        {
            ICommand command = new AsyncCommand(MockTask, null, null, true);

            bool isExecuting = true;
            Thread callingThread = null, executingThread = null;

            async Task MockTask()
            {
                executingThread = Thread.CurrentThread;
                await Task.Delay(Delay);                
                isExecuting = false;
            }

            var thread = new Thread(new ThreadStart(() =>
            {
                callingThread = Thread.CurrentThread;
                command.Execute(null);
            }));

            thread.Start();
            while (isExecuting)
                Thread.Sleep(Delay / 25);

            Assert.False(executingThread.IsThreadPoolThread);
            Assert.False(callingThread.IsThreadPoolThread);
        }
        #endregion

        #region Monkey Tests
        [Fact]
        public void ExecuteT_CalledTwice_FiresOnceIfBusy()
        {
            int times = 0;
            async Task MockTask(string text)
            {
                await Task.Delay(Delay);
                times++;
            }
            var dts = new DeterministicTaskScheduler(shouldThrowExceptions: false);
            ICommand command = new AsyncCommand<string>(MockTask, dts, null,null) ;

            command.Execute("test");
            command.Execute("test");
            dts.RunTasksUntilIdle();

            Assert.Equal(1, times);
        }

        [Fact]
        public void ExecuteT_CalledTwice_FiresTwiceIfNotBusy()
        {
            int times = 0;
            async Task MockTask(string text)
            {                
                await Task.Delay(Delay);
                times++;
            }
            var dts = new DeterministicTaskScheduler(shouldThrowExceptions: false);
            ICommand command = new AsyncCommand<string>(MockTask,dts,null,null);

            command.Execute("test");
            dts.RunTasksUntilIdle();

            command.Execute("test");
            dts.RunTasksUntilIdle();

            Assert.Equal(2, times);
        }

        [Fact]
        public void ExecuteT_OnExceptionSet_SecondCallAfterException_Executes()
        {
            int times = 0;
            bool isHandled = false;
            async Task MockTask(string text)
            {
                await Task.Delay(Delay);
                if (times++ == 0)
                    throw new Exception(); //Throws only on first try
            }

            var dts = new DeterministicTaskScheduler(shouldThrowExceptions: false);
            ICommand command = new AsyncCommand<string>(MockTask, dts, null, (ex)=> { isHandled = true; });

            command.Execute("test");
            dts.RunTasksUntilIdle();

            command.Execute("test");
            dts.RunTasksUntilIdle();

            Assert.NotEmpty(dts.Exceptions);
            Assert.True(isHandled);
            Assert.Equal(2, times);
        }

        [Fact]
        public void ExecuteT_SecondCallAfterException_Executes()
        {
            int times = 0;
            async Task MockTask(string text)
            {
                await Task.Delay(Delay);
                if (times++ == 0)
                    throw new Exception(); //Throws only on first try
            }

            var dts = new DeterministicTaskScheduler(shouldThrowExceptions: false);
            ICommand command = new AsyncCommand<string>(MockTask, dts, null, null);

            command.Execute("test");
            dts.RunTasksUntilIdle();

            command.Execute("test");
            dts.RunTasksUntilIdle();

            Assert.NotEmpty(dts.Exceptions);
            Assert.Equal(2, times);
        }

        #endregion
    }

}