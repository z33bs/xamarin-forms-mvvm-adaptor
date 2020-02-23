using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public partial class NavController
    {

        /// <summary>
        /// Set the <see cref="Roott""/>, and initialize its ViewModel,
        /// running the <see cref="IAdaptorViewModel.InitializeAsync(object)"/>
        /// and <see cref="IAdaptorViewModel.OnAppearingAsync"/> methods.
        /// </summary>
        /// <returns></returns>
        public async Task InitAsync(Page rootPage, object initialisationData = null)
        {
            Roott = new NavigationPage(rootPage);
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

            foreach (var existingPage in MainStack)
            {
                if (existingPage.GetType() == pageTypeAnchorPage)
                {
                    Roott..Navigation.InsertPageBefore(existingPage, newPage);
                    await InitializeVmForPageAsync(newPage, navigationData).ConfigureAwait(false);
                }
            }
        }

        #region Push
        /// <summary>
        /// Pushes a new page onto the stack given a <typeparamref name="TViewModel"/>.
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel to Push</typeparam>
        /// <param name="navigationData">Optional navigation data to be passed to the <see cref="AdaptorViewModel.InitializeAsync(object)"/>  method of the <typeparamref name="TViewModel"/></param>
        /// <param name="animated">Option whether to animate the push or not</param>
        /// <returns></returns>
        public async Task PushAsync<TViewModel>(object navigationData = null, bool animated = true) where TViewModel : IAdaptorViewModel
        {
            var page = InstantiatePage(typeof(TViewModel));

            var isPushedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Roott..Navigation.PushAsync(page, animated);
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
        /// <param name="navigationData">Object to be passed to the <see cref="AdaptorViewModel.InitializeAsync(object)"/>  method</param>
        /// <param name="animated">Animate the Push</param>
        /// <returns></returns>
        public async Task PushModalAsync<TViewModel>(object navigationData, bool animated) where TViewModel : IAdaptorViewModel
        {
            var page = InstantiatePage(typeof(TViewModel));

            var isPushedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Roott..Navigation.PushModalAsync(page, animated).ConfigureAwait(false);
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
