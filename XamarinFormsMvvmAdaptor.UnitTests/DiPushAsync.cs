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
            navController.DiPushAsync(new DiTestViewModel1(),null,isAnimated);
            Assume.That(navController.TopViewModel is DiTestViewModel1);
            var vm = navController.TopViewModel as DiTestViewModel1;
            Assert.IsTrue(vm.IsInitialized);
        }

        [Test]
        public void PushAsync_passes_data_to_ViewModel()
        {
            var data = new string[] { "Hello", "World" };
            navController.DiPushAsync(new DiTestViewModel1(),data);
            Assume.That(navController.TopViewModel is DiTestViewModel1);
            Assert.AreSame(data,
                (navController.TopViewModel as DiTestViewModel1)
                .NavigationData);
        }

        [Test]
        public void PushAsync_Runs_OnAppearing()
        {
            navController.DiPushAsync(new DiTestViewModel1());
            Assume.That(navController.TopViewModel is DiTestViewModel1);
            var vm = navController.TopViewModel as DiTestViewModel1;
            Assert.IsTrue(vm.OnAppearingRuns==1);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void PushModalAsync_fires_InitializeAsync(bool isAnimated)
        {
            navController.DiPushModalAsync(new DiTestViewModel1(), null, isAnimated);
            Assume.That(navController.TopViewModel is DiTestViewModel1);
            var vm = navController.TopViewModel as DiTestViewModel1;
            Assert.IsTrue(vm.IsInitialized);
        }

        [Test]
        public void PushModalAsync_passes_data_to_ViewModel()
        {
            var data = new string[] { "Hello", "World" };
            navController.DiPushModalAsync(new DiTestViewModel1(), data);
            Assume.That(navController.TopViewModel is DiTestViewModel1);
            Assert.AreSame(data,
                (navController.TopViewModel as DiTestViewModel1)
                .NavigationData);
        }

        [Test]
        public void PushModalAsync_Runs_OnAppearing()
        {
            navController.DiPushModalAsync(new DiTestViewModel1());
            Assume.That(navController.TopViewModel is DiTestViewModel1);
            var vm = navController.TopViewModel as DiTestViewModel1;
            Assert.IsTrue(vm.OnAppearingRuns == 1);
        }

        [Test]
        public void DiCreatePageFor_returns_appropriate_page()
        {
            Assert.IsInstanceOf<DiTestPage0>(NavController.DiCreatePageForAsync(new DiTestViewModel0()));
        }

    }
}
