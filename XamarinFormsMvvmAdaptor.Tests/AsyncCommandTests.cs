using System;
using Xunit;
using XamarinFormsMvvmAdaptor.Helpers;
using System.Threading.Tasks;
using System.Threading;

namespace XamarinFormsMvvmAdaptor.Tests
{
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
        #region GA - Thread Tests
        [Theory]
        [InlineData("Hello")]
        [InlineData(default)]
        public async Task ExecuteAsync_StringParameter_RunsOnCurrentThread(string parameter)
        {
            //Arrange
            int callingThreadId = -1, taskThreadId = -1;

            Task MockTask(string text)
            {
                taskThreadId = Thread.CurrentThread.ManagedThreadId;
                return Task.Delay(Delay);
            }
            AsyncCommand<string> command = new AsyncCommand<string>(MockTask);
            callingThreadId = Thread.CurrentThread.ManagedThreadId;

            //Act
            await command.ExecuteAsync(parameter);

            //Assert
            Assert.Equal(callingThreadId, taskThreadId);
        }

        [Theory]
        [InlineData("Hello")]
        [InlineData(default)]
        public async Task ExecuteAsync_StringParameter_DoesntRunOnTaskPool(string parameter)
        {
            Task MockTask(string text)
            {
                Assert.False(Thread.CurrentThread.IsThreadPoolThread);
                return Task.Delay(Delay);
            }
            AsyncCommand<string> command = new AsyncCommand<string>(MockTask);


            Assert.False(Thread.CurrentThread.IsThreadPoolThread);
            await command.ExecuteAsync(parameter);            
        }


        #endregion
    }
}