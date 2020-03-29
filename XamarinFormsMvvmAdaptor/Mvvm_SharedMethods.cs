using System;
using System.Threading.Tasks;

using Page = Xamarin.Forms.Page;
using Xamarin.Forms;
using System.Collections.Generic;

namespace XamarinFormsMvvmAdaptor
{
    ///<inheritdoc/>
    public partial class Mvvm : IMvvm
    {
        #region Settings
        const string DEFAULT_VM_NAMESPACE = "ViewModels";
        const string DEFAULT_V_NAMESPACE = "Views";
        const string DEFAULT_VM_SUFFIX = "ViewModel";
        const string DEFAULT_V_SUFFIX = "Page";

        static string _viewModelSubNamespace = DEFAULT_VM_NAMESPACE;
        static string _viewSubNamespace = DEFAULT_V_NAMESPACE;
        static string _viewModelSuffix = DEFAULT_VM_SUFFIX;
        static string _viewSuffix = DEFAULT_V_SUFFIX;
        #endregion

        public IIoc IocLocal { get; } = new Ioc();

        Page root;
        public Page NavigationRoot
        {
            get
            {
                ThrowIfNotInitialized();
                return root;
            }
            private set => root = value;
        }

        ///<inheritdoc/>
        public bool IsInitialized { get; private set; }

        ///<inheritdoc/>
        public IReadOnlyList<Page> MainStack => NavigationRoot.Navigation.NavigationStack;
        ///<inheritdoc/>
        public IReadOnlyList<Page> ModalStack => NavigationRoot.Navigation.ModalStack;

        ///<inheritdoc/>
        public Page RootPage => MainStack[0];
        ///<inheritdoc/>
        public Page TopPage
        {
            get
            {
                ThrowIfNotInitialized();
                if (ModalStack.Count > 0)
                    return ModalStack[ModalStack.Count - 1];

                return MainStack[MainStack.Count - 1];
            }
        }
        ///<inheritdoc/>
        public Page HiddenPage
        {
            get
            {
                ThrowIfNotInitialized();
                if (ModalStack.Count > 0)
                {
                    if (ModalStack.Count > 1)
                        return ModalStack[ModalStack.Count - 2];

                    return MainStack[MainStack.Count - 1];
                }

                if (MainStack.Count > 1)
                    return MainStack[MainStack.Count - 2];

                return null;
            }
        }

        ///<inheritdoc/>
        public IMvvmViewModelBase RootViewModel
        {
            get
            {
                try
                {
                    if (NavigationRoot is NavigationPage)
                        return ((NavigationPage)NavigationRoot).RootPage.BindingContext as IMvvmViewModelBase;

                    return NavigationRoot.BindingContext as IMvvmViewModelBase;
                }
                catch (NullReferenceException ex)
                {
                    throw new NullReferenceException($"{nameof(NavigationRoot)}'s BindingContext has not been set", ex);
                }
            }
        }
        ///<inheritdoc/>
        public IMvvmViewModelBase TopViewModel
        {
            get
            {
                try
                {
                    return
                        (TopPage is NavigationPage
                        ? (TopPage as NavigationPage).CurrentPage.BindingContext
                        : TopPage.BindingContext
                        ) as IMvvmViewModelBase;
                    //return TopPage.BindingContext as IAdaptorViewModel;
                }
                catch (NullReferenceException ex)
                {
                    throw new NullReferenceException($"{nameof(TopPage)}'s BindingContext has not been set", ex);
                }
            }
        }
        ///<inheritdoc/>
        public IMvvmViewModelBase HiddenViewModel
        {
            get
            {
                if (HiddenPage is null)
                    return null;

                try { return HiddenPage.BindingContext as IMvvmViewModelBase; }
                catch (NullReferenceException ex)
                {
                    throw new NullReferenceException($"{nameof(HiddenPage)}'s BindingContext has not been set", ex);
                }
            }
        }

