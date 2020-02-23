using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// MvvM Navigation Controller. Each instance contains its own Navigation stack.
    /// Navigation engine depends on the user sticking to a naming convention:
    /// Views are expected to be in the <see cref="NavController.DEFAULT_V_NAMESPACE"/> subnamespace
    /// and end with suffix <see cref="NavController.DEFAULT_V_SUFFIX"/>.
    /// ViewModels are expected to be in the <see cref="NavController.DEFAULT_VM_NAMESPACE"/> subnamespace
    /// and end with suffix <see cref="NavController.DEFAULT_VM_SUFFIX"/>.
    /// This behaviour can be customised with <see cref="NavController.SetNamingConventions(string, string, string, string)"/>
    /// </summary>
    public interface INavController
    {
        #region Properties
        /// <summary>
        /// Returns true if InitAsync has run successfully
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets the stack of pages in the navigation
        /// </summary>
        IReadOnlyList<Page> MainStack { get; }

        /// <summary>
        /// Gets the modal navigation stack. The <see cref="ModalStack"/> always hides the <see cref="MainStack"/>
        /// </summary>
        IReadOnlyList<Page> ModalStack { get; }

        /// <summary>
        /// Page at the root/bottom of the <see cref="MainStack"/>
        /// </summary>
        Page RootPage { get; }

        /// <summary>
        /// Currently visible <see cref="Page"/> at the top of the <see cref="MainStack"/>
        /// or <see cref="ModalStack"/> if it has any <see cref="Page"/>s in it
        /// </summary>
        Page TopPage { get; }

        /// <summary>
        /// <see cref="Page"/> beneath the <see cref="TopPage"/>
        /// It will be the page that appears when the currently visible
        /// <see cref="Page"/> is Popped.
        /// Returns null if <see cref="TopPage"/> is the <see cref="RootPage"/>
        /// </summary>
        Page HiddenPage { get; }

        /// <summary>
        /// Viewmodel corresponding to the <see cref="RootPage"/>
        /// <exception><see cref="System.NullReferenceException"/> is thrown when the <see cref="RootPage"/>s
        /// BindingContext has not been set.</exception>
        /// </summary>
        IAdaptorViewModel RootViewModel { get; }

        /// <summary>
        /// Viewmodel corresponding to the <see cref="TopPage"/>
        /// <exception><see cref="System.NullReferenceException"/> is thrown when the <see cref="TopPage"/>s
        /// BindingContext has not been set.</exception>
        /// </summary>
        IAdaptorViewModel TopViewModel { get; }

        /// <summary>
        /// Viewmodel corresponding to the <see cref="HiddenPage"/>
        /// Returns null if <see cref="TopPage"/> is the <see cref="RootPage"/>
        /// <exception><see cref="System.NullReferenceException"/> is thrown when the <see cref="HiddenPage"/>s
        /// BindingContext has not been set.</exception>
        /// </summary>
        IAdaptorViewModel HiddenViewModel { get; }
        #endregion

        #region Vanilla Implementation
        /// <summary>
        /// Set the <see cref="RootPage"/>, and initialize its ViewModel,
        /// running the <see cref="IAdaptorViewModel.InitializeAsync(object)"/>
        /// and <see cref="IAdaptorViewModel.OnAppearingAsync"/> methods.
        /// </summary>
        /// <returns></returns>
        Task InitAsync(Page rootPage, object initialisationData = null);

        /// <summary>
        /// Pushes a new page onto the stack given a <typeparamref name="TViewModel"/>.
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel to Push</typeparam>
        /// <param name="navigationData">Optional navigation data to be passed to the <see cref="AdaptorViewModel.InitializeAsync(object)"/>  method of the <typeparamref name="TViewModel"/></param>
        /// <param name="animated">Option whether to animate the push or not</param>
        /// <returns></returns>
        Task PushAsync<TViewModel>(object navigationData = null, bool animated = true) where TViewModel : IAdaptorViewModel;

        /// <summary>
        /// Pushes a new Modal page onto the navigation stack
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="navigationData">Object to be passed to the <see cref="AdaptorViewModel.InitializeAsync(object)"/>  method</param>
        /// <param name="animated">Animate the Push</param>
        /// <returns></returns>
        Task PushModalAsync<TViewModel>(object navigationData = null, bool animated = true) where TViewModel : IAdaptorViewModel;

        /// <summary>
        /// Inserts a page in the navigation stack before
        /// the page associated with the given ViewModel
        /// </summary>
        /// <typeparam name="TViewModelExisting"></typeparam>
        /// <typeparam name="TViewModelNew"></typeparam>
        /// <param name="navigationData">Optional navigation data that will be passed to
        /// <see cref="AdaptorViewModel.InitializeAsync(object)"/></param>
        /// <returns></returns>
        Task InsertPageBefore<TViewModelExisting, TViewModelNew>(object navigationData = null) where TViewModelNew : IAdaptorViewModel;
        #endregion

        #region Di Implementation
        /// <inheritdoc cref="InitAsync(Page, object)"/>
        Task DiInitAsync(IAdaptorViewModel rootViewModel, object initialisationData = null);

        /// <summary>
        /// Pushes a new page onto the stack
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="navigationData">Navigation data that will be passed to the
        /// <see cref="AdaptorViewModel.InitializeAsync(object)"/> method</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task DiPushAsync(IAdaptorViewModel viewModel, object navigationData = null, bool animated = true);

        /// <summary>
        /// Pushes a new modal page onto the modal stack
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="navigationData">Navigation data that will be passed to the
        /// <see cref="AdaptorViewModel.InitializeAsync(object)"/> method</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task DiPushModalAsync(IAdaptorViewModel viewModel, object navigationData = null, bool animated = true);

        /// <summary>
        /// Inserts a page in the navigation stack before
        /// the page associated with the given ViewModel
        /// </summary>
        /// <typeparam name="TViewModelExisting"></typeparam>
        /// <param name="viewModel"></param>
        /// <param name="navigationData">Optional navigation data that will be passed to the
        /// <see cref="AdaptorViewModel.InitializeAsync(object)"/> method</param>
        /// <returns></returns>
        Task DiInsertPageBefore<TViewModelExisting>(IAdaptorViewModel viewModel, object navigationData = null);

        /// <summary>
        /// Asynchronously removes the <see cref="TopPage" /> from the navigation stack, with optional animation.
        /// </summary>
        /// <param name="animated">Whether to animate the pop.</param>
        Task PopAsync(bool animated = true);

        /// <summary>
        /// Asynchronously dismisses the most recent modally presented <see cref="Page" />, with optional animation.
        /// </summary>
        /// <param name="animated">Whether to animate the pop.</param>
        Task PopModalAsync(bool animated = true);

        /// <summary>
        /// Pops the entire <see cref="MainStack"/>, leaving only the <see cref="RootPage"/>, with optional animation.
        /// </summary>
        /// <param name="animated">Whether to animate the pop.</param>
        /// <returns></returns>
        Task PopToRootAsync(bool animated = true);

        /// <summary>
        /// Removes a <see cref="Page"/> from the <see cref="MainStack"/>
        /// that corresponds to a given ViewModel
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        void RemovePageFor<TViewModel>() where TViewModel : IAdaptorViewModel;
        #endregion

        #region Additional Shared Methods

        /// <summary>
        /// Removes all pages in the <see cref="MainStack"/> except for
        /// the <see cref="TopPage"/>, which becomes the <see cref="RootPage"/> of the stack.
        /// </summary>
        void CollapseMainStack();

        /// <summary>
        /// Removes the <see cref="HiddenPage"/> from the <see cref="MainStack"/> but not the <see cref="ModalStack"/>
        /// </summary>
        /// <returns></returns>
        void RemovePreviousPageFromMainStack();
        #endregion
    }
}