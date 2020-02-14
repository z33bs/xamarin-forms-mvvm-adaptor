using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public partial class NavController
    {
        public NavController(Page rootPage, bool isWrappedInNavigationPage = true)
        {
            if (isWrappedInNavigationPage)
                RootPage = new NavigationPage(rootPage);
            else
                RootPage = rootPage;
        }

        //private Task<Page> GetPageForPush<TViewModel>()
        //{
        //    if (Navigation is null)
        //        throw new RootPageNotSetException();

        //    return InstantiatePage(typeof(TViewModel));
        //}


        //private Task<Page> CreatePageAndInitializeVmFor(Type viewModelType, object initialisationParameter = null)
        //{
        //    var page = InstantiatePage(viewModelType);
        //    //Vm is autoWired in your page class

        //    //await InitializeVmForPage(page, initialisationParameter).ConfigureAwait(false);

        //    return page;
        //}

        #region Navigation Helpers
        /// <summary>
        /// Instantiates a page associated with the given ViewModel
        /// </summary>
        /// <typeparam name="ViewModelType"></typeparam>
        /// <returns></returns>
        public static Page CreatePageForAsync<ViewModelType>()
        {
            return InstantiatePage(typeof(ViewModelType));
        }
        #endregion

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
            //var page = await CreatePageAndInitializeVmFor(typeof(TViewModelNew), navigationData).ConfigureAwait(false);
            var page = InstantiatePage(typeof(TViewModelNew));

            Type pageTypeOfPageBefore = GetPageTypeForViewModel(typeof(TViewModelExisting));

            foreach (var item in RootPage.Navigation.NavigationStack)
            {
                if (item.GetType() == pageTypeOfPageBefore)
                {
                    RootPage.Navigation.InsertPageBefore(page, item);
                    await InitializeVmForPage(page, navigationData).ConfigureAwait(false);
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
            Device.BeginInvokeOnMainThread(
                async () =>
                    await Navigation.PushAsync(page, animated));
            await InitializeVmForPage(page, navigationData).ConfigureAwait(false);
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
            Device.BeginInvokeOnMainThread(
                async () =>
                    await Navigation.PushModalAsync(page, animated).ConfigureAwait(false));
            await InitializeVmForPage(page, navigationData).ConfigureAwait(false);
        }
        #endregion
        #endregion
    }
}