        private void ThrowIfNotInitialized()
        {
            if (!IsInitialized)
                throw new NotInitializedException(
                    $"{nameof(Mvvm)} is not initialized. Please run {nameof(InitAsync)} first.");
        }

        private async Task InitializeVmAsync(Page page, object initialisationParameter, bool continueOnCapturedContext = false)
        {
            try
            {
                await (page.BindingContext as IMvvmViewModelBase).OnViewPushedAsync(initialisationParameter).ConfigureAwait(continueOnCapturedContext);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Check if your ViewModel is attached to the Page", ex);
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidCastException($"Check if your ViewModel implements {nameof(IMvvmViewModelBase)}", ex);
            }
        }

        ///<inheritdoc/>
        public async Task RemovePreviousPageFromMainStack()
        {
            var removedViewModel = MainStack.GetPreviousViewModel();

            if (MainStack.Count > 1)
                NavigationRoot.Navigation.RemovePage(
                    MainStack.GetPreviousPage());

            await removedViewModel.OnViewRemovedAsync();
        }

        ///<inheritdoc/>
        public async Task<IMvvmViewModelBase> CollapseMainStack()
        {
            if (MainStack.Count > 1)
            {
                while (MainStack.Count > 1)
                {
                    var removedViewModel = MainStack.GetPreviousViewModel();
                    NavigationRoot.Navigation.RemovePage(MainStack.GetPreviousPage());
                    await removedViewModel.OnViewRemovedAsync();
                }

                if (ModalStack.Count == 0)
                    await MainStack.GetCurrentViewModel().RefreshStateAsync().ConfigureAwait(false);
            }
            return MainStack.GetCurrentViewModel();
        }

        ///<inheritdoc/>
        public async Task RemovePageFor<TViewModel>() where TViewModel : IMvvmViewModelBase
        {
            var pageType = GetPageTypeForViewModel(typeof(TViewModel));

            foreach (var item in MainStack)
            {
                if (item.GetType() == pageType)
                {
                    NavigationRoot.Navigation.RemovePage(item);
                    await (item.BindingContext as IMvvmViewModelBase).OnViewRemovedAsync();
                    break;
                }
            }
        }

        ///<inheritdoc/>
        public async Task<IMvvmViewModelBase> PopAsync(bool animated = true)
        {
            var poppedViewModel = MainStack.GetCurrentViewModel();

            var isPoppedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await NavigationRoot.Navigation.PopAsync(animated);
                    isPoppedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPoppedTcs.SetException(ex);
                }
            });

            if (await isPoppedTcs.Task)
            {
                await poppedViewModel.OnViewRemovedAsync();

                if (ModalStack.Count == 0)
                    await MainStack.GetCurrentViewModel().RefreshStateAsync().ConfigureAwait(false);
            }

            return MainStack.GetCurrentViewModel();
        }

        ///<inheritdoc/>
        public async Task<IMvvmViewModelBase> PopToRootAsync(bool animated = true)
        {
            var poppedViewModel = MainStack.GetCurrentViewModel();

            var isPoppedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await NavigationRoot.Navigation.PopToRootAsync(animated);
                    isPoppedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPoppedTcs.SetException(ex);
                }
            });

            if (await isPoppedTcs.Task)
            {
                await poppedViewModel.OnViewRemovedAsync();

                if (ModalStack.Count == 0)
                    await RootViewModel.RefreshStateAsync().ConfigureAwait(false);
            }

            return RootViewModel;
        }

        ///<inheritdoc/>
        public async Task<IMvvmViewModelBase> PopModalAsync(bool animated = true)
        {
            var poppedViewModel = ModalStack.GetCurrentViewModel();

            var isPoppedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await NavigationRoot.Navigation.PopModalAsync(animated);
                    isPoppedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPoppedTcs.SetException(ex);
                }
            });

            if (await isPoppedTcs.Task)
            {
                await poppedViewModel.OnViewRemovedAsync();
                await TopViewModel.RefreshStateAsync().ConfigureAwait(false);
            }

            return TopViewModel;
        }
    }
}