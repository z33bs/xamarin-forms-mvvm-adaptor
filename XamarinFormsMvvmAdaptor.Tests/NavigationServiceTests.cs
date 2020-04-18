using System;
using Xunit;
using Moq;
using XamarinFormsMvvmAdaptor.Tests.ViewModels;
using XamarinFormsMvvmAdaptor.Tests.Views;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor.Tests
{
    public class NavigationServiceTests
    {
        public NavigationServiceTests()
        {
        }

        [Fact]
        public async Task PushAsync_Always_ExecutesINavigationPushAsync()
        {
            var navigation = new Mock<INavigation>();
            navigation.Setup(o => o.PushAsync(It.IsAny<EmptyPage>(),true)).Verifiable();
            var ns = new NavigationService(navigation.Object);
            var page = await ns.PushAsync<EmptyViewModel>(true);
            navigation.VerifyAll();
        }
    }
}
