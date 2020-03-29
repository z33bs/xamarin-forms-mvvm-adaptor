using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    ///<inheritdoc/>
    public partial class Mvvm : IMvvm
    {
        private Page CreatePageAndWireToVm(IMvvmViewModelBase viewModel)
        {
            var page = InstantiatePage(viewModel.GetType());
            BindViewModelToPage(page, viewModel);
            return page;
        }

        ///<inheritdoc/>
        public async Task DiInitAsync(IMvvmViewModelBase rootViewModel, object initialisationData = null)
        {
            var page = InstantiatePage(rootViewModel.GetType());
            BindViewModelToPage(page, rootViewModel);

            NavigationRoot = new NavigationPage(page);

            try
            {
                IsInitialized = true;
                await RootViewModel.OnViewPushedAsync(initialisationData).ConfigureAwait(false);
                await RootViewModel.OnAppearingAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                IsInitialized = false;
                throw new NotInitializedException("Initialization failed", ex);
            }
        }

        ///<inheritdoc/>
        public async Task DiInsertPageBefore<TViewModelExisting>(IMvvmViewModelBase viewModel, object navigationData = null)
        {
            var newPage = CreatePageAndWireToVm(viewModel);
            BindViewModelToPage(newPage, viewModel);

            var anchorPage = GetPageTypeForViewModel(typeof(TViewModelExisting));

            foreach (var existingPage in NavigationRoot.Navigation.NavigationStack)
            {
                if (existingPage.GetType() == anchorPage)
                {
                    NavigationRoot.Navigation.InsertPageBefore(newPage, existingPage);
                    await InitializeVmAsync(newPage, navigationData).ConfigureAwait(false);
                    break;
                }
            }
        }

        ///<inheritdoc/>
        public async Task DiPushAsync(IMvvmViewModelBase viewModel, object navigationData = null, bool animated = true)
        {
            var page = CreatePageAndWireToVm(viewModel);

            var isPushedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await NavigationRoot.Navigation.PushAsync(page, animated);
                    isPushedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPushedTcs.SetException(ex);
                }
            });

            if (await isPushedTcs.Task)
            {
                await InitializeVmAsync(page, navigationData).ConfigureAwait(false);
                await TopViewModel.OnAppearingAsync().ConfigureAwait(false);
            }
        }

        ///<inheritdoc/>
        public async Task DiPushModalAsync(IMvvmViewModelBase viewModel, object navigationData, bool animated)
        {
            var page = CreatePageAndWireToVm(viewModel);

            var isPushedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await NavigationRoot.Navigation.PushModalAsync(page, animated).ConfigureAwait(false);
                    isPushedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPushedTcs.SetException(ex);
                }
            });

            if (await isPushedTcs.Task)
            {
                await InitializeVmAsync(page, navigationData).ConfigureAwait(false);
                await TopViewModel.OnAppearingAsync().ConfigureAwait(false);
            }
        }
    }
}
