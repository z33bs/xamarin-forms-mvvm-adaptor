using NUnit.Framework;
using XamarinFormsMvvmAdaptor.UnitTests.Views;
using XamarinFormsMvvmAdaptor.UnitTests.ViewModels;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.UnitTests
{
    public class Initialization
    {
        INavController navController;

        [OneTimeSetUp]
        public void SetupOnce()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
        }

        [SetUp]
        public void Setup()
        {
            navController = new NavController();
        }

        [Test]
        public void New_NavController_IsInitialized_False()
        {
            Assert.IsFalse(navController.IsInitialized);
        }

        [Test]
        public async Task InitAsync_Page_IsInitialized_True()
        {
            await navController.InitAsync(new TestPage0());
            Assert.IsTrue(navController.IsInitialized);
        }

        [Test]
        public void NotInitializedException_RootPage()
        {
            Assert.Throws<NotInitializedException>(() => { var page = navController.RootPage; });
        }

        [Test]
        public void NotInitializedException_HiddenPage()
        {
            Assert.Throws<NotInitializedException>(() => { var page = navController.HiddenPage; });
        }

        [Test]
        public void NotInitializedException_TopPage()
        {
            Assert.Throws<NotInitializedException>(() => { var page = navController.TopPage; });
        }

        [Test]
        public void NotInitializedException_RootViewModel()
        {
            Assert.Throws<NotInitializedException>(() => { var page = navController.RootViewModel; });
        }

        [Test]
        public void NotInitializedException_HiddenViewModel()
        {
            Assert.Throws<NotInitializedException>(() => { var page = navController.HiddenViewModel; });
        }

        [Test]
        public void NotInitializedException_TopViewModel()
        {
            Assert.Throws<NotInitializedException>(() => { var page = navController.TopViewModel; });
        }

        [Test]
        public void NotInitializedException_PushAsync()
        {
            Assert.ThrowsAsync<NotInitializedException>(async () => { await navController.PushAsync<TestViewModel0>(); });
        }

        [Test]
        public void NotInitializedException_PushAsync_Di()
        {
            Assert.ThrowsAsync<NotInitializedException>(async () => { await navController.PushAsync(new TestViewModel0()); });
        }

        [Test]
        public void NotInitializedException_PopAsync()
        {
            Assert.ThrowsAsync<NotInitializedException>(async () => { await navController.PopAsync(); });
        }
    }
}
