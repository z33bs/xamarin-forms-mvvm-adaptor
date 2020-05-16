using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinFormsMvvmAdaptor.Helpers;

//todo
//Ensure BIOMT first checks if on Main
//SafeBeginInvokeOnMainThread
//Make ExtensionClasses mockable
//Helpers in Separate Namespace
//documentation for IoC
//acknowledgements in Licence
//clean old unused code
//Autofac adaptor
namespace XamarinFormsMvvmAdaptor
{
    ///<inheritdoc/>
    public class NavigationService : INavigationService
    {
        ///<inheritdoc/>
        public Shell CurrentShell => Shell.Current;

        readonly INavigation navigation;

        /// <summary>
        /// Default Constructor
        /// </summary>
        [ResolveUsing]
        public NavigationService()
        {
            navigation = Shell.Current.Navigation;
        }

        /// <summary>
        /// Constructor for unit testing
        /// </summary>
        /// <param name="navigation"></param>
        public NavigationService(INavigation navigation)
        {
            this.navigation = navigation;
        }

        ///<inheritdoc/>
        public IReadOnlyList<Page> NavigationStack => navigation.NavigationStack;
        ///<inheritdoc/>
        public IReadOnlyList<Page> ModalStack => navigation.ModalStack;

        #region Avoid dependancy on Xamarin.Forms
        ///<inheritdoc/>
        public ISafeMessagingCenter SafeMessagingCenter => Helpers.SafeMessagingCenter.Instance;

        ///<inheritdoc/>
        public void BeginInvokeOnMainThread(Action action)
            => Device.BeginInvokeOnMainThread(action);

        #endregion
        #region CONSTRUCTIVE
        ///<inheritdoc/>
        public Task GoToAsync(ShellNavigationState state, bool animate = true)
        {
            return GoToAsync(state, null, animate);
        }

