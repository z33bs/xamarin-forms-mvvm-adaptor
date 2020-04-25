﻿using System;
using Xunit;
using XamarinFormsMvvmAdaptor.Helpers;
using System.Threading.Tasks;

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
    }
}