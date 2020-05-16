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
    /// <summary>
    /// Controlls Page Navigation from the ViewModel
    /// </summary>
    public class NavigationService : INavigationService
    {
        /// <summary>
        /// Returns Xamarin.Forms.Shell.Current
        /// </summary>
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

        /// <summary>
        /// Gets the Current Shell's NavigationStack
        /// </summary>
        public IReadOnlyList<Page> NavigationStack => navigation.NavigationStack;
        /// <summary>
        /// Gets the Current Shell's ModalStack
        /// </summary>
        public IReadOnlyList<Page> ModalStack => navigation.ModalStack;

        #region Avoid dependancy on Xamarin.Forms
        /// <summary>
        /// Returns a singleton instance of the MessagingCenter
        /// </summary>
        public ISafeMessagingCenter SafeMessagingCenter => Helpers.SafeMessagingCenter.Instance;

        /// <summary>
        /// Invokes an Action on the device's main (UI) thread.
        /// Wrapper of Xamarin.Forms <see cref="Device.BeginInvokeOnMainThread(Action)"/>
        /// </summary>
        public void BeginInvokeOnMainThread(Action action)
            => Device.BeginInvokeOnMainThread(action);

        #endregion
        #region CONSTRUCTIVE
        /// <summary>
        /// Navigates to a <see cref="Page"/>
        /// </summary>
        /// <param name="state">A URI representing either the current page or a destination for navigation in a Shell application.</param>
        /// <param name="animate"></param>
        /// <returns></returns>
        public Task GoToAsync(ShellNavigationState state, bool animate = true)
        {
            return GoToAsync(state, null, animate);
        }

        /// <summary>
        /// Navigates to a <see cref="Page"/> passing data to the target ViewModel
        /// </summary>
        /// <param name="state"></param>
        /// <param name="navigationData"></param>
        /// <param name="animate"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Navigates to a <see cref="Page"/> passing data to the target ViewModel
        /// </summary>
        /// <param name="state"></param>
        public Task GoToAsync(ShellNavigationState state)
        {
            return GoToAsync(state, true);
        }

        /// <summary>
        /// Pushes a <see cref="Page"/> onto the <see cref="NavigationStack"/>
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel corresponding the the Page to be Pushed</typeparam>
        /// <param name="navigationData">Object that will be recieved by the <see cref="IOnViewNavigated.OnViewNavigatedAsync(object)"/> method</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        public async Task<TViewModel> PushAsync<TViewModel>(
            object navigationData, bool animated = true)
            where TViewModel : class, IOnViewNavigated
        {
            var page = await InternalPushAsync<TViewModel>(navigationData, animated);
            return page.BindingContext as TViewModel; //can be null if no viewModel resolved
        }

        /// <summary>
        /// Pushes a <see cref="Page"/> onto the <see cref="NavigationStack"/>
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel corresponding the the Page to be Pushed</typeparam>
        /// <param name="animated"></param>
        /// <returns></returns>
        public async Task<TViewModel> PushAsync<TViewModel>(
            bool animated = true)
            where TViewModel : class
        {
            var page = await InternalPushAsync<TViewModel>(null, animated);
            return page.BindingContext as TViewModel; //can be null if no viewModel resolved
        }


        /// <summary>
        /// Pushes a <see cref="Page"/> onto the <see cref="ModalStack"/>
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel corresponding the the Page to be Pushed</typeparam>
        /// <param name="navigationData">Object that will be recieved by the <see cref="IOnViewNavigated.OnViewNavigatedAsync(object)"/> method</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        public async Task<TViewModel> PushModalAsync<TViewModel>(
            object navigationData, bool animated = true)
            where TViewModel : class, IOnViewNavigated
        {
            var page = await InternalPushAsync<TViewModel>(navigationData, animated, isModal: true);
            return page.BindingContext as TViewModel;
        }

        /// <summary>
        /// Pushes a <see cref="Page"/> onto the <see cref="ModalStack"/>
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel corresponding the the Page to be Pushed</typeparam>
        /// <param name="animated"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Removes the <see cref="Page"/> underneath the Top Page in the <see cref="NavigationStack"/>
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Removes specific <see cref="Page"/> from the <see cref="NavigationStack"/>
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel corresponding the the Page to be Removed</typeparam>
        /// <returns></returns>
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

        /// <summary>
        /// Pops a <see cref="Page"/> off the <see cref="NavigationStack"/>
        /// </summary>
        /// <param name="animated"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Pops all pages <see cref="Page"/> off the <see cref="NavigationStack"/>, leaving only the Root Page
        /// </summary>
        /// <param name="animated"></param>
        /// <returns></returns>
        public async Task PopToRootAsync(bool animated = true)
        {
            for (int i = navigation.NavigationStack.Count - 1; i > 0; i--)
            {
                await PopAsync(animated).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Pops a <see cref="Page"/> off the <see cref="ModalStack"/>
        /// </summary>
        /// <param name="animated"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Display's an Alert
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        public Task DisplayAlert(string title, string message, string cancel)
        {
            return DisplayAlert(title, message, null, cancel);
        }

        /// <summary>
        /// Display's an Alert
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="accept"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Display's an Action Sheet
        /// </summary>
        /// <param name="title"></param>
        /// <param name="cancel"></param>
        /// <param name="destruction"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Display's a Prompt to the user
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="accept"></param>
        /// <param name="cancel"></param>
        /// <param name="placeholder"></param>
        /// <param name="maxLength"></param>
        /// <param name="keyboard"></param>
        /// <param name="initialValue"></param>
        /// <returns></returns>
        public Task<string> DisplayPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel", string placeholder = null, int maxLength = -1, Keyboard keyboard = default, string initialValue = "")
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
