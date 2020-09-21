using System;
using Xunit;
using Moq;
using XamarinFormsMvvmAdaptor.Tests.ViewModels;
using XamarinFormsMvvmAdaptor.Tests.Views;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xamarin.Forms.Mocks; //Needed for Mocking Device.BeginInvokeOnMainThread
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor.Tests
{
    [Collection("SmartDi")]
    public class NavigationServiceTests
    {
        public NavigationServiceTests()
        {
            MockForms.Init();
        }

        [Fact]
        public async Task PushAsync_Always_ExecutesINavigationPushAsync()
        {
            var navigation = new Mock<INavigation>();
            navigation.Setup(o => o.PushAsync(It.IsAny<EmptyPage>(),true)).Verifiable();
            var ns = new NavigationService(navigation.Object);
            await ns.PushAsync<EmptyViewModel>(true);
            navigation.VerifyAll();
        }

        [Fact]
        public async Task PushModelAsync_Always_ExecutesINavigationPushModelAsync()
        {
            var navigation = new Mock<INavigation>();
            navigation.Setup(o => o.PushModalAsync(It.IsAny<EmptyPage>(), true)).Verifiable();
            var ns = new NavigationService(navigation.Object);
            await ns.PushModalAsync<EmptyViewModel>(true);
            navigation.VerifyAll();
        }

        [Fact]
        public async Task PushAsyncWithData_Always_ExecutesINavigationPushAsync()
        {
            var navigation = new Mock<INavigation>();
            navigation.Setup(o => o.PushAsync(It.IsAny<ImplementsBasePage>(), true)).Verifiable();
            var ns = new NavigationService(navigation.Object);
            await ns.PushAsync<ImplementsBaseViewModel>(new object(),true);
            navigation.VerifyAll();
        }

        //todo PushAsync_TargetImplementsIPushed_ExecutesOnViewPushedAsync()
        //todo PushAsyncWithData_TargetImplementsIPushed_ExecutesOnViewPushedAsyncWithData()

        [Fact]
        public async Task PushModelAsyncWithData_Always_ExecutesINavigationPushModelAsync()
        {
            var navigation = new Mock<INavigation>();
            navigation.Setup(o => o.PushModalAsync(It.IsAny<ImplementsBasePage>(), true)).Verifiable();
            var ns = new NavigationService(navigation.Object);
            await ns.PushModalAsync<ImplementsBaseViewModel>(new object(),true);
            navigation.VerifyAll();
        }

        //todo PushModalAsync_TargetImplementsIPushed_ExecutesOnViewPushedAsync()
        //todo PushModalAsyncWithData_TargetImplementsIPushed_ExecutesOnViewPushedAsyncWithData()

        [Fact]
        public async Task PopAsync_Always_ExecutesINavigationPopAsync()
        {
            var page = new Mock<Page>();
            page.Object.BindingContext = new Mock<EmptyViewModel>().Object;
            var navigation = new Mock<INavigation>();
            navigation.Setup(o => o.PopAsync(true)).Verifiable();
            navigation.Setup(o => o.NavigationStack).Returns(
                new List<Page> { null, page.Object });
            var ns = new NavigationService(navigation.Object);
            await ns.PopAsync();
            navigation.VerifyAll();
        }

        [Fact]
        public async Task PopAsync_INavigationException_ThrowsException()
        {
            var page = new Mock<Page>();
            page.Object.BindingContext = new Mock<EmptyViewModel>().Object;
            var navigation = new Mock<INavigation>();
            //Typical exceptions are InvalidOperationException and ArgumentOutOfRangeException
            navigation.Setup(o => o.PopAsync(true)).Throws<InvalidOperationException>();
            navigation.Setup(o => o.NavigationStack).Returns(
                new List<Page> { null, page.Object });
            var ns = new NavigationService(navigation.Object);
            await Assert.ThrowsAsync<InvalidOperationException>(() => ns.PopAsync());
        }

        //todo Test exceptions for all navigation operations

        [Fact]
        public async Task PopModalAsync_Always_ExecutesINavigationPopModalAsync()
        {
            var page = new Mock<Page>();
            page.Object.BindingContext = new Mock<EmptyViewModel>().Object;
            var navigation = new Mock<INavigation>();
            navigation.Setup(o => o.PopModalAsync(true)).Verifiable();
            navigation.Setup(o => o.ModalStack).Returns(
                new List<Page> { page.Object });
            var ns = new NavigationService(navigation.Object);
            await ns.PopModalAsync();
            navigation.VerifyAll();
        }

        [Fact]
        public async Task PopAsync_TargetImplementsIRemoved_ExecutesOnRemovedAsync()
        {
            var vm = new Mock<IOnViewRemoved>();
            vm.Setup(o => o.OnViewRemovedAsync()).Verifiable();

            var page = new Mock<Page>();
            page.Object.BindingContext = vm.Object;
            var navigation = new Mock<INavigation>();

            navigation.Setup(o => o.NavigationStack).Returns(
                new List<Page> { null, page.Object });
            var ns = new NavigationService(navigation.Object);
            await ns.PopAsync();
            vm.VerifyAll();
        }

        [Fact]
        public async Task PopModalAsync_TargetImplementsIRemoved_ExecutesOnRemovedAsync()
        {
            var vm = new Mock<IOnViewRemoved>();
            vm.Setup(o => o.OnViewRemovedAsync()).Verifiable();

            var page = new Mock<Page>();
            page.Object.BindingContext = vm.Object;
            var navigation = new Mock<INavigation>();

            navigation.Setup(o => o.ModalStack).Returns(
                new List<Page> { page.Object });
            var ns = new NavigationService(navigation.Object);
            await ns.PopModalAsync();
            vm.VerifyAll();
        }

        [Fact]
        public async Task PopModalAsync_EmptyModalStack_ThrowsInvalidOperationException()
        {
            var navigation = new Mock<INavigation>();

            navigation.Setup(o => o.ModalStack).Returns(
                new List<Page>());
            var ns = new NavigationService(navigation.Object);
            ;
            await Assert.ThrowsAsync<InvalidOperationException>(() => ns.PopModalAsync());
        }

        [Fact]
        public async Task RemovePageFor_Always_ExecutesINavigationRemovePage()
        {
            var page = new EmptyPage();
            var navigation = new Mock<INavigation>();
            navigation.Setup(o => o.RemovePage(page)).Verifiable();
            navigation.Setup(o => o.NavigationStack).Returns(
                new List<Page> { null, page });
            var ns = new NavigationService(navigation.Object);
            await ns.RemovePageFor<EmptyViewModel>();
            navigation.VerifyAll();
        }

        [Fact]
        public async Task RemovePageFor_TargetImplementsIRemoved_ExecutesOnRemovedAsync()
        {
            var vm = new Mock<IOnViewRemoved>();
            vm.Setup(o => o.OnViewRemovedAsync()).Verifiable();

            //Can't Mock page because will compare types
            //and Page != Mock<Page>
            var page = new EmptyPage
            {
                BindingContext = vm.Object
            }; 
            
            var navigation = new Mock<INavigation>();


            navigation.Setup(o => o.NavigationStack).Returns(
                new List<Page> { null, page, new Mock<Page>().Object });
            var ns = new NavigationService(navigation.Object);
            await ns.RemovePageFor<EmptyViewModel>();

            vm.VerifyAll();
        }

        [Fact]
        public async Task RemovePreviousPageFromMainStack_Always_ExecutesINavigationRemovePage()
        {
            var page = new EmptyPage();
            var navigation = new Mock<INavigation>();
            navigation.Setup(o => o.RemovePage(page)).Verifiable();
            navigation.Setup(o => o.NavigationStack).Returns(
                new List<Page> { null, page, new Page() });
            var ns = new NavigationService(navigation.Object);
            await ns.RemovePreviousPageFromMainStack();
            navigation.VerifyAll();
        }

        [Fact]
        public async Task RemovePreviousPageFromMainStack_TargetImplementsIRemoved_ExecutesOnRemovedAsync()
        {
            var vm = new Mock<IOnViewRemoved>();
            vm.Setup(o => o.OnViewRemovedAsync()).Verifiable();

            var page = new Mock<Page>();
            page.Object.BindingContext = vm.Object;
            var navigation = new Mock<INavigation>();

            navigation.Setup(o => o.NavigationStack).Returns(
                new List<Page> { null, page.Object, new Mock<Page>().Object });
            var ns = new NavigationService(navigation.Object);
            await ns.RemovePreviousPageFromMainStack();

            vm.VerifyAll();
        }

        [Fact]
        public async Task PopToRootAsync_Always_ExecutesPopAsyncForEveryPage()
        {
            var navigation = new Mock<INavigation>();

            int times = 0;

            var fakestack = new List<Page> { null, new Page(), new Page() };
            navigation.SetupGet(o => o.NavigationStack).Returns(fakestack);
            navigation.Setup(o => o.PopAsync(true)).Callback(
                () =>
                {
                    fakestack.RemoveAt(fakestack.Count - 1);
                    times++;
                });

            var ns = new NavigationService(navigation.Object);
            await ns.PopToRootAsync();

            Assert.Equal(2, times);
        }

        [Fact]
        public async Task PopToRootAsync_TargetImplementsIRemoved_ExecutesOnRemovedAsync()
        {
            var vm = new Mock<IOnViewRemoved>();
            vm.Setup(o => o.OnViewRemovedAsync()).Verifiable();
            var page = new Mock<Page>();
            page.Object.BindingContext = vm.Object;

            var navigation = new Mock<INavigation>();

            var fakestack = new List<Page> { null, page.Object, new Page() };
            navigation.SetupGet(o => o.NavigationStack).Returns(fakestack);
            navigation.Setup(o => o.PopAsync(true)).Callback(() => fakestack.RemoveAt(fakestack.Count - 1));

            var ns = new NavigationService(navigation.Object);
            await ns.PopToRootAsync();
            vm.VerifyAll();
        }


    }
}
