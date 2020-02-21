using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    /// <inheritdoc cref="NavController"/>
    public interface INavController
    {
        /// <inheritdoc cref="NavController.IsInitialized"/>
        bool IsInitialized { get; }

        /// <inheritdoc cref="NavController.NavigationStack"/>
        IReadOnlyList<Page> NavigationStack { get; }

        /// <inheritdoc cref="NavController.ModalStack"/>
        IReadOnlyList<Page> ModalStack { get; }

        /// <inheritdoc cref="NavController.RootPage"/>
        Page RootPage { get; }

        /// <inheritdoc cref="NavController.TopPage"/>
        Page TopPage { get; }

        /// <inheritdoc cref="NavController.HiddenPage"/>
        Page HiddenPage { get; }

        /// <inheritdoc cref="NavController.RootViewModel"/>
        IAdaptorViewModel RootViewModel { get; }

        /// <inheritdoc cref="NavController.TopViewModel"/>
        IAdaptorViewModel TopViewModel { get; }

        /// <inheritdoc cref="NavController.HiddenViewModel"/>
        IAdaptorViewModel HiddenViewModel { get; }

        /// <inheritdoc cref="NavController.CollapseStack"/>
        void CollapseStack();

        /// <inheritdoc cref="NavController.InitAsync(Page)"/>/>
        Task InitAsync(Page rootPage);

        /// <inheritdoc cref="NavController.InitAsync(Page,object)"/>/>
        Task InitAsync(Page rootPage, object initialisationData);

        /// <inheritdoc cref="NavController.InitAsync(IAdaptorViewModel)"/>/>
        Task InitAsync(IAdaptorViewModel rootViewModel);

        /// <inheritdoc cref="NavController.InitAsync(IAdaptorViewModel,object)"/>/>
        Task InitAsync(IAdaptorViewModel rootViewModel, object initialisationData);

        /// <inheritdoc cref="NavController.InsertPageBefore{TViewModelExisting, TViewModelNew}(object)"/>
        Task InsertPageBefore<TViewModelExisting, TViewModelNew>(object navigationData = null);

        /// <inheritdoc cref="NavController.InsertPageBefore{TViewModelExisting}(IAdaptorViewModel, object)"/>
        Task InsertPageBefore<TViewModelExisting>(IAdaptorViewModel viewModel, object navigationData = null);

        /// <inheritdoc cref="NavController.PopAsync()"/>
        Task PopAsync();

        /// <inheritdoc cref="NavController.PopAsync(bool)"/>
        Task PopAsync(bool animated);

        /// <inheritdoc cref="NavController.PopModalAsync()"/>
        Task PopModalAsync();

        /// <inheritdoc cref="NavController.PopModalAsync(bool)"/>
        Task PopModalAsync(bool animated);

        /// <inheritdoc cref="NavController.PopToRootAsync()"/>
        Task PopToRootAsync();

        /// <inheritdoc cref="NavController.PopToRootAsync(bool)"/>
        Task PopToRootAsync(bool animated);

        /// <inheritdoc cref="NavController.PushAsync{TViewModel}()"/>
        Task PushAsync<TViewModel>();

        /// <inheritdoc cref="NavController.PushAsync{TViewModel}(object)"/>
        Task PushAsync<TViewModel>(object navigationData);

        /// <inheritdoc cref="NavController.PushAsync{TViewModel}(bool)"/>
        Task PushAsync<TViewModel>(bool animated);

        /// <inheritdoc cref="NavController.PushAsync{TViewModel}(object, bool)"/>
        Task PushAsync<TViewModel>(object navigationData, bool animated);

        /// <inheritdoc cref="NavController.PushAsync(IAdaptorViewModel)"/>
        Task PushAsync(IAdaptorViewModel viewModel);

        /// <inheritdoc cref="NavController.PushAsync(IAdaptorViewModel, object)"/>
        Task PushAsync(IAdaptorViewModel viewModel, object navigationData);

        /// <inheritdoc cref="NavController.PushAsync(IAdaptorViewModel, bool)"/>
        Task PushAsync(IAdaptorViewModel viewModel, bool animated);

        /// <inheritdoc cref="NavController.PushAsync(IAdaptorViewModel, object, bool)"/>
        Task PushAsync(IAdaptorViewModel viewModel, object navigationData, bool animated);

        /// <inheritdoc cref="NavController.PushModalAsync{TViewModel}()"/>
        Task PushModalAsync<TViewModel>();

        /// <inheritdoc cref="NavController.PushModalAsync{TViewModel}(object)"/>
        Task PushModalAsync<TViewModel>(object navigationData);

        /// <inheritdoc cref="NavController.PushModalAsync{TViewModel}(bool)"/>
        Task PushModalAsync<TViewModel>(bool animated);

        /// <inheritdoc cref="NavController.PushModalAsync(IAdaptorViewModel, object, bool)"/>
        Task PushModalAsync<TViewModel>(object navigationData, bool animated);

        /// <inheritdoc cref="NavController.PushModalAsync(IAdaptorViewModel)"/>
        Task PushModalAsync(IAdaptorViewModel viewModel);

        /// <inheritdoc cref="NavController.PushModalAsync(IAdaptorViewModel, object)"/>
        Task PushModalAsync(IAdaptorViewModel viewModel, object navigationData);

        /// <inheritdoc cref="NavController.PushModalAsync(IAdaptorViewModel, bool)"/>
        Task PushModalAsync(IAdaptorViewModel viewModel, bool animated);

        /// <inheritdoc cref="NavController.PushModalAsync(IAdaptorViewModel, object, bool)"/>
        Task PushModalAsync(IAdaptorViewModel viewModel, object navigationData, bool animated);

        /// <inheritdoc cref="NavController.RemoveHiddenPageFromStack"/>
        void RemoveHiddenPageFromStack();

        /// <inheritdoc cref="NavController.RemovePageFor{TViewModel}"/>
        void RemovePageFor<TViewModel>() where TViewModel : IAdaptorViewModel;
    }
}