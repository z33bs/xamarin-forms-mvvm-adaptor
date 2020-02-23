using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public interface INavController
    {
        #region Properties
        bool IsInitialized { get; }
        IReadOnlyList<Page> MainStack { get; }
        IReadOnlyList<Page> ModalStack { get; }
        Page RootPage { get; }
        Page TopPage { get; }
        Page HiddenPage { get; }
        IAdaptorViewModel RootViewModel { get; }
        IAdaptorViewModel TopViewModel { get; }
        IAdaptorViewModel HiddenViewModel { get; }
        #endregion

        #region Vanilla Implementation
        Task InitAsync(Page rootPage, object initialisationData = null);
        Task InsertPageBefore<TViewModelExisting, TViewModelNew>(object navigationData = null);
        Task PushAsync<TViewModel>(object navigationData = null, bool animated = true) where TViewModel : IAdaptorViewModel;
        Task PushModalAsync<TViewModel>(object navigationData, bool animated) where TViewModel : IAdaptorViewModel;
        void RemovePageFor<TViewModel>() where TViewModel : IAdaptorViewModel;
        #endregion
        #region Di Implementation
        Task InitAsync(IAdaptorViewModel rootViewModel, object initialisationData = null);
        Task DiPushAsync(IAdaptorViewModel viewModel, object navigationData = null, bool animated = true);
        Task DiPushModalAsync(IAdaptorViewModel viewModel, object navigationData, bool animated);
        Task InsertPageBefore<TViewModelExisting>(IAdaptorViewModel viewModel, object navigationData = null);
        #endregion
        #region Common/Shared Methods
        Task PopAsync(bool animated = true);
        Task PopModalAsync(bool animated = true);
        Task PopToRootAsync(bool animated = true);
        void CollapseMainStack();
        void RemovePreviousPageFromMainStack();
        #endregion
    }
}