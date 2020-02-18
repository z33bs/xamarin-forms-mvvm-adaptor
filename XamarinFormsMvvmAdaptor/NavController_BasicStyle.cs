using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public partial class NavController
    {
        /// <summary>
        /// Set the <see cref="RootPage"/>, and initialize its ViewModel,
        /// running the <see cref="IAdaptorViewModel.InitializeAsync(object)"/>
        /// and <see cref="IAdaptorViewModel.OnAppearingAsync"/> methods.
        /// </summary>
        /// <returns></returns>
        public Task InitAsync(Page rootPage, bool isWrappedInNavigationPage = true)
        {
            return InitAsync(rootPage, null, isWrappedInNavigationPage);
        }

        /// <inheritdoc cref="InitAsync(Page,bool)"/>
        public async Task InitAsync(Page rootPage, object initialisationData, bool isWrappedInNavigationPage = true)
        {
            if (isWrappedInNavigationPage)
                RootPage = new NavigationPage(rootPage);
            else
                RootPage = rootPage;
            try
            {
                IsInitialized = true;
                await RootViewModel.InitializeAsync(initialisationData).ConfigureAwait(false);
                await RootViewModel.OnAppearingAsync().ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                IsInitialized = false;
                throw new NotInitializedException("Initialization failed", ex);
            }
        }

        #region Forms.INavigation Adaptation
        /// <summary>
        /// Inserts a page in the navigation stack before
        /// the page associated with the given ViewModel
        /// </summary>
        /// <typeparam name="TViewModelExisting"></typeparam>
        /// <typeparam name="TViewModelNew"></typeparam>
        /// <param name="navigationData">Optional navigation data that will be passed to
        /// <see cref="AdaptorViewModel.InitializeAsync(object)"/></param>
        /// <returns></returns>
        public async Task InsertPageBefore<TViewModelExisting, TViewModelNew>(object navigationData = null)
        {
            var newPage = InstantiatePage(typeof(TViewModelNew));

            Type pageTypeAnchorPage = GetPageTypeForViewModel(typeof(TViewModelExisting));

            foreach (var existingPage in NavigationStack)
            {
                if (existingPage.GetType() == pageTypeAnchorPage)
                {
                    RootPage.Navigation.InsertPageBefore(existingPage, newPage);
                    await InitializeVmForPageAsync(newPage, navigationData).ConfigureAwait(false);
                }
            }
        }

        #region Push
        /// <summary>
        /// Pushes a new page onto the stack
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <returns></returns>
        public Task PushAsync<TViewModel>()
        {
            return PushAsync<TViewModel>(null, true);
        }

        /// <summary>
        /// Pushes a new page onto the stack
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="animated"></param>
        /// <returns></returns>
        public Task PushAsync<TViewModel>(bool animated)
        {
            return PushAsync<TViewModel>(null, animated);
        }

        /// <summary>
        /// Pushes a new page onto the stack
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="navigationData">Object to be passed to the <see cref="AdaptorViewModel.InitializeAsync(object)"/>  method</param>
        /// <returns></returns>
        public Task PushAsync<TViewModel>(object navigationData)
        {
            return PushAsync<TViewModel>(navigationData, true);
        }
        /// <summary>
        /// Pushes a new page onto the stack
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="navigationData">Object to be passed to the <see cref="AdaptorViewModel.InitializeAsync(object)"/>  method</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        public async Task PushAsync<TViewModel>(object navigationData, bool animated)
        {
            var page = InstantiatePage(typeof(TViewModel));

            var isPushedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await RootPage.Navigation.PushAsync(page, animated);
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
        #endregion

        #region Modal Push
        /// <summary>
        /// Pushes a new Modal page onto the navigation stack
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <returns></returns>
        public Task PushModalAsync<TViewModel>()
        {
            return PushModalAsync<TViewModel>(null, true);
        }

        /// <summary>
        /// Pushes a new Modal page onto the navigation stack
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="animated"></param>
        /// <returns></returns>
        public Task PushModalAsync<TViewModel>(bool animated)
        {
            return PushModalAsync<TViewModel>(null, animated);
        }

        /// <summary>
        /// Pushes a new Modal page onto the navigation stack
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="navigationData">Object to be passed to the <see cref="AdaptorViewModel.InitializeAsync(object)"/>  method</param>
        /// <returns></returns>
        public Task PushModalAsync<TViewModel>(object navigationData)
        {
            return PushModalAsync<TViewModel>(navigationData, true);
        }

        /// <summary>
        /// Pushes a new Modal page onto the navigation stack
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="navigationData">Object to be passed to the <see cref="AdaptorViewModel.InitializeAsync(object)"/>  method</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        public async Task PushModalAsync<TViewModel>(object navigationData, bool animated)
        {
            var page = InstantiatePage(typeof(TViewModel));

            var isPushedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await RootPage.Navigation.PushModalAsync(page, animated).ConfigureAwait(false);
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
        #endregion
        #endregion
    }
}