        ///<inheritdoc/>
        public async Task GoToAsync(ShellNavigationState state, object navigationData, bool animate = true)
        {
            var isPushed = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Shell.Current.GoToAsync(state, animate).ConfigureAwait(false);
                    isPushed.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPushed.SetException(ex);
                }
            });

            if (await isPushed.Task.ConfigureAwait(false))
            {
                var presentedViewModel = (Shell.Current?.CurrentItem?.CurrentItem as IShellSectionController)?
                .PresentedPage.BindingContext;
                if (presentedViewModel is IOnViewNavigated viewModel)
                    await viewModel.OnViewNavigatedAsync(navigationData).ConfigureAwait(false);
                else
                    throw new ArgumentException($"You are trying to pass {nameof(navigationData)}" +
                        $" to a ViewModel that doesn't implement {nameof(IOnViewNavigated)}");
            }
        }

        ///<inheritdoc/>
        public Task GoToAsync(ShellNavigationState state)
        {
            return GoToAsync(state, true);
        }

        ///<inheritdoc/>
        public async Task<TViewModel> PushAsync<TViewModel>(
            object navigationData, bool animated = true)
            where TViewModel : class, IOnViewNavigated
        {
            var page = await InternalPushAsync<TViewModel>(navigationData, animated);
            return page.BindingContext as TViewModel; //can be null if no viewModel resolved
        }

        ///<inheritdoc/>
        public async Task<TViewModel> PushAsync<TViewModel>(
            bool animated = true)
            where TViewModel : class
        {
            var page = await InternalPushAsync<TViewModel>(null, animated);
            return page.BindingContext as TViewModel; //can be null if no viewModel resolved
        }


        ///<inheritdoc/>
        public async Task<TViewModel> PushModalAsync<TViewModel>(
            object navigationData, bool animated = true)
            where TViewModel : class, IOnViewNavigated
        {
            var page = await InternalPushAsync<TViewModel>(navigationData, animated, isModal: true);
            return page.BindingContext as TViewModel;
        }

        ///<inheritdoc/>
        public async Task<TViewModel> PushModalAsync<TViewModel>(
            bool animated = true)
            where TViewModel : class
        {
            var page = await InternalPushAsync<TViewModel>(null, animated, isModal: true);
            return page.BindingContext as TViewModel;
        }

        private async Task<Page> InternalPushAsync<TViewModel>(
            object navigationData,
            bool animated = true,
            bool isModal = false)
            where TViewModel : class
        {
            var page = Activator.CreateInstance(
                    GetPageTypeForViewModel<TViewModel>()) as Page;

            var isPushed = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (isModal)
                        await navigation.PushModalAsync(
                            page, animated).ConfigureAwait(false);
                    else
                        await navigation.PushAsync(page, animated).ConfigureAwait(false);
                    isPushed.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPushed.SetException(ex);
                }
            });


            if (await isPushed.Task.ConfigureAwait(false) && page.BindingContext is IOnViewNavigated viewModel)
                await viewModel.OnViewNavigatedAsync(navigationData).ConfigureAwait(false);

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
                ? viewModelType.Name.Substring(1).ReplaceLastOccurrence(
                            Settings.ViewModelSuffix, Settings.ViewSuffix)
                : viewModelType.Name.ReplaceLastOccurrence(
                            Settings.ViewModelSuffix, Settings.ViewSuffix);

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
            var viewModel = NavigationStack.GetPreviousViewModel();

            if (NavigationStack.Count > 1)
            {
                var isRemoved = new TaskCompletionSource<bool>();
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        navigation.RemovePage(
                                NavigationStack.GetPreviousPage());
                        isRemoved.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        isRemoved.SetException(ex);
                    }
                });

                if (await isRemoved.Task.ConfigureAwait(false) && viewModel is IOnViewRemoved removedViewModel)
                    await removedViewModel.OnViewRemovedAsync().ConfigureAwait(false);
            }
        }

        ///<inheritdoc/>
        public async Task RemovePageFor<TViewModel>()
        {
            var pageType = GetPageTypeForViewModel(typeof(TViewModel));

            foreach (var item in NavigationStack)
            {
                if (item?.GetType() == pageType)
                {
                    var isRemoved = new TaskCompletionSource<bool>();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            navigation.RemovePage(item);
                            isRemoved.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            isRemoved.SetException(ex);
                        }
                    });

                    if (await isRemoved.Task.ConfigureAwait(false)
                        && item.BindingContext is IOnViewRemoved viewModel)
                        await viewModel.OnViewRemovedAsync().ConfigureAwait(false);

                    break;
                }
            }
        }

        ///<inheritdoc/>
        public async Task PopAsync(bool animated = true)
        {
            var viewModel = NavigationStack.GetCurrentViewModel();

            var isPopped = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await navigation.PopAsync(animated).ConfigureAwait(false);
                    isPopped.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPopped.SetException(ex);
                }
            });

            if (await isPopped.Task.ConfigureAwait(false)
                && viewModel is IOnViewRemoved removedViewModel)
                await removedViewModel.OnViewRemovedAsync().ConfigureAwait(false);
        }

        ///<inheritdoc/>
        public async Task PopToRootAsync(bool animated = true)
        {
            for (int i = navigation.NavigationStack.Count - 1; i > 0; i--)
            {
                await PopAsync(animated).ConfigureAwait(false);
            }
        }

        ///<inheritdoc/>
        public async Task PopModalAsync(bool animated = true)
        {
            if (!ModalStack.Any())
                throw new InvalidOperationException("Modal Stack is Empty");

            var viewModel = ModalStack.GetCurrentViewModel();

            var isPopped = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await navigation.PopModalAsync(animated);
                    isPopped.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPopped.SetException(ex);
                }
            });

            if (await isPopped.Task.ConfigureAwait(false)
                && viewModel is IOnViewRemoved removedViewModel)
                await removedViewModel.OnViewRemovedAsync().ConfigureAwait(false);
        }
        #endregion

        #region Dialogues/Popups
        ///<inheritdoc/>
        public Task DisplayAlert(string title, string message, string cancel)
        {
            return DisplayAlert(title, message, null, cancel);
        }

        ///<inheritdoc/>
        public Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
        {
            var hasDisplayed = new TaskCompletionSource<Task<bool>>();
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    hasDisplayed.SetResult(CurrentShell.DisplayAlert(title, message, accept, cancel));
                }
                catch (Exception ex)
                {
                    hasDisplayed.SetException(ex);
                }
            });

            return hasDisplayed.Task.Result;
        }

        ///<inheritdoc/>
        public Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
        {
            var hasDisplayed = new TaskCompletionSource<Task<string>>();
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    hasDisplayed.SetResult(CurrentShell.DisplayActionSheet(title, cancel, destruction, buttons));
                }
                catch (Exception ex)
                {
                    hasDisplayed.SetException(ex);
                }
            });

            return hasDisplayed.Task.Result;
        }

        ///<inheritdoc/>
        public Task<string> DisplayPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel", string placeholder = null, int maxLength = -1, Keyboard keyboard = default(Keyboard), string initialValue = "")
        {
            var hasDisplayed = new TaskCompletionSource<Task<string>>();
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    hasDisplayed.SetResult(CurrentShell.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength, keyboard, initialValue));
                }
                catch (Exception ex)
                {
                    hasDisplayed.SetException(ex);
                }
            });

            return hasDisplayed.Task.Result;
        }
        #endregion

    }
}
