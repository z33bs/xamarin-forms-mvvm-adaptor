using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    ///<inheritdoc/>
    public partial class Mvvm : IMvvm
    {
        //public IIocContainer LocalContainer { get; set; } = new IocContainer();

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
        public async Task DiPushAsync<TViewModel>(object navigationData = null, bool animated = true) where TViewModel : IAdaptorViewModel
        {
            var viewModel = ResolveOrCreateViewModel<TViewModel>();
            var page = GetPageForPush(viewModel);

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
                await InitializeVmForPageAsync(page, navigationData).ConfigureAwait(false);
                await TopViewModel.OnAppearingAsync().ConfigureAwait(false);
            }
        }


        ///<inheritdoc/>
        public async Task DiPushModalAsync<TViewModel>(object navigationData = null, bool animated = true) where TViewModel : IAdaptorViewModel
        {
            var viewModel = ResolveOrCreateViewModel<TViewModel>();
            var page = GetPageForPush(viewModel);

            var isPushedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await NavigationRoot.Navigation
                    .PushModalAsync(
                        this.ModalStack.Any()
                            ? page
                            : new NavigationPage(page)
                        , animated)
                    .ConfigureAwait(false);

                    isPushedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPushedTcs.SetException(ex);
                }
            });

            if (await isPushedTcs.Task)
            {
                //await InitializeVmForPageAsync(page, navigationData).ConfigureAwait(false);
                await TopViewModel.InitializeAsync(navigationData).ConfigureAwait(false);
                await TopViewModel.OnAppearingAsync().ConfigureAwait(false);
            }
        }
    }
}
