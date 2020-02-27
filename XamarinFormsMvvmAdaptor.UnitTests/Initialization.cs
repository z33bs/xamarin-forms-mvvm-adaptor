using NUnit.Framework;
using XamarinFormsMvvmAdaptor.UnitTests.Views;
using XamarinFormsMvvmAdaptor.UnitTests.ViewModels;
using System.Threading.Tasks;
using Xamarin.Forms;

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
        public void Root_returns_NotInitializedException_if_not_initialized()
        {
            NavigationPage test;
            Assert.Throws<NotInitializedException>(()=> test = navController.Root);
        }

        [Test]
        public async Task Root_returns_NavigationPage_if_initialized()
        {
            await navController.InitAsync(new TestPage0());
            Assert.IsInstanceOf<NavigationPage>(navController.Root);
        }

        [Test]
        public async Task DiInitAsync_Page_IsInitialized_True()
        {
            await navController.DiInitAsync(new DiTestViewModel0());
            Assert.IsTrue(navController.IsInitialized);
        }

        [Test]
        public async Task InitAsync_passes_data_to_viewmodel()
        {
            var data = "Hello";
            await navController.InitAsync(new TestPage1(), data);
            Assert.AreEqual(data, (navController.RootViewModel as TestViewModel1).NavigationData as string);
        }

        [Test]
        public async Task DiInitAsync_passes_data_to_viewmodel()
        {
            var data = "Hello";
            await navController.DiInitAsync(new DiTestViewModel1(), data);
            Assert.AreEqual(data, (navController.RootViewModel as DiTestViewModel1).NavigationData as string);
        }

        [TearDown]
        public void TearDown()
        {
            NavController.SetNamingConventions();
        }

        [Test]
        public async Task SetNamingConventions_subNameSpace()
        {
            NavController.SetNamingConventions("Vms","Pages");
            navController = new NavController();
            await navController.InitAsync(new Pages.TestVmsPage());

            Assert.IsInstanceOf<Vms.TestVmsViewModel>(navController.RootViewModel);

            await navController.PushAsync<Vms.TestVmsViewModel>();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, navController.MainStack.Count);
                Assert.IsInstanceOf<Pages.TestVmsPage>(navController.TopPage);
            });
        }

        [Test]
        public async Task SetNamingConventions_subNameSpace_withDi()
        {
            NavController.SetNamingConventions("Vms", "Pages");
            navController = new NavController();
            await navController.DiInitAsync(new Vms.DiTestVmsViewModel());

            Assert.IsInstanceOf<Pages.DiTestVmsPage>(navController.RootPage);

            await navController.DiPushAsync(new Vms.DiTestVmsViewModel());

            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, navController.MainStack.Count);
                Assert.IsInstanceOf<Pages.DiTestVmsPage>(navController.TopPage);
            });
        }

        [Test]
        public async Task SetNamingConventions_suffix()
        {
            NavController.SetNamingConventions(viewModelSuffix: "Vm", viewSuffix: "Pg");
            navController = new NavController();
            await navController.InitAsync(new TestPg0());

            Assert.IsInstanceOf<TestVm0>(navController.RootViewModel);

            await navController.PushAsync<TestVm0>();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, navController.MainStack.Count);
                Assert.IsInstanceOf<TestPg0>(navController.TopPage);
            });
        }

        [Test]
        public async Task SetNamingConventions_suffix_withDi()
        {
            NavController.SetNamingConventions(viewModelSuffix: "Vm", viewSuffix: "Pg");
            navController = new NavController();
            await navController.DiInitAsync(new DiTestVm0());

            Assert.IsInstanceOf<DiTestPg0>(navController.RootPage);

            await navController.DiPushAsync(new DiTestVm0());

            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, navController.MainStack.Count);
                Assert.IsInstanceOf<DiTestPg0>(navController.TopPage);
            });
        }

        [Test]
        public async Task SetNamingConventions_subNameSpace_and_suffix()
        {
            NavController.SetNamingConventions("Vms", "Pages", "Vm", "Pg");
            navController = new NavController();
            await navController.InitAsync(new Pages.TestVmsPg());

            Assert.IsInstanceOf<Vms.TestVmsVm>(navController.RootViewModel);

            await navController.PushAsync<Vms.TestVmsVm>();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, navController.MainStack.Count);
                Assert.IsInstanceOf<Pages.TestVmsPg>(navController.TopPage);
            });
        }

        [Test]
        public async Task SetNamingConventions_subNameSpace_and_suffix_withDi()
        {
            NavController.SetNamingConventions("Vms", "Pages", "Vm", "Pg");
            navController = new NavController();
            await navController.DiInitAsync(new Vms.DiTestVmsVm());

            Assert.IsInstanceOf<Pages.DiTestVmsPg>(navController.RootPage);

            await navController.DiPushAsync(new Vms.DiTestVmsVm());

            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, navController.MainStack.Count);
                Assert.IsInstanceOf<Pages.DiTestVmsPg>(navController.TopPage);
            });
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
            Assert.ThrowsAsync<NotInitializedException>(async () => { await navController.DiPushAsync(new TestViewModel0()); });
        }

        [Test]
        public void NotInitializedException_PopAsync()
        {
            Assert.ThrowsAsync<NotInitializedException>(async () => { await navController.PopAsync(); });
        }
    }
}
