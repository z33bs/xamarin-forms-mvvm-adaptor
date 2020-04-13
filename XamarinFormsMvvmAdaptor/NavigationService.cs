using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;

//todo
//Make OnRemoved optional
//Finalise BaseViewModel interfaces and version
//functionality
//documentation
//tests
//Autofac adaptor
namespace XamarinFormsMvvmAdaptor
{
    public class NavigationService : INavigationService
    {
        ///<inheritdoc/>
        public IReadOnlyList<Page> NavigationStack => Shell.Current.Navigation.NavigationStack;
        ///<inheritdoc/>
        public IReadOnlyList<Page> ModalStack => Shell.Current.Navigation.ModalStack;


        #region CONSTRUCTIVE
        ///<inheritdoc/>
        public Task GoToAsync(ShellNavigationState state, bool animate)
        {
            return Shell.Current.GoToAsync(state, animate);
        }

        ///<inheritdoc/>
        public Task GoToAsync(ShellNavigationState state)
        {
            return Shell.Current.GoToAsync(state);
        }

        ///<inheritdoc/>
        public async Task<TViewModel> PushAsync<TViewModel>(
            object navigationData, bool animated = true)
            where TViewModel : class, IPushed
        {
            var page = await InternalPushAsync<TViewModel>(navigationData, animated);
            return page.BindingContext as TViewModel; //can be null if no viewModel resolved
        }

        ///<inheritdoc/>
        public async Task<TViewModel> PushAsync<TViewModel>(
            bool animated = true)
            where TViewModel : class
        {
            var page = await InternalPushAsync<TViewModel>(animated);
            return page.BindingContext as TViewModel; //can be null if no viewModel resolved
        }


        ///<inheritdoc/>
        public async Task<TViewModel> PushModalAsync<TViewModel>(
            object navigationData, bool animated = true)
            where TViewModel : class, IPushed
        {
            var page = await InternalPushAsync<TViewModel>(navigationData, animated, isModal: true);
            return page.BindingContext as TViewModel;
        }

        ///<inheritdoc/>
        public async Task<TViewModel> PushModalAsync<TViewModel>(
            bool animated = true)
            where TViewModel : class
        {
            var page = await InternalPushAsync<TViewModel>(animated, isModal: true);
            return page.BindingContext as TViewModel;
        }

        private async Task<Page> InternalPushAsync<TViewModel>(
            bool animated = true, bool isModal = false)
            where TViewModel : class
        {
            var page = Activator.CreateInstance(
                    GetPageTypeForViewModel<TViewModel>()) as Page;

            if (isModal)
                await Shell.Current.Navigation.PushModalAsync(
                    page, animated);
            else
                await Shell.Current.Navigation.PushAsync(page, animated);

            return page;
        }

        async Task<Page> InternalPushAsync<TViewModel>(
            object navigationData = null, bool animated = true, bool isModal = false)
            where TViewModel : class, IPushed
        {
            var page = await InternalPushAsync<TViewModel>(animated, isModal);

            if (page.BindingContext is IPushed viewModel)
                await viewModel.OnViewPushedAsync(navigationData).ConfigureAwait(false);

            return page;
        }

        private Type GetPageTypeForViewModel<TViewModel>()
        {
            return GetPageTypeForViewModel(typeof(TViewModel));
        }

        private Type GetPageTypeForViewModel(Type viewModelType)
        {
            var name =
                viewModelType.IsInterface && viewModelType.Name.StartsWith("I")
                ? Helpers.ReplaceLastOccurrence(
                            viewModelType.Name.Substring(1), Settings.ViewModelSuffix, Settings.ViewSuffix)
                : Helpers.ReplaceLastOccurrence(
                            viewModelType.Name, Settings.ViewModelSuffix, Settings.ViewSuffix);

            var viewAssemblyName = string.Format(CultureInfo.InvariantCulture
                , "{0}.{1}, {2}"
                , Settings.ViewNamespace ??
                    viewModelType.Namespace
                    .Replace(Settings.ViewModelSubNamespace, Settings.ViewSubNamespace)
                , name
                , Settings.ViewAssemblyName ?? viewModelType.GetTypeInfo().Assembly.FullName);

            return Type.GetType(viewAssemblyName);
        }


        #endregion

        #region DESTRUCTIVE
        ///<inheritdoc/>
        public async Task RemovePreviousPageFromMainStack()
        {
            var removedViewModel = NavigationStack.GetPreviousViewModel();

            if (NavigationStack.Count > 1)
                Shell.Current.Navigation.RemovePage(
                    NavigationStack.GetPreviousPage());

            await removedViewModel.OnViewRemovedAsync();
        }

        ///<inheritdoc/>
        public async Task RemovePageFor<TViewModel>() where TViewModel : IBaseViewModel
        {
            var pageType = GetPageTypeForViewModel(typeof(TViewModel));

            foreach (var item in NavigationStack)
            {
                if (item.GetType() == pageType)
                {
                    Shell.Current.Navigation.RemovePage(item);
                    await (item.BindingContext as IBaseViewModel).OnViewRemovedAsync();
                    break;
                }
            }
        }

        ///<inheritdoc/>
        public async Task PopAsync(bool animated = true)
        {
            var poppedViewModel = NavigationStack.GetCurrentViewModel();

            var isPoppedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Shell.Current.Navigation.PopAsync(animated);
                    isPoppedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPoppedTcs.SetException(ex);
                }
            });

            if (await isPoppedTcs.Task)
                await poppedViewModel.OnViewRemovedAsync();
        }

        ///<inheritdoc/>
        public async Task PopToRootAsync(bool animated = true)
        {
            var poppedViewModel = NavigationStack.GetCurrentViewModel();

            var isPoppedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Shell.Current.Navigation.PopToRootAsync(animated);
                    isPoppedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPoppedTcs.SetException(ex);
                }
            });

            if (await isPoppedTcs.Task)
                await poppedViewModel.OnViewRemovedAsync();
        }

        ///<inheritdoc/>
        public async Task PopModalAsync(bool animated = true)
        {
            if (!ModalStack.Any())
                throw new InvalidOperationException("Modal Stack is Empty");

            var poppedViewModel = ModalStack.GetCurrentViewModel();

            var isPoppedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Shell.Current.Navigation.PopModalAsync(animated);
                    isPoppedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPoppedTcs.SetException(ex);
                }
            });

            if (await isPoppedTcs.Task)
                await poppedViewModel.OnViewRemovedAsync();

        }
        #endregion

    }
}
