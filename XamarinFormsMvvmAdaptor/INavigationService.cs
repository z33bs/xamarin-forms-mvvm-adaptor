using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public interface INavigationService
    {
        IReadOnlyList<Page> NavigationStack { get; }
        IReadOnlyList<Page> ModalStack { get; }

        Task GoToAsync(ShellNavigationState state, bool animate);
        Task GoToAsync(ShellNavigationState state);
        Task PopAsync(bool animated = true);
        Task PopModalAsync(bool animated = true);
        Task PopToRootAsync(bool animated = true);
        Task<TViewModel> PushAsync<TViewModel>(object navigationData, bool animated = true) where TViewModel : class, IPushed;
        Task<TViewModel> PushAsync<TViewModel>(bool animated = true) where TViewModel : class;
        Task<TViewModel> PushModalAsync<TViewModel>(object navigationData, bool animated = true) where TViewModel : class, IPushed;
        Task<TViewModel> PushModalAsync<TViewModel>(bool animated = true) where TViewModel : class;
        Task RemovePageFor<TViewModel>() where TViewModel : IBaseViewModel;
        Task RemovePreviousPageFromMainStack();
    }
}