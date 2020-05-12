using System;
using Xunit;
using Moq;
using Xamarin.Forms;
using XamarinFormsMvvmAdaptor.Tests.Views;
using XamarinFormsMvvmAdaptor.Tests.ViewModels;

namespace XamarinFormsMvvmAdaptor.Tests
{
    public class ViewModelLocatorTests
    {
        public ViewModelLocatorTests()
        {
        }

        [Fact]
        public void AutoWireViewModel_Always_CallsIIocResolve()
        {
            var mockIoc = new Mock<IIoc>();
            mockIoc.Setup(m => m.Resolve(typeof(EmptyViewModel))).Returns(new EmptyViewModel()).Verifiable();
            var page = new EmptyPage();
            ViewModelLocator.ContainerImplementation = mockIoc.Object;
            ViewModelLocator.AutoWireViewModel(page);
            mockIoc.VerifyAll();
        }


        [Fact]
        public void AutoWireViewModel_ViewModelExists_SetsBindingContextToViewModel()
        {
            var mockIoc = new Mock<IIoc>();
            mockIoc.Setup(m => m.Resolve(typeof(EmptyViewModel))).Returns(new EmptyViewModel());
            var page = new EmptyPage();
            ViewModelLocator.ContainerImplementation = mockIoc.Object;
            ViewModelLocator.AutoWireViewModel(page);
            Assert.IsType<EmptyViewModel>(page.BindingContext);
        }

        //todo public void AutoWireViewModel_VmIAppearing_WiresOnViewAppearingEvent()
        //todo public void AutoWireViewModel_VmIDisappearing_WiresOnViewAppearingEvent()

        //todo public void AutoWireViewModel_VmTypeIsNull_ThrowsViewModelBindingException()
        //todo public void AutoWireViewModel_VmIsNull_ThrowsViewModelBindingException()
    }
}
