using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public partial class NavController
    {
        public NavController(IMvvmAdaptorViewModel viewModel)
        {
            RootPage = InstantiatePage(viewModel.GetType());
            BindPageToViewModel(RootPage, viewModel);
        }

        private Task<Page> GetPageForPush(IMvvmAdaptorViewModel viewModel, object initialisationParameter)
        {
            if (Navigation is null)
                throw new RootPageNotSetException();

            return CreatePageAndInitializeVmFor(viewModel, initialisationParameter);
        }

        private async Task<Page> CreatePageAndInitializeVmFor(IMvvmAdaptorViewModel viewModel, object initialisationParameter = null)
        {
            await viewModel.InitializeAsync(initialisationParameter).ConfigureAwait(false);

            var page = InstantiatePage(viewModel.GetType());
            BindPageToViewModel(page, viewModel);

            return page;
        }



        #region Forms.INavigation Adaptation
        public async Task InsertPageBefore<TViewModelBefore>(IMvvmAdaptorViewModel viewModel, object initialisationParameter)
        {
            var page = await CreatePageAndInitializeVmFor(viewModel.GetType(), initialisationParameter).ConfigureAwait(false);
            BindPageToViewModel(page, viewModel);

            var pageTypeOfPageBefore = GetPageTypeForViewModel(typeof(TViewModelBefore));

            foreach (var item in RootPage.Navigation.NavigationStack)
            {
                if (item.GetType() == pageTypeOfPageBefore)
                {
                    RootPage.Navigation.InsertPageBefore(page, item);
                }
            }
        }

        #region Push
        public Task PushAsync(IMvvmAdaptorViewModel viewModel)
        {
            return PushAsync(viewModel, null, true);
        }

        public Task PushAsync(IMvvmAdaptorViewModel viewModel, bool animated)
        {
            return PushAsync(viewModel, null, animated);
        }

        public Task PushAsync(IMvvmAdaptorViewModel viewModel, object initialisationParameter)
        {
            return PushAsync(viewModel, initialisationParameter, true);
        }

        public async Task PushAsync(IMvvmAdaptorViewModel viewModel, object initialisationParameter, bool animated)
        {
            var page = await GetPageForPush(viewModel, initialisationParameter).ConfigureAwait(false);
            await Navigation.PushAsync(page, animated).ConfigureAwait(false);
        }
        #endregion

        #region Modal Push
        public Task PushModalAsync(IMvvmAdaptorViewModel viewModel)
        {
            return PushModalAsync(viewModel, null, true);
        }

        public Task PushModalAsync(IMvvmAdaptorViewModel viewModel, bool animated)
        {
            return PushModalAsync(viewModel, null, animated);
        }

        public Task PushModalAsync(IMvvmAdaptorViewModel viewModel, object initialisationParameter)
        {
            return PushModalAsync(viewModel, initialisationParameter, true);
        }

        public async Task PushModalAsync(IMvvmAdaptorViewModel viewModel, object initialisationParameter, bool animated)
        {
            var page = await GetPageForPush(viewModel, initialisationParameter).ConfigureAwait(false);
            await Navigation.PushModalAsync(page, animated).ConfigureAwait(false);
        }
        #endregion
        #endregion
    }
}
