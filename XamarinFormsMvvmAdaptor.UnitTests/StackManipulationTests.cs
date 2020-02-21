using NUnit.Framework;
using Xamarin.Forms;
using XamarinFormsMvvmAdaptor;
using XamarinFormsMvvmAdaptor.UnitTests.Views;
using XamarinFormsMvvmAdaptor.UnitTests.ViewModels;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.UnitTests
{
    public class StackManipulationTests
    {
        INavController navController;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            navController = new NavController();
            await navController.InitAsync(new TestPage0());
            await navController.PushAsync<TestViewModel1>();
            await navController.PushAsync<TestViewModel2>();
            await navController.PushAsync<TestViewModel3>();
        }

        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void TopPage_returns_top_page()
        {
            var stack = navController.NavigationStack;
            Assert.Multiple(() =>
            {
                Assert.AreSame(stack[stack.Count - 1], navController.TopPage);
                Assert.IsInstanceOf<TestPage3>(navController.TopPage);
            });
        }

        [Test]
        public void HiddenPage_returns_hidden_page()
        {
            var stack = navController.NavigationStack;
            Assert.Multiple(() =>
            {
                Assert.AreSame(stack[stack.Count - 2], navController.HiddenPage);
                Assert.IsInstanceOf<TestPage2>(navController.HiddenPage);
            });
        }



        //[Test]
        //public async Task PageProperties_Return_Expected_Pages_After_Stack_Manipulation()
        //{
        //    TestContext.Out.WriteLine("Out WriteLIne");
        //    TestContext.WriteLine("Straight WriteLIne");

        //    await navController.InitAsync(new TestPage0());
        //    //todo refactor so get the page
        //    Assert.IsInstanceOf<TestPage0>((navController.RootPage as NavigationPage).RootPage);

        //    await navController.PushAsync<TestViewModel1>();
        //    await navController.PushAsync<TestViewModel2>();
        //    Assert.IsInstanceOf<TestPage2>(navController.TopPage);
        //    Assert.IsInstanceOf<TestPage1>(navController.HiddenPage);

        //    await navController.PushAsync<TestViewModel3>();
        //    Assert.IsInstanceOf<TestPage3>(navController.TopPage);
        //    Assert.IsInstanceOf<TestPage2>(navController.HiddenPage);

        //    await navController.PushModalAsync<TestViewModel4>();
        //    Assert.IsInstanceOf<TestPage4>(navController.TopPage);
        //    Assert.IsInstanceOf<TestPage3>(navController.HiddenPage);

        //    await navController.PushModalAsync<TestViewModel5>();
        //    Assert.IsInstanceOf<TestPage5>(navController.TopPage);
        //    Assert.IsInstanceOf<TestPage4>(navController.HiddenPage);

        //    Assert.IsInstanceOf<TestPage0>(navController.NavigationStack[0]);
        //    Assert.IsInstanceOf<TestPage1>(navController.NavigationStack[1]);
        //    Assert.IsInstanceOf<TestPage2>(navController.NavigationStack[2]);
        //    Assert.IsInstanceOf<TestPage3>(navController.NavigationStack[3]);
        //    Assert.IsInstanceOf<TestPage4>(navController.ModalStack[0]);
        //    Assert.IsInstanceOf<TestPage5>(navController.ModalStack[1]);

        //    await navController.PopModalAsync();
        //    Assert.IsInstanceOf<TestPage4>(navController.TopPage);
        //    Assert.IsInstanceOf<TestPage3>(navController.HiddenPage);

        //    await navController.PopModalAsync();
        //    Assert.IsInstanceOf<TestPage3>(navController.TopPage);
        //    Assert.IsInstanceOf<TestPage2>(navController.HiddenPage);

        //    await navController.PopAsync();
        //    Assert.IsInstanceOf<TestPage2>(navController.TopPage);
        //    Assert.IsInstanceOf<TestPage1>(navController.HiddenPage);

        //    await navController.PopAsync();
        //    Assert.IsInstanceOf<TestPage1>(navController.TopPage);
        //    Assert.IsInstanceOf<TestPage0>(navController.HiddenPage);

        //    await navController.PopAsync();
        //    Assert.IsInstanceOf<TestPage0>(navController.TopPage);
        //    //todo check rootpage after refactored
        //    Assert.IsNull(navController.HiddenPage);
        //}

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
