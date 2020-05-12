using System;
using System.Threading.Tasks;
using Xunit;
using XamarinFormsMvvmAdaptor.Helpers;
using System.Threading;
using Moq;

namespace XamarinFormsMvvmAdaptor.Tests
{
    [Collection("SafeTests")]
    public class SafeTaskTests
    {
        class SpecificException : Exception { }

        [Fact]
        public void SafeContinueWith_NothingSet_DoesNothing()
        {
            SpecificException specificException = new SpecificException();

            SafeExecutionHelpers.Initialize();
            var mockHelpers = new Mock<ISafeExecutionHelpers>();
            SafeExecutionHelpers.SetImplementation(mockHelpers.Object);

            var dts = new DeterministicTaskScheduler(shouldThrowExceptions: false);
            var sut = new SafeTask();

            sut.SafeContinueWith<Exception>(
                Task.Factory.StartNew(
                    () => throw specificException,
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    dts),
                null,
                dts);

            dts.RunTasksUntilIdle();

            Assert.Contains(specificException, dts.Exceptions);
            mockHelpers.Verify(h => h.HandleException<Exception>(specificException,null), Times.Never);
            
            SafeExecutionHelpers.RevertToDefaultImplementation();
        }

        [Fact]
        public void SafeContinueWith_OnExceptionExceptionSet_HandlesException()
        {
            SpecificException specificException = new SpecificException();
            Action<Exception> onException = new Mock<Action<Exception>>().Object;

            SafeExecutionHelpers.Initialize();
            var mockHelpers = new Mock<ISafeExecutionHelpers>();
            SafeExecutionHelpers.SetImplementation(mockHelpers.Object);

            var dts = new DeterministicTaskScheduler(shouldThrowExceptions: false);
            var sut = new SafeTask();

            sut.SafeContinueWith<Exception>(
                Task.Factory.StartNew(
                    ()=> throw specificException,
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    dts),
                onException, //The crux
                dts);

            dts.RunTasksUntilIdle();

            Assert.Contains(specificException, dts.Exceptions);
            mockHelpers.Verify(h => h.HandleException(specificException, onException));

            SafeExecutionHelpers.RevertToDefaultImplementation();
        }

        [Fact]
        public void SafeContinueWith_DefaultExceptionHandlerSet_HandlesException()
        {
            SpecificException specificException = new SpecificException();
            Action<Exception> defaultExceptionHandler = new Mock<Action<Exception>>().Object;

            SafeExecutionHelpers.Initialize();
            var mockHelpers = new Mock<ISafeExecutionHelpers>();

            //Crux - DefaultHandler returns non-null delegate
            mockHelpers.SetupGet(h => h.DefaultExceptionHandler).Returns(defaultExceptionHandler);

            SafeExecutionHelpers.SetImplementation(mockHelpers.Object);

            var dts = new DeterministicTaskScheduler(shouldThrowExceptions: false);
            var sut = new SafeTask();

            sut.SafeContinueWith<Exception>(
                Task.Factory.StartNew(
                    () => throw specificException,
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    dts),
                null,
                dts);

            dts.RunTasksUntilIdle();

            Assert.Contains(specificException, dts.Exceptions);
            mockHelpers.Verify(h => h.HandleException<Exception>(specificException,null));

            SafeExecutionHelpers.RevertToDefaultImplementation();
        }


        //Tested more directly
        //[Fact]
        //public void SafeContinueWith_Exception_TargetThrowsException_IsHandled()
        //{
        //    bool isHandled = false;
        //    SpecificException exception = new SpecificException();

        //    Task ThrowingAsync()
        //    { throw exception; }

        //    var dts = new DeterministicTaskScheduler(shouldThrowExceptions: false);
        //    var sut = new SafeTask();

        //    sut.SafeContinueWith<Exception>(
        //        Task.Factory.StartNew(
        //            ThrowingAsync,
        //            CancellationToken.None,
        //            TaskCreationOptions.None,
        //            dts),
        //        (ex) => isHandled = true,dts);

        //    dts.RunTasksUntilIdle();

        //    Assert.Contains(exception, dts.Exceptions);
        //    Assert.True(isHandled);
        //}

        //Tested more directly
        //[Fact]
        //public void SafeContinueWith_SpecificException_TargetThrowsSpecificException_IsHandled()
        //{
        //    bool isHandled = false;
        //    SpecificException exception = new SpecificException();

        //    Task ThrowingAsync()
        //    {  throw exception; }

        //    var dts = new DeterministicTaskScheduler(shouldThrowExceptions: false);
        //    Task.Factory.StartNew(
        //    ThrowingAsync, CancellationToken.None, TaskCreationOptions.None, dts).SafeContinueWith<SpecificException>((ex) => isHandled = true, dts);
        //    dts.RunTasksUntilIdle();

        //    Assert.Contains(exception, dts.Exceptions);
        //    Assert.True(isHandled);

        //}

        //Tested more directly
        //[Fact]
        //public void SafeContinueWith_Exception_TargetThrowsSpecificException_IsHandled()
        //{
        //    SafeExecutionHelpers.Initialize();
        //    SafeExecutionHelpers.RemoveDefaultExceptionHandler();

        //    bool isHandled = false;
        //    SpecificException exception = new SpecificException();

        //    Task ThrowingAsync()
        //    { throw exception; }

        //    var dts = new DeterministicTaskScheduler(shouldThrowExceptions: false);
        //    Task.Factory.StartNew(
        //    ThrowingAsync, CancellationToken.None, TaskCreationOptions.None, dts).SafeContinueWith<Exception>((ex) => isHandled = true, dts);
        //    dts.RunTasksUntilIdle();

        //    Assert.Contains(exception, dts.Exceptions);
        //    Assert.True(isHandled);

        //}


        //Tested more directly
        //[Fact]
        //public void SafeContinueWith_SpecificException_TargetThrowsDifferentException_NotHandled()
        //{
        //    bool isHandled = false;
        //    DifferentException exception = new DifferentException();

        //    Task ThrowingAsync()
        //    { throw exception; }

        //    var dts = new DeterministicTaskScheduler(shouldThrowExceptions: false);
        //    Task.Factory.StartNew(
        //    ThrowingAsync, CancellationToken.None, TaskCreationOptions.None, dts).SafeContinueWith<SpecificException>((ex) => isHandled = true, dts);
        //    dts.RunTasksUntilIdle();

        //    Assert.Contains(exception, dts.Exceptions);
        //    Assert.False(isHandled);

        //}
    }
}
