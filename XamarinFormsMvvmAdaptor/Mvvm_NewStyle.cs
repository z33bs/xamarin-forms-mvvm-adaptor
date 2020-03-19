using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    ///<inheritdoc/>
    public partial class Mvvm : IMvvm
    {
        public Page Initialize<TViewModel>() where TViewModel : IAdaptorViewModel
        {
            var viewModel = ResolveOrCreateViewModel<TViewModel>();
            var page = CreatePageFor<TViewModel>();

            BindViewModelToPage(page, viewModel);
            WirePageEventsToViewModel(viewModel, page);

            NavigationRoot = new NavigationPage(page);

            IsInitialized = true;

            return NavigationRoot;
        }

        private IAdaptorViewModel ResolveOrCreateViewModel<TViewModel>() where TViewModel : IAdaptorViewModel
        {
            if (Ioc.IsRegistered<TViewModel>())
                return Ioc.Resolve<TViewModel>();

            if (IocLocal.IsRegistered<TViewModel>())
                return IocLocal.Resolve<TViewModel>();

            if (HasParamaterlessConstructor<TViewModel>())
                return Activator.CreateInstance<TViewModel>();

            throw new InvalidOperationException(
                $"Could not Resolve or Create {typeof(TViewModel).Name}" +
                $". It is not registered in {nameof(Ioc)} or" +
                $" in {nameof(IocLocal)}. Furthermore, {typeof(TViewModel).Name}" +
                $" does not have a paramaterless constructor. Either" +
                $" register the class, or give it a paramaterless" +
                $" constructor.");
        }

        private bool HasParamaterlessConstructor<T>()
            => typeof(T).GetConstructor(Type.EmptyTypes) != null;

        ///<inheritdoc/>
        public Task NewPushAsync<TViewModel>(
            object navigationData = null, bool animated = true)
            where TViewModel : IAdaptorViewModel
        {
            return InternalPushAsync<TViewModel>(navigationData, animated);
        }

        ///<inheritdoc/>
        public Task NewPushModalAsync<TViewModel>(
            object navigationData = null, bool animated = true)
            where TViewModel : IAdaptorViewModel
        {
            return InternalPushAsync<TViewModel>(navigationData, animated, isModal: true);
        }

        async Task InternalPushAsync<TViewModel>(
            object navigationData = null, bool animated = true, bool isModal = false)
            where TViewModel : IAdaptorViewModel
        {
            //todo clean up unused InstantiatePage and Other such things once finished with NewStyle stuff
            var viewModel = ResolveOrCreateViewModel<TViewModel>();
            var page = CreatePageFor<TViewModel>();
            BindViewModelToPage(page, viewModel);
            WirePageEventsToViewModel(viewModel, page);

            var isPushedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (isModal)
                        await NavigationRoot.Navigation.PushModalAsync(
                            ModalStack.Any()
                            ? page
                            : new NavigationPage(page)
                            , animated);
                    else
                        await NavigationRoot.Navigation.PushAsync(page, animated);

                    isPushedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPushedTcs.SetException(ex);
                }
            });

            if (await isPushedTcs.Task)
                await viewModel.OnViewPushedAsync(navigationData).ConfigureAwait(false);
        }

        private static Page CreatePageFor<TViewModel>() where TViewModel : IAdaptorViewModel
        {
            return Activator.CreateInstance(
                    GetPageTypeForViewModel<TViewModel>()) as Page;
        }

        private static void WirePageEventsToViewModel(IAdaptorViewModel viewModel, Page page)
        {
            page.Appearing += new WeakEventHandler<EventArgs>(
                viewModel.OnViewAppearing).Handler;
            page.Disappearing += new WeakEventHandler<EventArgs>(
                viewModel.OnViewDisappearing).Handler;
        }
    }
}
