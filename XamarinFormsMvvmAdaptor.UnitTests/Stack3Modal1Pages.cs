using NUnit.Framework;
using Xamarin.Forms;
using XamarinFormsMvvmAdaptor;
using XamarinFormsMvvmAdaptor.UnitTests.Views;
using XamarinFormsMvvmAdaptor.UnitTests.ViewModels;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.UnitTests
{
    public class Stack3Modal1Pages
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
            await navController.PushModalAsync<TestViewModel3>();
        }

        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void TopPage_returns_top_page()
        {            
            Assert.Multiple(() =>
            {
                Assert.AreSame(navController.ModalStack[0], navController.TopPage);
                Assert.IsInstanceOf<TestPage3>(navController.TopPage);
            });
        }

        [Test]
        public void HiddenPage_returns_hidden_page()
        {
            Assert.Multiple(() =>
            {
                Assert.AreSame(navController.NavigationStack[2], navController.HiddenPage);
                Assert.IsInstanceOf<TestPage2>(navController.HiddenPage);
            });
        }

        [Test]
        public void RemoveHiddenPageFromStack()
        {
            Assume.That(
                navController.NavigationStack
                .GetCurrentPage()
                is TestPage2);
            Assume.That(navController.NavigationStack
                .GetPreviousPage()
                is TestPage1);
            Assume.That(navController.NavigationStack.Count == 3);
            Assume.That(navController.ModalStack.Count == 1);
            navController.RemovePreviousPageFromMainStack();
            Assert.Multiple(() =>
            {
                Assert.IsTrue(navController.NavigationStack.Count == 2);
                Assert.IsTrue(navController.ModalStack.Count == 1);
                Assert.IsInstanceOf<TestPage2>(
                    navController.NavigationStack
                    .GetCurrentPage());
                Assert.IsInstanceOf<TestPage0>(
                    navController.NavigationStack
                    .GetPreviousPage());
            });
        }

    }
}
