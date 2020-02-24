using NUnit.Framework;
using Xamarin.Forms;
using XamarinFormsMvvmAdaptor;
using XamarinFormsMvvmAdaptor.UnitTests.Views;
using XamarinFormsMvvmAdaptor.UnitTests.ViewModels;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.UnitTests
{
    public class Stack4Pages
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
            var stack = navController.MainStack;
            Assert.Multiple(() =>
            {
                Assert.AreSame(stack[stack.Count - 1], navController.TopPage);
                Assert.IsInstanceOf<TestPage3>(navController.TopPage);
            });
        }

        [Test]
        public void HiddenPage_returns_hidden_page()
        {
            var stack = navController.MainStack;
            Assert.Multiple(() =>
            {
                Assert.AreSame(stack[stack.Count - 2], navController.HiddenPage);
                Assert.IsInstanceOf<TestPage2>(navController.HiddenPage);
            });
        }

        [Test]
        public void RootPage_returns_root_page()
        {
            Assert.Multiple(() =>
            {
                Assert.AreSame(navController.MainStack[0], navController.RootPage);
                Assert.IsInstanceOf<TestPage0>(navController.RootPage);
            });
        }

        [Test]
        public void TopViewModel_associated_with_TopPage()
        {
            Assert.Multiple(() =>
            {

                Assert.IsInstanceOf<TestViewModel3>(navController.TopViewModel);
                Assert.AreSame(navController.TopViewModel, navController.TopPage.BindingContext);
            });
        }

        [Test]
        public void HiddenViewModel_associated_with_HiddenPage()
        {
            Assert.Multiple(() =>
            {

                Assert.IsInstanceOf<TestViewModel2>(navController.HiddenViewModel);
                Assert.AreSame(navController.HiddenViewModel, navController.HiddenPage.BindingContext);
            });
        }

        [Test]
        public void RootViewModel_associated_with_RootPage()
        {
            Assert.Multiple(() =>
            {

                Assert.IsInstanceOf<TestViewModel0>(navController.RootViewModel);
                Assert.AreSame(navController.RootViewModel, navController.RootPage.BindingContext);
            });
        }

        [Test]
        public void RemoveHiddenPageFromStack()
        {
            Assume.That(navController.HiddenPage is TestPage2);
            navController.RemovePreviousPageFromMainStack();
            Assert.Multiple(() =>
            {
                Assert.IsTrue(navController.MainStack.Count == 3);
                Assert.IsInstanceOf<TestPage1>(navController.HiddenPage);
            });
        }

    }
}
