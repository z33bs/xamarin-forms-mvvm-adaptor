using System;
using Xunit;
using XamarinFormsMvvmAdaptor.Helpers;
using System.Threading.Tasks;
using Moq;
using System.Threading;

namespace XamarinFormsMvvmAdaptor.Tests
{
    [Collection("SafeTests")]
    public class SafeExecutionHelpersTests
    {
        protected const int Delay = 50;
        //protected WeakEventManager TestWeakEventManager { get; } = new WeakEventManager();
        //protected WeakEventManager<string> TestStringWeakEventManager { get; } = new WeakEventManager<string>();

        public class SpecificException : Exception { };
        public class DifferentException : Exception { }

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
        protected bool CanExecuteTrue(object parameter) => true;
        protected bool CanExecuteFalse(object parameter) => false;
        protected bool CanExecuteDynamic(object booleanParameter)
        {
            if (booleanParameter is bool parameter)
                return parameter;

            throw new InvalidCastException();
        }

        #region Setup/TearDown
        private void BeforeEachTest()
        {
            SafeExecutionHelpers.Initialize(false);
            SafeExecutionHelpers.RemoveDefaultExceptionHandler();
        }

        private void AfterEachTest()
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
            Exception exception = null;

            //Act
            NoParameterDelayedNullReferenceExceptionTask().SafeFireAndForget(onException: ex => exception = ex);
            await NoParameterTask();
            await NoParameterTask();

            //Assert
            Assert.NotNull(exception);

            AfterEachTest();
        }

        //Tested under Safe Task
        //[Fact]
        //public void SafeFireAndForget_SetDefaultExceptionHandling_NoParams()
        //{
            //BeforeEachTest();

            ////Arrange
            //Exception? exception = null;
            //SafeExecutionHelpers.SetDefaultExceptionHandler(ex => exception = ex);

            ////Act
            //NoParameterDelayedNullReferenceExceptionTask().SafeFireAndForget();
            //await NoParameterTask();
            //await NoParameterTask();

            ////Assert
            //Assert.NotNull(exception);

            //AfterEachTest();
        //}

         //Tested more directly with Handle
        //[Fact]
        //public async Task SafeFireAndForget_SetDefaultExceptionHandling_WithParams()
        //{
        //    BeforeEachTest();

        //    //Arrange
        //    Exception? exception1 = null;
        //    Exception? exception2 = null;
        //    SafeExecutionHelpers.SetDefaultExceptionHandler(ex => exception1 = ex);

        //    //Act
        //    NoParameterDelayedNullReferenceExceptionTask().SafeFireAndForget(onException: ex => exception2 = ex);
        //    await NoParameterTask();
        //    await NoParameterTask();

        //    //Assert
        //    Assert.NotNull(exception1);
        //    Assert.NotNull(exception2);

        //    AfterEachTest();
        //}

        [Fact]
        public void HandleException_SpecificException_TargetThrowsSpecificException_IsHandledByGivenDelegate()
        {
            bool isHandled = false;
            SafeExecutionHelpers.HandleException<SpecificException>(new SpecificException(), (ex) => isHandled = true);
            Assert.True(isHandled);
        }


        [Fact]
        public void HandleException_SpecificException_TargetThrowsDifferentException_NotHandledByGivenDelegate()
        {
            bool isHandled = false;
            SafeExecutionHelpers.HandleException<SpecificException>(new DifferentException(), (ex) => isHandled = true);
            Assert.False(isHandled);
        }

        #endregion
        #region <T> Tests

        [Fact]
        public async Task SafeFireAndForgetT_HandledException()
        {
            BeforeEachTest();

            //Arrange
            NullReferenceException exception = null;

            //Act
            NoParameterDelayedNullReferenceExceptionTask().SafeFireAndForget<NullReferenceException>(onException: ex => exception = ex);
            await NoParameterTask();
            await NoParameterTask();

            //Assert
            Assert.NotNull(exception);

            AfterEachTest();
        }

        //Tested directly under HandleException
        //[Fact]
        //public async Task SafeFireAndForgetT_SetDefaultExceptionHandling_NoParams()
        //{
        //    BeforeEachTest();

        //    //Arrange
        //    Exception? exception = null;
        //    SafeExecutionHelpers.SetDefaultExceptionHandler(ex => exception = ex);

        //    //Act
        //    NoParameterDelayedNullReferenceExceptionTask().SafeFireAndForget();
        //    await NoParameterTask();
        //    await NoParameterTask();

        //    //Assert
        //    Assert.NotNull(exception);

        //    AfterEachTest();
        //}

            //Tested directly under HandleException.
        //[Fact]
        //public async Task SafeFireAndForgetT_SetDefaultExceptionHandling_WithParams()
        //{
        //    BeforeEachTest();

        //    //Arrange
        //    Exception? exception1 = null;
        //    NullReferenceException? exception2 = null;
        //    SafeExecutionHelpers.SetDefaultExceptionHandler(ex => exception1 = ex);

        //    //Act
        //    NoParameterDelayedNullReferenceExceptionTask().SafeFireAndForget<NullReferenceException>(onException: ex => exception2 = ex);
        //    await NoParameterTask();
        //    await NoParameterTask();

