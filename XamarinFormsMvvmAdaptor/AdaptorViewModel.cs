using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor
{
    public abstract class AdaptorViewModel : MvvmHelpers.BaseViewModel, IAdaptorViewModel
    {
        public virtual Task InitializeAsync(object navigationData)
        {
            return Task.FromResult(false);
        }
    }
}
