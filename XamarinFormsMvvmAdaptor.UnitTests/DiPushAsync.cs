using System;
using System.Threading.Tasks;
using NUnit.Framework;
using XamarinFormsMvvmAdaptor.UnitTests.ViewModels;
using XamarinFormsMvvmAdaptor.UnitTests.Views;

namespace XamarinFormsMvvmAdaptor.UnitTests
{
    public class PushAsync
    {
        INavController navController;

        [OneTimeSetUp]
        public async Task SetupOnce()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            navController = new NavController();
            await navController.InitAsync(new TestPage0());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void PushAsync_fires_InitializeAsync(bool isAnimated)
        {
            navController.PushAsync<TestViewModel1>(null,isAnimated);
            Assume.That(navController.TopViewModel is TestViewModel1);
            var vm = navController.TopViewModel as TestViewModel1;
            Assert.IsTrue(vm.IsInitialized);
        }

        [Test]
        public void PushAsync_passes_data_to_ViewModel()
        {
            var data = new string[] { "Hello", "World" };
            navController.PushAsync<TestViewModel1>(data);
            Assume.That(navController.TopViewModel is TestViewModel1);
            var vm = navController.TopViewModel as TestViewModel1;
            Assert.AreSame(data,
                (navController.TopViewModel as TestViewModel1)
                .NavigationData);
        }

        [Test]
        public void PushAsync_Runs_OnAppearing()
        {
            navController.PushAsync<TestViewModel1>();
            Assume.That(navController.TopViewModel is TestViewModel1);
            var vm = navController.TopViewModel as TestViewModel1;
            Assert.IsTrue(vm.OnAppearingRuns==1);
        }
    }
}