        //    //Assert
        //    Assert.NotNull(exception1);
        //    Assert.NotNull(exception2);

        //    AfterEachTest();
        //}
        #endregion
        #region HandleExceptionVariations
        //todo Do more / redo tests using moq
        #region No DefaultHandler
        [Fact]
        public void HandleException_Exception_ExceptionOnException_Handled()
        {
            SafeExecutionHelpers.Initialize();
            SafeExecutionHelpers.RemoveDefaultExceptionHandler();

            var exception = new Exception();
            var handler = new Mock<Action<Exception>>();
            
            SafeExecutionHelpers.HandleException(exception, handler.Object);
            handler.Verify(h => h.Invoke(exception));
        }

        [Fact]
        public void HandleException_SpecificException_SpecificExceptionOnException_Handled()
        {
            SafeExecutionHelpers.Initialize();
            SafeExecutionHelpers.RemoveDefaultExceptionHandler();

            var exception = new SpecificException();
            var handler = new Mock<Action<SpecificException>>();

            SafeExecutionHelpers.HandleException(exception, handler.Object);
            handler.Verify(h => h.Invoke(exception));
        }

        [Fact]
        public void HandleException_SpecificException_ExceptionOnException_Handled()
        {
            SafeExecutionHelpers.Initialize();
            SafeExecutionHelpers.RemoveDefaultExceptionHandler();



            var exception = new SpecificException();
            var handler = new Mock<Action<Exception>>();

            SafeExecutionHelpers.HandleException(exception, handler.Object);
            handler.Verify(h => h.Invoke(exception));
        }

        [Fact]
        public void HandleException_Exception_SpecificExceptionOnException_NotHandled()
        {
            SafeExecutionHelpers.Initialize();
            SafeExecutionHelpers.RemoveDefaultExceptionHandler();

            var exception = new Exception();
            var handler = new Mock<Action<SpecificException>>();

            SafeExecutionHelpers.HandleException(exception, handler.Object);
            Assert.Empty(handler.Invocations);
        }
        #endregion
        #region Test When DefaultHandler fires
        [Fact]
        public void HandleException_DefaultHandlerSet_Exception_ExceptionOnException_DefaultNotInvoked()
        {
            SafeExecutionHelpers.Initialize();
            SafeExecutionHelpers.RemoveDefaultExceptionHandler();

            var defaultHandler = new Mock<Action<Exception>>();
            SafeExecutionHelpers.SetDefaultExceptionHandler(defaultHandler.Object);

            var exception = new Exception();

            SafeExecutionHelpers.HandleException(exception, new Mock<Action<Exception>>().Object);
            Assert.Empty(defaultHandler.Invocations);
        }

        [Fact]
        public void HandleException_DefaultHandlerSet_SpecificException_SpecificExceptionOnException_DefaultNotInvoked()
        {
            SafeExecutionHelpers.Initialize();
            SafeExecutionHelpers.RemoveDefaultExceptionHandler();

            var defaultHandler = new Mock<Action<Exception>>();
            SafeExecutionHelpers.SetDefaultExceptionHandler(defaultHandler.Object);

            var exception = new SpecificException();
            var handler = new Mock<Action<SpecificException>>();

            SafeExecutionHelpers.HandleException(exception, handler.Object);
            Assert.Empty(defaultHandler.Invocations);
        }

        [Fact]
        public void HandleException_DefaultHandlerSet_SpecificException_ExceptionOnException__DefaultNotInvoked()
        {
            SafeExecutionHelpers.Initialize();
            SafeExecutionHelpers.RemoveDefaultExceptionHandler();

            var defaultHandler = new Mock<Action<Exception>>();
            SafeExecutionHelpers.SetDefaultExceptionHandler(defaultHandler.Object);

            var exception = new SpecificException();
            var handler = new Mock<Action<Exception>>();

            SafeExecutionHelpers.HandleException(exception, handler.Object);
            Assert.Empty(defaultHandler.Invocations);
        }

        [Fact]
        public void HandleException_DefaultHandlerSet_Exception_SpecificExceptionOnException__DefaultInvoked()
        {
            SafeExecutionHelpers.Initialize();
            SafeExecutionHelpers.RemoveDefaultExceptionHandler();

            var defaultHandler = new Mock<Action<Exception>>();
            SafeExecutionHelpers.SetDefaultExceptionHandler(defaultHandler.Object);

            var exception = new Exception();
            var handler = new Mock<Action<SpecificException>>();

            SafeExecutionHelpers.HandleException(exception, handler.Object);
            Assert.NotEmpty(defaultHandler.Invocations);
        }
        #endregion
        #endregion

        [Fact]
        public void HandleException_ShouldRethrow_Throws()
        {
            Xamarin.Forms.Mocks.MockForms.Init(); //For Device.BeginInvokeOnMainThread
            SafeExecutionHelpers.Initialize(shouldAlwaysRethrowException: true);
            SafeExecutionHelpers.RemoveDefaultExceptionHandler();

            var exception = new NullReferenceException();
            var handler = new Mock<Action<Exception>>();

            Assert.Throws<NullReferenceException>(()=>SafeExecutionHelpers.HandleException(exception, handler.Object));
            handler.Verify(h => h.Invoke(exception));
        }

    }
}
