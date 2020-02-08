using System.Threading.Tasks;

using MvvmHelpers;

namespace XamarinFormsMvvmAdaptor
{
    public class MvvmAdaptorViewModel : BaseViewModel, IMvvmAdaptorViewModel
    {
        public Task InitializeAsync(object navigationData)
        {
            return Task.FromResult(false);
        }
    }
}
