using System.Threading.Tasks;
using NUnit.Framework;
using XamarinFormsMvvmAdaptor.UnitTests.ViewModels;
using XamarinFormsMvvmAdaptor.UnitTests.Views;

namespace XamarinFormsMvvmAdaptor.UnitTests
{
    public class InsertPageBefore
    {
        NavController navController;
        NavController diNavController;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
        }

        [SetUp]
        public async Task SetupAsync()
        {
            navController = new NavController();
            await navController.InitAsync(new TestPage0());
            await navController.PushAsync<TestViewModel1>();
            await navController.PushAsync<TestViewModel2>();
            await navController.PushAsync<TestViewModel3>();

            diNavController = new NavController();
            await diNavController.DiInitAsync(new DiTestViewModel0());
            await diNavController.DiPushAsync(new DiTestViewModel1());
            await diNavController.DiPushAsync(new DiTestViewModel2());
            await diNavController.DiPushAsync(new DiTestViewModel3());
        }

        [Test]
        public async Task InsertPageBefore_inserts_page_in_correct_spot()
        {
            Assume.That(navController.MainStack.Count == 4);
            Assume.That(navController.MainStack[2] is TestPage2);

            await navController.InsertPageBefore<TestViewModel2, TestViewModel5>();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(navController.MainStack.Count == 5);
                Assert.IsInstanceOf<TestPage5>(navController.MainStack[2]);
            });
        }


        [Test]
        public async Task InsertPageBefore_fires_InitializeAsync()
        {
            Assume.That(navController.MainStack.Count == 4);
            Assume.That(navController.MainStack[2] is TestPage2);

            await navController.InsertPageBefore<TestViewModel2, TestViewModel1>();

            Assume.That(navController.MainStack[2] is TestPage1);
            Assert.IsTrue((navController.MainStack[2].BindingContext as TestViewModel1).IsInitialized);
        }

        [Test]
        public async Task InsertPageBefore_passes_data_to_viewmodel()
        {
            var testData = "Test data";
            Assume.That(navController.MainStack[2] is TestPage2);

            await navController.InsertPageBefore<TestViewModel2, TestViewModel1>(testData);

            Assume.That(navController.MainStack[2] is TestPage1);
            Assert.AreEqual(testData, (string)(navController.MainStack[2].BindingContext as TestViewModel1).NavigationData);
        }

    }
}
