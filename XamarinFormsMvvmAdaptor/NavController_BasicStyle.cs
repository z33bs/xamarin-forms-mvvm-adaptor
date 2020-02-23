using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    ///<inheritdoc/>
    public partial class NavController : INavController
    {
        ///<inheritdoc/>
        public async Task InitAsync(Page rootPage, object initialisationData = null)
        {
            Root = new NavigationPage(rootPage);
            try
            {
                IsInitialized = true;
                await RootViewModel.InitializeAsync(initialisationData).ConfigureAwait(false);
                await RootViewModel.OnAppearingAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                IsInitialized = false;
                throw new NotInitializedException("Initialization failed", ex);
            }
        }

        ///<inheritdoc/>
        public async Task InsertPageBefore<TViewModelExisting, TViewModelNew>(object navigationData = null) where TViewModelNew : IAdaptorViewModel
        {
            var newPage = InstantiatePage(typeof(TViewModelNew));

            Type pageTypeAnchorPage = GetPageTypeForViewModel(typeof(TViewModelExisting));

            foreach (var existingPage in MainStack)
            {
                if (existingPage.GetType() == pageTypeAnchorPage)
                {
                    Root.Navigation.InsertPageBefore(existingPage, newPage);
                    await InitializeVmForPageAsync(newPage, navigationData).ConfigureAwait(false);
                }
            }
        }

        ///<inheritdoc/>
        public async Task PushAsync<TViewModel>(object navigationData = null, bool animated = true) where TViewModel : IAdaptorViewModel
        {
            var page = InstantiatePage(typeof(TViewModel));

            var isPushedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Root.Navigation.PushAsync(page, animated);
                    isPushedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPushedTcs.SetException(ex);
                }
            });

            if (await isPushedTcs.Task)
            {
                await InitializeVmForPageAsync(page, navigationData).ConfigureAwait(false);
                await TopViewModel.OnAppearingAsync().ConfigureAwait(false);
            }
        }

        ///<inheritdoc/>
        public async Task PushModalAsync<TViewModel>(object navigationData = null, bool animated = true) where TViewModel : IAdaptorViewModel
        {
            var page = InstantiatePage(typeof(TViewModel));

            var isPushedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Root.Navigation.PushModalAsync(page, animated).ConfigureAwait(false);
                    isPushedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPushedTcs.SetException(ex);
                }
            });

            if (await isPushedTcs.Task)
            {
                await InitializeVmForPageAsync(page, navigationData).ConfigureAwait(false);
                await TopViewModel.OnAppearingAsync().ConfigureAwait(false);
            }
        }

    }
}
