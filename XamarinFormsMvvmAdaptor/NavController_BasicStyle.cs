using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public partial class NavController
    {
        /// <summary>
        /// Constructs the <see cref="NavController"/> with the given <see cref="RootPage"/>
        /// </summary>
        /// <param name="rootPage"></param>
        /// <param name="isWrappedInNavigationPage">If true then the
        /// <see cref="RootPage"/> will be wrapped in a <see cref="NavigationPage"/></param>
        public NavController(Page rootPage, bool isWrappedInNavigationPage = true)
        {
            if (isWrappedInNavigationPage)
                RootPage = new NavigationPage(rootPage);
            else
                RootPage = rootPage;
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
