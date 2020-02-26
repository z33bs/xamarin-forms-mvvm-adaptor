using NUnit.Framework;
using XamarinFormsMvvmAdaptor.UnitTests.Views;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.UnitTests
{
    public class Stack1Page
    {
        INavController navController;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            navController = new NavController();
            await navController.InitAsync(new TestPage0());
        }

        [Test]
        public void TopPage_is_RootPage()
        {
            Assert.AreSame(navController.TopPage, navController.RootPage);
        }

        [Test]
        public void HiddenPage_is_Null()
        {
            Assert.IsNull(navController.HiddenPage);
        }

        [Test]
        public void GetPreviousViewModel_returns_null_if_only_1page()
        {
            Assert.IsNull(navController.ModalStack.GetPreviousViewModel());
        }

    }
}
