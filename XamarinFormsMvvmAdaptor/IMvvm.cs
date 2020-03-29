using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public interface IMvvm
    {
        IIoc Ioc { get; }
        Page NavigationRoot { get; }
        bool IsInitialized { get; }
        IReadOnlyList<Page> MainStack { get; }
        IReadOnlyList<Page> ModalStack { get; }
        Page RootPage { get; }
        Page TopPage { get; }
        Page HiddenPage { get; }
        IMvvmViewModelBase RootViewModel { get; }
        IMvvmViewModelBase TopViewModel { get; }
        IMvvmViewModelBase HiddenViewModel { get; }

        Task<IMvvmViewModelBase> CollapseMainStack();
        Page Initialize<TViewModel>(bool mustWrapInNavigationPage = true) where TViewModel : class, IMvvmViewModelBase;
        Task<TViewModel>  PushAsync<TViewModel>(TViewModel viewModel, object navigationData = null, bool animated = true) where TViewModel : class, IMvvmViewModelBase;
        Task<TViewModel>  PushAsync<TViewModel>(object navigationData = null, bool animated = true) where TViewModel : class, IMvvmViewModelBase;
        Task<TViewModel>  PushModalAsync<TViewModel>(TViewModel viewModel, object navigationData = null, bool animated = true) where TViewModel : class, IMvvmViewModelBase;
        Task<TViewModel>  PushModalAsync<TViewModel>(object navigationData = null, bool animated = true) where TViewModel : class, IMvvmViewModelBase;
        Task<IMvvmViewModelBase> PopAsync(bool animated = true);
        Task<IMvvmViewModelBase> PopModalAsync(bool animated = true);
        Task<IMvvmViewModelBase> PopMainStackToRootAsync(bool animated = true);
        Task RemovePageFor<TViewModel>() where TViewModel : IMvvmViewModelBase;
        Task RemovePreviousPageFromMainStack();
    }
}