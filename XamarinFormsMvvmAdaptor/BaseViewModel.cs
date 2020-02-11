using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor
{
    public abstract class BaseViewModel : MvvmHelpers.BaseViewModel, IBaseViewModel
    {
        public virtual Task InitializeAsync(object navigationData)
        {
            return Task.FromResult(false);
        }
    }
}
