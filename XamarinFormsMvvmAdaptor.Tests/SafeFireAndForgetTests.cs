using System;
using Xunit;
using XamarinFormsMvvmAdaptor.Helpers;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.Tests
{
    public class Tests_SafeFireAndForget
    {
        protected const int Delay = 500;
        //protected WeakEventManager TestWeakEventManager { get; } = new WeakEventManager();
        //protected WeakEventManager<string> TestStringWeakEventManager { get; } = new WeakEventManager<string>();

        protected Task NoParameterTask() => Task.Delay(Delay);
        protected Task IntParameterTask(int delay) => Task.Delay(delay);
        protected Task StringParameterTask(string text) => Task.Delay(Delay);
        protected Task NoParameterImmediateNullReferenceExceptionTask() => throw new NullReferenceException();
        protected Task ParameterImmediateNullReferenceExceptionTask(int delay) => throw new NullReferenceException();
        protected async Task NoParameterDelayedNullReferenceExceptionTask()
        {
            await Task.Delay(Delay);
            throw new NullReferenceException();
        }

        protected async Task IntParameterDelayedNullReferenceExceptionTask(int delay)
        {
            await Task.Delay(delay);
            throw new NullReferenceException();
        }
        protected bool CanExecuteTrue(object? parameter) => true;
        protected bool CanExecuteFalse(object? parameter) => false;
        protected bool CanExecuteDynamic(object? booleanParameter)
        {
            if (booleanParameter is bool parameter)
                return parameter;

            throw new InvalidCastException();
        }

        #region Setup/TearDown
        public void BeforeEachTest()
        {
            SafeFireAndForgetExtensions.Initialize(false);
            SafeFireAndForgetExtensions.RemoveDefaultExceptionHandling();
        }

        public void AfterEachTest()
        {
            SafeFireAndForgetExtensions.Initialize(false);
            SafeFireAndForgetExtensions.RemoveDefaultExceptionHandling();
        }
        #endregion

        #region Tests
        [Fact]
        public async Task SafeFireAndForget_HandledException()
        {
            BeforeEachTest();

            //Arrange
            Exception? exception = null;

            //Act
            NoParameterDelayedNullReferenceExceptionTask().SafeFireAndForget(onException: ex => exception = ex);
            await NoParameterTask();
            await NoParameterTask();

            //Assert
            Assert.NotNull(exception);

            AfterEachTest();
        }

        [Fact]
        public async Task SafeFireAndForget_SetDefaultExceptionHandling_NoParams()
        {
            BeforeEachTest();

            //Arrange
            Exception? exception = null;
            SafeFireAndForgetExtensions.SetDefaultExceptionHandling(ex => exception = ex);

            //Act
            NoParameterDelayedNullReferenceExceptionTask().SafeFireAndForget();
            await NoParameterTask();
            await NoParameterTask();

            //Assert
            Assert.NotNull(exception);

            AfterEachTest();
        }

        [Fact]
        public async Task SafeFireAndForget_SetDefaultExceptionHandling_WithParams()
        {
            BeforeEachTest();

            //Arrange
            Exception? exception1 = null;
            Exception? exception2 = null;
            SafeFireAndForgetExtensions.SetDefaultExceptionHandling(ex => exception1 = ex);

            //Act
            NoParameterDelayedNullReferenceExceptionTask().SafeFireAndForget(onException: ex => exception2 = ex);
            await NoParameterTask();
            await NoParameterTask();

            //Assert
            Assert.NotNull(exception1);
            Assert.NotNull(exception2);

            AfterEachTest();
        }
        #endregion
        #region <T> Tests

        [Fact]
        public async Task SafeFireAndForgetT_HandledException()
        {
            BeforeEachTest();

            //Arrange
            NullReferenceException? exception = null;

            //Act
            NoParameterDelayedNullReferenceExceptionTask().SafeFireAndForget<NullReferenceException>(onException: ex => exception = ex);
            await NoParameterTask();
            await NoParameterTask();

            //Assert
            Assert.NotNull(exception);

            AfterEachTest();
        }

        [Fact]
        public async Task SafeFireAndForgetT_SetDefaultExceptionHandling_NoParams()
        {
            BeforeEachTest();

            //Arrange
            Exception? exception = null;
            SafeFireAndForgetExtensions.SetDefaultExceptionHandling(ex => exception = ex);

            //Act
            NoParameterDelayedNullReferenceExceptionTask().SafeFireAndForget();
            await NoParameterTask();
            await NoParameterTask();

            //Assert
            Assert.NotNull(exception);

            AfterEachTest();
        }

        [Fact]
        public async Task SafeFireAndForgetT_SetDefaultExceptionHandling_WithParams()
        {
            BeforeEachTest();

            //Arrange
            Exception? exception1 = null;
            NullReferenceException? exception2 = null;
            SafeFireAndForgetExtensions.SetDefaultExceptionHandling(ex => exception1 = ex);

            //Act
            NoParameterDelayedNullReferenceExceptionTask().SafeFireAndForget<NullReferenceException>(onException: ex => exception2 = ex);
            await NoParameterTask();
            await NoParameterTask();

            //Assert
            Assert.NotNull(exception1);
            Assert.NotNull(exception2);

            AfterEachTest();
        }
        #endregion
    }
}
