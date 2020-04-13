using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public interface IMvvmBase
    {
        IIoc Ioc { get; }
        Page NavigationRoot { get; }
        bool IsInitialized { get; }
        IReadOnlyList<Page> MainStack { get; }
        IReadOnlyList<Page> ModalStack { get; }
        Page RootPage { get; }
        Page TopPage { get; }
        Page HiddenPage { get; }
        IBaseViewModel RootViewModel { get; }
        IBaseViewModel TopViewModel { get; }
        IBaseViewModel HiddenViewModel { get; }

        Task<IBaseViewModel> CollapseMainStack();
        Task<IBaseViewModel> PopAsync(bool animated = true);
        Task<IBaseViewModel> PopMainStackToRootAsync(bool animated = true);
        Task<IBaseViewModel> PopModalAsync(bool animated = true);
        Task<TViewModel> PushAsync<TViewModel>(TViewModel viewModel, object navigationData = null, bool animated = true) where TViewModel : class, IBaseViewModel;
        Task<TViewModel> PushAsync<TViewModel>(object navigationData = null, bool animated = true) where TViewModel : class, IBaseViewModel;
        Task<TViewModel> PushModalAsync<TViewModel>(TViewModel viewModel, object navigationData = null, bool animated = true) where TViewModel : class, IBaseViewModel;
        Task<TViewModel> PushModalAsync<TViewModel>(object navigationData = null, bool animated = true) where TViewModel : class, IBaseViewModel;
        Task RemovePageFor<TViewModel>() where TViewModel : IBaseViewModel;
        Task RemovePreviousPageFromMainStack();
    }
}