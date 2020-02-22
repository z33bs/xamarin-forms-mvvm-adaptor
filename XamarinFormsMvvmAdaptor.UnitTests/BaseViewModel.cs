using System;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.UnitTests
{
    public class BaseViewModel : IAdaptorViewModel
    {
        public BaseViewModel()
        {
        }

        public virtual Task InitializeAsync(object navigationData)
        {
            return Task.FromResult(true);
        }

        public virtual Task OnAppearingAsync()
        {
            return Task.FromResult(false);
        }
    }
}
