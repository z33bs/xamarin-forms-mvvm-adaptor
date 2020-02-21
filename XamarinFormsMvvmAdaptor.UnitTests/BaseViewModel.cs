using System;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.UnitTests
{
    public class BaseViewModel : IAdaptorViewModel
    {
        public BaseViewModel()
        {
        }

        public Task InitializeAsync(object navigationData)
        {
            return Task.FromResult(true);
        }

        public Task OnAppearingAsync()
        {
            return Task.FromResult(false);
        }
    }
}
