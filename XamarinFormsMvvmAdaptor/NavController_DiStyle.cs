using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public partial class NavController
    {
        public NavController(IBaseViewModel rootViewModel)
        {
            RootPage = InstantiatePage(rootViewModel.GetType());
            BindPageToViewModel(RootPage, rootViewModel);
        }

        private Task<Page> GetPageForPush(IBaseViewModel viewModel, object initialisationParameter)
        {
            if (Navigation is null)
                throw new RootPageNotSetException();

            return CreatePageAndInitializeVmFor(viewModel, initialisationParameter);
        }

        private async Task<Page> CreatePageAndInitializeVmFor(IBaseViewModel viewModel, object initialisationParameter = null)
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
        /// <see cref="BaseViewModel.InitializeAsync(object)"/> method</param>
        /// <returns></returns>
        public async Task InsertPageBefore<TViewModelExisting>(IBaseViewModel viewModel, object navigationData = null)
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
        public Task PushAsync(IBaseViewModel viewModel)
        {
            return PushAsync(viewModel, null, true);
        }

        /// <summary>
        /// Pushes a new page onto the stack
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="animated"></param>
        /// <returns></returns>
        public Task PushAsync(IBaseViewModel viewModel, bool animated)
        {
            return PushAsync(viewModel, null, animated);
        }

        /// <summary>
        /// Pushes a new page onto the stack
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="navigationData">Navigation data that will be passed to the
        /// <see cref="BaseViewModel.InitializeAsync(object)"/> method</param>
        /// <returns></returns>
        public Task PushAsync(IBaseViewModel viewModel, object navigationData)
        {
            return PushAsync(viewModel, navigationData, true);
        }

        /// <summary>
        /// Pushes a new page onto the stack
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="navigationData">Navigation data that will be passed to the
        /// <see cref="BaseViewModel.InitializeAsync(object)"/> method</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        public async Task PushAsync(IBaseViewModel viewModel, object navigationData, bool animated)
        {
            var page = await GetPageForPush(viewModel, navigationData).ConfigureAwait(false);
            await Navigation.PushAsync(page, animated).ConfigureAwait(false);
        }
        #endregion

        #region Modal Push
        /// <summary>
        /// Pushes a new modal page onto the modal stack
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public Task PushModalAsync(IBaseViewModel viewModel)
        {
            return PushModalAsync(viewModel, null, true);
        }

        /// <summary>
        /// Pushes a new modal page onto the modal stack
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="animated"></param>
        /// <returns></returns>
        public Task PushModalAsync(IBaseViewModel viewModel, bool animated)
        {
            return PushModalAsync(viewModel, null, animated);
        }

        /// <summary>
        /// Pushes a new modal page onto the modal stack
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="navigationData">Navigation data that will be passed to the
        /// <see cref="BaseViewModel.InitializeAsync(object)"/> method</param>
        /// <returns></returns>
        public Task PushModalAsync(IBaseViewModel viewModel, object navigationData)
        {
            return PushModalAsync(viewModel, navigationData, true);
        }

        /// <summary>
        /// Pushes a new modal page onto the modal stack
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="navigationData">Navigation data that will be passed to the
        /// <see cref="BaseViewModel.InitializeAsync(object)"/> method</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        public async Task PushModalAsync(IBaseViewModel viewModel, object navigationData, bool animated)
        {
            var page = await GetPageForPush(viewModel, navigationData).ConfigureAwait(false);
            await Navigation.PushModalAsync(page, animated).ConfigureAwait(false);
        }
        #endregion
        #endregion
    }
}
