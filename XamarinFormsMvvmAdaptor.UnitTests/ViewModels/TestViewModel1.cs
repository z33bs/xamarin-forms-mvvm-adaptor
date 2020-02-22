using System;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.UnitTests.ViewModels
{
    public class TestViewModel1 : BaseViewModel
    {
        public object NavigationData { get; private set; }
        public bool IsInitialized { get; private set; }
        public int OnAppearingRuns { get; private set; }

        public override Task InitializeAsync(object navigationData)
        {
            NavigationData = navigationData;
            IsInitialized = true;
            return base.InitializeAsync(navigationData);
        }

        public override Task OnAppearingAsync()
        {
            OnAppearingRuns++;
            return base.OnAppearingAsync();
        }
    }
}
