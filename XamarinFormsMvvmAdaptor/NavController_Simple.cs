using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public partial class NavController
    {
        public NavController(Page rootPage)
        {
            RootPage = rootPage;
        }

        private Task<Page> GetPageForPush<TViewModel>(object initialisationParameter)
        {
            if (Navigation is null)
                throw new RootPageNotSetException();

            return CreatePageAndInitializeVmFor(typeof(TViewModel), initialisationParameter);
        }


        private async Task<Page> CreatePageAndInitializeVmFor(Type viewModelType, object initialisationParameter = null)
        {
            var page = InstantiatePage(viewModelType);
            //Vm is autoWired in your page class
            await InitializeVmForPage(page, initialisationParameter).ConfigureAwait(false);

            return page;
        }

        #region Navigation Helpers
        public static Page CreatePageForAsync<ViewModelType>()
        {
            return InstantiatePage(typeof(ViewModelType));
        }
        #endregion

        #region Forms.INavigation Adaptation
        public async Task InsertPageBefore<TViewModelBefore, TViewModel>(object initialisationParameter)
        {
            var page = await CreatePageAndInitializeVmFor(typeof(TViewModel), initialisationParameter).ConfigureAwait(false);

            Type pageTypeOfPageBefore = GetPageTypeForViewModel(typeof(TViewModelBefore));

            foreach (var item in RootPage.Navigation.NavigationStack)
            {
                if (item.GetType() == pageTypeOfPageBefore)
                {
                    RootPage.Navigation.InsertPageBefore(page, item);
                }
            }
        }

        #region Push
        public Task PushAsync<TViewModel>()
        {
            return PushAsync<TViewModel>(null, true);
        }

        public Task PushAsync<TViewModel>(bool animated)
        {
            return PushAsync<TViewModel>(null, animated);
        }

        public Task PushAsync<TViewModel>(object initialisationParameter)
        {
            return PushAsync<TViewModel>(initialisationParameter, true);
        }

        public async Task PushAsync<TViewModel>(object initialisationParameter, bool animated)
        {
            var page = await GetPageForPush<TViewModel>(initialisationParameter).ConfigureAwait(false);
            await Navigation.PushAsync(page, animated).ConfigureAwait(false);
        }
        #endregion

        #region Modal Push
        public Task PushModalAsync<TViewModel>()
        {
            return PushModalAsync<TViewModel>(null, true);
        }

        public Task PushModalAsync<TViewModel>(bool animated)
        {
            return PushModalAsync<TViewModel>(null, animated);
        }

        public Task PushModalAsync<TViewModel>(object initialisationParameter)
        {
            return PushModalAsync<TViewModel>(initialisationParameter, true);
        }

        public async Task PushModalAsync<TViewModel>(object initialisationParameter, bool animated)
        {
            var page = await GetPageForPush<TViewModel>(initialisationParameter);
            await Navigation.PushModalAsync(page, animated).ConfigureAwait(false);
        }
        #endregion
        #endregion
    }
}
