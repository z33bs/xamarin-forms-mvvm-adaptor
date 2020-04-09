using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public interface INavigationService
    {
        Task GoToAsync(ShellNavigationState state, bool animate);
        Task GoToAsync(ShellNavigationState state);
        Task<TViewModel> PushAsync<TViewModel>(object navigationData = null, bool animated = true) where TViewModel : class, IMvvmViewModelBase;
        Task<TViewModel> PushModalAsync<TViewModel>(object navigationData = null, bool animated = true) where TViewModel : class, IMvvmViewModelBase;
    }
}