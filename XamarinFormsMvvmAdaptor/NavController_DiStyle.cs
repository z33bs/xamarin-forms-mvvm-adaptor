using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public partial class NavController
    {
        public NavController(IAdaptorViewModel rootViewModel, bool isWrappedInNavigationPage = true)
        {
            var page = InstantiatePage(rootViewModel.GetType());
            BindPageToViewModel(page, rootViewModel);
            if (isWrappedInNavigationPage)
                RootPage = new NavigationPage(page);
            else
                RootPage = page;
        }

        private Task<Page> GetPageForPush(IAdaptorViewModel viewModel, object initialisationParameter)
        {
            if (Navigation is null)
                throw new RootPageNotSetException();

            return CreatePageAndInitializeVmFor(viewModel, initialisationParameter);
        }

        private async Task<Page> CreatePageAndInitializeVmFor(IAdaptorViewModel viewModel, object initialisationParameter = null)
        {
            await viewModel.InitializeAsync(initialisationParameter).ConfigureAwait(false);

            var page = InstantiatePage(viewModel.GetType());
            BindPageToViewModel(page, viewModel);

            return page;
        }



        #region Forms.INavigation Adaptation
        /// <summary>
        /// Inserts a page in the navigation stack before
        /// the page associated with the given ViewModel
        /// </summary>
        /// <typeparam name="TViewModelExisting"></typeparam>
        /// <param name="viewModel"></param>
        /// <param name="navigationData">Optional navigation data that will be passed to the
        /// <see cref="AdaptorViewModel.InitializeAsync(object)"/> method</param>
        /// <returns></returns>
        public async Task InsertPageBefore<TViewModelExisting>(IAdaptorViewModel viewModel, object navigationData = null)
        {
            var page = await CreatePageAndInitializeVmFor(viewModel.GetType(), navigationData).ConfigureAwait(false);
            BindPageToViewModel(page, viewModel);

            var pageTypeOfPageBefore = GetPageTypeForViewModel(typeof(TViewModelExisting));

            foreach (var item in RootPage.Navigation.NavigationStack)
            {
                if (item.GetType() == pageTypeOfPageBefore)
                {
                    RootPage.Navigation.InsertPageBefore(page, item);
                }
            }
        }

        #region Push
        /// <summary>
        /// Pushes a new page onto the stack
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public Task PushAsync(IAdaptorViewModel viewModel)
        {
            return PushAsync(viewModel, null, true);
        }

        /// <summary>
        /// Pushes a new page onto the stack
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="animated"></param>
        /// <returns></returns>
        public Task PushAsync(IAdaptorViewModel viewModel, bool animated)
        {
            return PushAsync(viewModel, null, animated);
        }

        /// <summary>
        /// Pushes a new page onto the stack
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="navigationData">Navigation data that will be passed to the
        /// <see cref="AdaptorViewModel.InitializeAsync(object)"/> method</param>
        /// <returns></returns>
        public Task PushAsync(IAdaptorViewModel viewModel, object navigationData)
        {
            return PushAsync(viewModel, navigationData, true);
        }

        /// <summary>
        /// Pushes a new page onto the stack
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="navigationData">Navigation data that will be passed to the
        /// <see cref="AdaptorViewModel.InitializeAsync(object)"/> method</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        public async Task PushAsync(IAdaptorViewModel viewModel, object navigationData, bool animated)
        {
            var page = await GetPageForPush(viewModel, navigationData).ConfigureAwait(false);
            Device.BeginInvokeOnMainThread(
                async () =>
                    await Navigation.PushAsync(page, animated).ConfigureAwait(false));
        }
        #endregion

        #region Modal Push
        /// <summary>
        /// Pushes a new modal page onto the modal stack
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public Task PushModalAsync(IAdaptorViewModel viewModel)
        {
            return PushModalAsync(viewModel, null, true);
        }

        /// <summary>
        /// Pushes a new modal page onto the modal stack
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="animated"></param>
        /// <returns></returns>
        public Task PushModalAsync(IAdaptorViewModel viewModel, bool animated)
        {
            return PushModalAsync(viewModel, null, animated);
        }

        /// <summary>
        /// Pushes a new modal page onto the modal stack
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="navigationData">Navigation data that will be passed to the
        /// <see cref="AdaptorViewModel.InitializeAsync(object)"/> method</param>
        /// <returns></returns>
        public Task PushModalAsync(IAdaptorViewModel viewModel, object navigationData)
        {
            return PushModalAsync(viewModel, navigationData, true);
        }

        /// <summary>
        /// Pushes a new modal page onto the modal stack
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="navigationData">Navigation data that will be passed to the
        /// <see cref="AdaptorViewModel.InitializeAsync(object)"/> method</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        public async Task PushModalAsync(IAdaptorViewModel viewModel, object navigationData, bool animated)
        {
            var page = await GetPageForPush(viewModel, navigationData).ConfigureAwait(false);
            Device.BeginInvokeOnMainThread(
                async () =>
                    await Navigation.PushModalAsync(page, animated).ConfigureAwait(false));
        }
        #endregion
        #endregion
    }
}
