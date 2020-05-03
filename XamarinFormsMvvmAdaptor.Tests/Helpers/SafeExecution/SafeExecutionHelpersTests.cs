using System;
using Xunit;
using XamarinFormsMvvmAdaptor.Helpers;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.Tests
{
    [Collection("SafeTests")]
    public class SafeExecutionHelpersTests
    {
        protected const int Delay = 50;
        //protected WeakEventManager TestWeakEventManager { get; } = new WeakEventManager();
        //protected WeakEventManager<string> TestStringWeakEventManager { get; } = new WeakEventManager<string>();

        class SpecificException : Exception { };

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
            SafeExecutionHelpers.Initialize(false);
            SafeExecutionHelpers.RemoveDefaultExceptionHandler();
        }

        public void AfterEachTest()
        {
            SafeExecutionHelpers.Initialize(false);
            SafeExecutionHelpers.RemoveDefaultExceptionHandler();
        }
        #endregion

        #region Tests
        [Fact]
        public void SafeAction_Runs()
        {
            BeforeEachTest();
            Action EmptyAction = ()=>{ };
            EmptyAction.SafeInvoke((Exception ex) => { });
            AfterEachTest();
        }

        [Fact]
        public void SafeActionTExceptionSpecificException_ThrowsDifferentException_NotHandled()
        {
            bool wasHandled = false;
            BeforeEachTest();
            Action ActionThatThrowsException = () => { throw new Exception(); };
            void OnVanillaException(Exception ex) { wasHandled = true; }

            Assert.Throws<Exception>(()=>ActionThatThrowsException.SafeInvoke<SpecificException>(OnVanillaException));
            Assert.False(wasHandled);
            AfterEachTest();
        }

        [Fact]
        public void SafeAction_Throws_ExceptionHandled()
        {
            
            bool wasHandled = false;
            BeforeEachTest();
            Action ActionThatThrowsException = () => { throw new Exception(); };
            void OnVanillaException(Exception ex) { wasHandled = true; }
            ActionThatThrowsException.SafeInvoke(OnVanillaException);
            Assert.True(wasHandled);
            AfterEachTest();
        }


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
            SafeExecutionHelpers.SetDefaultExceptionHandler(ex => exception = ex);

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
            SafeExecutionHelpers.SetDefaultExceptionHandler(ex => exception1 = ex);

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
            SafeExecutionHelpers.SetDefaultExceptionHandler(ex => exception = ex);

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
            SafeExecutionHelpers.SetDefaultExceptionHandler(ex => exception1 = ex);

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
