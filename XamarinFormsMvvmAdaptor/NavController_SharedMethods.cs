using System;
using System.Threading.Tasks;

using Page = Xamarin.Forms.Page;
using Xamarin.Forms;
using System.Collections.Generic;

namespace XamarinFormsMvvmAdaptor
{
    ///<inheritdoc/>
    public partial class NavController : INavController
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

        NavigationPage root;
        public NavigationPage NavigationRoot
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
        public IAdaptorViewModel RootViewModel
        {
            get
            {
                try
                {
                    if (NavigationRoot is NavigationPage)
                        return ((NavigationPage)NavigationRoot).RootPage.BindingContext as IAdaptorViewModel;

                    return NavigationRoot.BindingContext as IAdaptorViewModel;
                }
                catch (NullReferenceException ex)
                {
                    throw new NullReferenceException($"{nameof(NavigationRoot)}'s BindingContext has not been set", ex);
                }
            }
        }
        ///<inheritdoc/>
        public IAdaptorViewModel TopViewModel
        {
            get
            {
                try { return TopPage.BindingContext as IAdaptorViewModel; }
                catch (NullReferenceException ex)
                {
                    throw new NullReferenceException($"{nameof(TopPage)}'s BindingContext has not been set", ex);
                }
            }
        }
        ///<inheritdoc/>
        public IAdaptorViewModel HiddenViewModel
        {
            get
            {
                if (HiddenPage is null)
                    return null;

                try { return HiddenPage.BindingContext as IAdaptorViewModel; }
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
                    $"{nameof(NavController)} is not initialized. Please run {nameof(InitAsync)} first.");
        }

        private async Task InitializeVmForPageAsync(Page page, object initialisationParameter, bool continueOnCapturedContext = false)
        {
            try
            {
                await (page.BindingContext as IAdaptorViewModel).InitializeAsync(initialisationParameter).ConfigureAwait(continueOnCapturedContext);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Check if your ViewModel is attached to the Page", ex);
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidCastException($"Check if your ViewModel implements {nameof(IAdaptorViewModel)}", ex);
            }
        }

        ///<inheritdoc/>
        public void RemovePreviousPageFromMainStack()
        {
            if (MainStack.Count > 1)
                NavigationRoot.Navigation.RemovePage(
                    MainStack[MainStack.Count - 2]);
        }

        ///<inheritdoc/>
        public async Task CollapseMainStack()
        {
            if (MainStack.Count == 1)
                return;

            while (MainStack.Count > 1)
            {
                NavigationRoot.Navigation.RemovePage(MainStack.GetPreviousPage());
            }

            if (ModalStack.Count == 0)
                await MainStack.GetCurrentViewModel().OnAppearingAsync().ConfigureAwait(false);
        }

        ///<inheritdoc/>
        public void RemovePageFor<TViewModel>() where TViewModel : IAdaptorViewModel
        {
            var pageType = GetPageTypeForViewModel(typeof(TViewModel));

            foreach (var item in MainStack)
            {
                if (item.GetType() == pageType)
                {
                    NavigationRoot.Navigation.RemovePage(item);
                    break;
                }
            }
        }

        ///<inheritdoc/>
        public async Task PopAsync(bool animated = true)
        {
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

            if (await isPoppedTcs.Task
                && ModalStack.Count == 0)
                    await MainStack.GetCurrentViewModel().OnAppearingAsync().ConfigureAwait(false);

        }

        ///<inheritdoc/>
        public async Task PopToRootAsync(bool animated = true)
        {
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

            if (await isPoppedTcs.Task
                && ModalStack.Count == 0)
                    await RootViewModel.OnAppearingAsync().ConfigureAwait(false);
        }

        ///<inheritdoc/>
        public async Task PopModalAsync(bool animated = true)
        {
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
                await TopViewModel.OnAppearingAsync().ConfigureAwait(false);
        }
    }
}