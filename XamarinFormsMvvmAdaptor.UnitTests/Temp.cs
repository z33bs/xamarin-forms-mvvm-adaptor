using System;
using System.Threading.Tasks;
using XamarinFormsMvvmAdaptor.UnitTests.ViewModels;
using XamarinFormsMvvmAdaptor.UnitTests.Views;

namespace XamarinFormsMvvmAdaptor.UnitTests
{
    public class Temp
    {
        public async Task Main()
        {
            var navController = new NavController();
            await navController.InitAsync(new TestPage0());
            await navController.PushAsync<TestViewModel1>();
            await navController.PushAsync<TestViewModel2>();
            await navController.PushAsync<TestViewModel3>();

            await navController.InsertPageBefore<TestViewModel2, TestViewModel5>();
        }
    }
}
