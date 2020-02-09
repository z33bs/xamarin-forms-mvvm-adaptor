using System.Threading.Tasks;

using MvvmHelpers;

namespace XamarinFormsMvvmAdaptor
{
    public abstract class MvvmAdaptorViewModel : BaseViewModel, IMvvmAdaptorViewModel
    {
        public virtual Task InitializeAsync(object navigationData)
        {
            return Task.FromResult(false);
        }
    }
}
