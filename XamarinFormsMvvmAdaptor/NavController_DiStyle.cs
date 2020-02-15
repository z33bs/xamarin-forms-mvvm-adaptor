﻿using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public partial class NavController
    {
        /// <summary>
        /// Constructs the <see cref="NavController"/> with the given <see cref="RootViewModel"/>
        /// </summary>
        /// <param name="rootViewModel"></param>
        /// <param name="isWrappedInNavigationPage">If true then the
        /// <see cref="RootPage"/> will be wrapped in a <see cref="NavigationPage"/></param>
        public NavController(IAdaptorViewModel rootViewModel, bool isWrappedInNavigationPage = true)
        {
            var page = InstantiatePage(rootViewModel.GetType());
            BindViewModelToPage(page, rootViewModel);

            if (isWrappedInNavigationPage)
                RootPage = new NavigationPage(page);
            else
                RootPage = page;
        }

        private Page GetPageForPush(IAdaptorViewModel viewModel)
        {
            var page = InstantiatePage(viewModel.GetType());
            BindViewModelToPage(page, viewModel);
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
            var newPage = GetPageForPush(viewModel);
            BindViewModelToPage(newPage, viewModel);

            var anchorPage = GetPageTypeForViewModel(typeof(TViewModelExisting));

            foreach (var existingPage in RootPage.Navigation.NavigationStack)
            {
                if (existingPage.GetType() == anchorPage)
                {
                    RootPage.Navigation.InsertPageBefore(existingPage, newPage);
                    await InitializeVmForPage(newPage, navigationData).ConfigureAwait(false);
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
            var page = GetPageForPush(viewModel);

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
                await InitializeVmForPage(page, navigationData).ConfigureAwait(false);
                await TopViewModel.OnAppearing().ConfigureAwait(false);
            }
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
            var page = GetPageForPush(viewModel);

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
                await InitializeVmForPage(page, navigationData).ConfigureAwait(false);
                await TopViewModel.OnAppearing().ConfigureAwait(false);
            }
        }
        #endregion
        #endregion
    }
}
