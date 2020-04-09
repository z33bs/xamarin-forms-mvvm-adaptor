using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public class NavigationService : INavigationService
    {

        #region CONSTRUCTIVE

        public Task GoToAsync(ShellNavigationState state, bool animate)
        {
            return Shell.Current.GoToAsync(state, animate);
        }

        public Task GoToAsync(ShellNavigationState state)
        {
            return Shell.Current.GoToAsync(state);
        }

        //public Task<TViewModel> PushAsync<TViewModel>(TViewModel viewModel, object navigationData = null, bool animated = true)
        //    where TViewModel : class, IMvvmViewModelBase
        //{
        //    return InternalPushAsync<TViewModel>(navigationData, animated);
        //}

        //public Task<TViewModel> PushModalAsync<TViewModel>(TViewModel viewModel,
        //    object navigationData = null, bool animated = true)
        //    where TViewModel : class, IMvvmViewModelBase
        //{
        //    return InternalPushAsync<TViewModel>(navigationData, animated, isModal: true);
        //}

        ///<inheritdoc/>
        public Task<TViewModel> PushAsync<TViewModel>(
            object navigationData = null, bool animated = true)
            where TViewModel : class, IMvvmViewModelBase
        {
            return InternalPushAsync<TViewModel>(navigationData, animated);
        }

        ///<inheritdoc/>
        public Task<TViewModel> PushModalAsync<TViewModel>(
            object navigationData = null, bool animated = true)
            where TViewModel : class, IMvvmViewModelBase
        {
            return InternalPushAsync<TViewModel>(navigationData, animated, isModal: true);
        }

        async Task<TViewModel> InternalPushAsync<TViewModel>(
            object navigationData = null, bool animated = true, bool isModal = false)
            where TViewModel : class, IMvvmViewModelBase
        {
            //todo clean up unused InstantiatePage and Other such things once finished with NewStyle stuff
            var page = CreatePageFor<TViewModel>();
            //BindViewModelToPage(page, viewModel);
            //WirePageEventsToViewModel(viewModel, page);

            var isPushedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (isModal)
                        await Shell.Current.Navigation.PushModalAsync(
                            page, animated);
                    else
                        await Shell.Current.Navigation.PushAsync(page, animated);

                    isPushedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPushedTcs.SetException(ex);
                }
            });


            if (page.BindingContext is IMvvmViewModelBase viewModel
                && await isPushedTcs.Task)
                await viewModel.OnViewPushedAsync(navigationData).ConfigureAwait(false);

            //can be null if no viewModel resolved
            return page.BindingContext as TViewModel;
        }

        protected Page CreatePageFor<TViewModel>() where TViewModel : IMvvmViewModelBase
        {
            return Activator.CreateInstance(
                    GetPageTypeForViewModel<TViewModel>()) as Page;
        }

        protected void WirePageEventsToViewModel(IMvvmViewModelBase viewModel, Page page)
        {
            //todo consider just taking a page and assuming its wired-up (note assumption required)
            page.Appearing += new WeakEventHandler<EventArgs>(
                viewModel.OnViewAppearing).Handler;
            page.Disappearing += new WeakEventHandler<EventArgs>(
                viewModel.OnViewDisappearing).Handler;
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
    }
}
