using NUnit.Framework;
using Xamarin.Forms;
using XamarinFormsMvvmAdaptor;
using XamarinFormsMvvmAdaptor.UnitTests.Views;
using XamarinFormsMvvmAdaptor.UnitTests.ViewModels;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.UnitTests
{
    public class Stack2Pages
    {
        INavController navController;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            navController = new NavController();
            await navController.InitAsync(new TestPage0());
            await navController.PushAsync<TestViewModel1>();
        }

        [Test]
        public void HiddenPage_is_RootPage()
        {
            Assert.AreSame(navController.HiddenPage, navController.RootPage);
        }
    }
}
