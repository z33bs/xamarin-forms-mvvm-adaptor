using System.Threading.Tasks;
using NUnit.Framework;
using XamarinFormsMvvmAdaptor.UnitTests.ViewModels;
using XamarinFormsMvvmAdaptor.UnitTests.Views;

namespace XamarinFormsMvvmAdaptor.UnitTests
{
    public class DiInsertPageBefore
    {
        NavController navController;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
        }

        [SetUp]
        public async Task SetupAsync()
        {

            navController = new NavController();
            await navController.DiInitAsync(new DiTestViewModel0());
            await navController.DiPushAsync(new DiTestViewModel1());
            await navController.DiPushAsync(new DiTestViewModel2());
            await navController.DiPushAsync(new DiTestViewModel3());
        }

        [Test]
        public async Task InsertPageBefore_inserts_page_in_correct_spot()
        {
            Assume.That(navController.MainStack.Count == 4);
            Assume.That(navController.MainStack[2] is DiTestPage2);

            await navController.DiInsertPageBefore<DiTestViewModel2>(new DiTestViewModel5());

            Assert.Multiple(() =>
            {
                Assert.IsTrue(navController.MainStack.Count == 5);
                Assert.IsInstanceOf<DiTestPage5>(navController.MainStack[2]);
            });
        }


        [Test]
        public async Task InsertPageBefore_fires_InitializeAsync()
        {
            Assume.That(navController.MainStack.Count == 4);
            Assume.That(navController.MainStack[2] is DiTestPage2);

            await navController.DiInsertPageBefore<DiTestViewModel2>(new DiTestViewModel1());

            Assume.That(navController.MainStack[2] is DiTestPage1);
            Assert.IsTrue((navController.MainStack[2].BindingContext as DiTestViewModel1).IsInitialized);
        }

        [Test]
        public async Task InsertPageBefore_passes_data_to_viewmodel()
        {
            var testData = "Test data";
            Assume.That(navController.MainStack[2] is DiTestPage2);

            await navController.DiInsertPageBefore<DiTestViewModel2>(new DiTestViewModel1(),testData);

            Assume.That(navController.MainStack[2] is DiTestPage1);
            Assert.AreEqual(testData, (string)(navController.MainStack[2].BindingContext as DiTestViewModel1).NavigationData);
        }

    }
}
