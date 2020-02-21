using NUnit.Framework;
using Xamarin.Forms;
using XamarinFormsMvvmAdaptor;
using XamarinFormsMvvmAdaptor.UnitTests.Views;
using XamarinFormsMvvmAdaptor.UnitTests.ViewModels;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.UnitTests
{
    public class NavControllerTests
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
            TestContext.WriteLine(nameof(Setup) + " runs before each test");
            navController = new NavController();
        }

        [TestCase("Hellow")]
        public void New_NavController_IsInitialized_False(string message)
        {
            TestContext.WriteLine(message);
            Assert.IsFalse(navController.IsInitialized);
        }

        [Test]
        public async Task InitAsync_Page_IsInitialized_True()
        {
            await navController.InitAsync(new TestPage0());
            Assert.IsTrue(navController.IsInitialized);
        }
        //[Test]
        //public async Task InitAsync_Vm_IsInitialized_True()
        //{
        //    await navController.InitAsync(new TestViewModel0());
        //    Assert.IsTrue(navController.IsInitialized);
        //}


        [Test]
        public async Task RootPage_Default_Is_NavigationPage()
        {
            await navController.InitAsync(new TestPage0());
            Assert.IsInstanceOf<NavigationPage>(navController.RootPage);
        }

        [Test]
        public async Task RootPage_CanBe_Page()
        {
            await navController.InitAsync(new TestPage0(),isWrappedInNavigationPage:false);
            Assert.IsInstanceOf<Page>(navController.RootPage);
            Assert.IsNotInstanceOf<NavigationPage>(navController.RootPage);
        }

        [Test]
        public async Task PageProperties_Return_Expected_Pages_After_Stack_Manipulation()
        {
            TestContext.Out.WriteLine("Out WriteLIne");
            TestContext.WriteLine("Straight WriteLIne");

            await navController.InitAsync(new TestPage0());
            //todo refactor so get the page
            Assert.IsInstanceOf<TestPage0>((navController.RootPage as NavigationPage).RootPage);

            await navController.PushAsync<TestViewModel1>();
            await navController.PushAsync<TestViewModel2>();
            Assert.IsInstanceOf<TestPage2>(navController.TopPage);
            Assert.IsInstanceOf<TestPage1>(navController.HiddenPage);

            await navController.PushAsync<TestViewModel3>();
            Assert.IsInstanceOf<TestPage3>(navController.TopPage);
            Assert.IsInstanceOf<TestPage2>(navController.HiddenPage);

            await navController.PushModalAsync<TestViewModel4>();
            Assert.IsInstanceOf<TestPage4>(navController.TopPage);
            Assert.IsInstanceOf<TestPage3>(navController.HiddenPage);

            await navController.PushModalAsync<TestViewModel5>();
            Assert.IsInstanceOf<TestPage5>(navController.TopPage);
            Assert.IsInstanceOf<TestPage4>(navController.HiddenPage);

            Assert.IsInstanceOf<TestPage0>(navController.NavigationStack[0]);
            Assert.IsInstanceOf<TestPage1>(navController.NavigationStack[1]);
            Assert.IsInstanceOf<TestPage2>(navController.NavigationStack[2]);
            Assert.IsInstanceOf<TestPage3>(navController.NavigationStack[3]);
            Assert.IsInstanceOf<TestPage4>(navController.ModalStack[0]);
            Assert.IsInstanceOf<TestPage5>(navController.ModalStack[1]);

            await navController.PopModalAsync();
            Assert.IsInstanceOf<TestPage4>(navController.TopPage);
            Assert.IsInstanceOf<TestPage3>(navController.HiddenPage);

            await navController.PopModalAsync();
            Assert.IsInstanceOf<TestPage3>(navController.TopPage);
            Assert.IsInstanceOf<TestPage2>(navController.HiddenPage);

            await navController.PopAsync();
            Assert.IsInstanceOf<TestPage2>(navController.TopPage);
            Assert.IsInstanceOf<TestPage1>(navController.HiddenPage);

            await navController.PopAsync();
            Assert.IsInstanceOf<TestPage1>(navController.TopPage);
            Assert.IsInstanceOf<TestPage0>(navController.HiddenPage);

            await navController.PopAsync();
            Assert.IsInstanceOf<TestPage0>(navController.TopPage);
            //todo check rootpage after refactored
            Assert.IsNull(navController.HiddenPage);
        }

        [Test]
        public async Task ViewModelProperties_Correspond_To_PageProperties()
        {
            await navController.InitAsync(new TestPage0());
            Assert.IsInstanceOf<TestViewModel0>(navController.RootViewModel);
            //todo when rootpage fixed

            await navController.PushAsync<TestViewModel1>();
            await navController.PushAsync<TestViewModel2>();
            Assert.IsInstanceOf<TestPage2>(navController.TopPage);
            Assert.IsInstanceOf<TestViewModel2>(navController.TopViewModel);
            Assert.AreSame(navController.TopViewModel, navController.TopPage.BindingContext);

            Assert.IsInstanceOf<TestPage1>(navController.HiddenPage);
            Assert.IsInstanceOf<TestViewModel1>(navController.HiddenViewModel);
            Assert.AreSame(navController.HiddenViewModel, navController.HiddenPage.BindingContext);
        }

    }
}
