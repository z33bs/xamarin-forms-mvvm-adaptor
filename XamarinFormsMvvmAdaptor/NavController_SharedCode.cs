using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

using Page = Xamarin.Forms.Page;
using INavigation = Xamarin.Forms.INavigation;
using Xamarin.Forms;
using System.Collections.Generic;

namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// MvvM Navigation Controller. Each instance contains its own Navigation stack.
    /// Navigation engine depends on the user sticking to a naming convention:
    /// Views are expected to be in the <see cref="DEFAULT_V_NAMESPACE"/> subnamespace
    /// and end with suffix <see cref="DEFAULT_V_SUFFIX"/>.
    /// ViewModels are expected to be in the <see cref="DEFAULT_VM_NAMESPACE"/> subnamespace
    /// and end with suffix <see cref="DEFAULT_VM_SUFFIX"/>.
    /// This behaviour can be customised with <see cref="SetNamingConventions(string, string, string, string)"/>
    /// </summary>
    public partial class NavController
    {
        #region Settings
        const string DEFAULT_VM_NAMESPACE = "ViewModels";
        const string DEFAULT_V_NAMESPACE = "Views";
        const string DEFAULT_VM_SUFFIX = "ViewModel";
        const string DEFAULT_V_SUFFIX = "Page";

        static string _viewModelSubNamespace = DEFAULT_VM_NAMESPACE;
        static string _viewSubNamespace = DEFAULT_V_NAMESPACE;
        static string _viewModelSuffix = DEFAULT_VM_SUFFIX;
        static string _viewSuffix = DEFAULT_V_SUFFIX;

        /// <summary>
        /// Customise the controller's expected naming convention
        /// </summary>
        /// <param name="viewModelSubNamespace"></param>
        /// <param name="viewSubNamespace"></param>
        /// <param name="viewModelSuffix"></param>
        /// <param name="viewSuffix"></param>
        public static void SetNamingConventions(
            string viewModelSubNamespace = DEFAULT_VM_NAMESPACE
            , string viewSubNamespace = DEFAULT_V_NAMESPACE
            , string viewModelSuffix = DEFAULT_VM_SUFFIX
            , string viewSuffix = DEFAULT_V_SUFFIX)
        {
            _viewModelSubNamespace = viewModelSubNamespace;
            _viewSubNamespace = viewSubNamespace;
            _viewModelSuffix = viewModelSuffix;
            _viewSuffix = viewSuffix;
        }
        #endregion

        /// <summary>
        /// Constructs the <see cref="NavController"/>
        /// Remember to initialize before use
        /// </summary>
        public NavController()
        {
        }

        /// <summary>
        /// Returns true if InitAsync has run successfully
        /// </summary>
        public bool IsInitialized { get; private set; }
        void ThrowIfNotInitialized()
        {
            if (!IsInitialized)
                throw new NotInitializedException(
                    $"{nameof(NavController)} is not initialized. Please run {nameof(InitAsync)} first.");
        }

        /// <summary>
        /// Gets the stack of pages in the navigation
        /// </summary>
        public IReadOnlyList<Page> MainStack => Root.Navigation.NavigationStack;

        /// <summary>
        /// Gets the modal navigation stack. The <see cref="ModalStack"/> always hides the <see cref="MainStack"/>
        /// </summary>
        public IReadOnlyList<Page> ModalStack => Root.Navigation.ModalStack;

        /// <summary>
        /// Page at the root/bottom of the <see cref="MainStack"/>
        /// </summary>
        public Page RootPage => MainStack[0];

        private NavigationPage root;
        private NavigationPage Root
        {
            get
            {
                ThrowIfNotInitialized();
                return root;
            }
            set => root = value;
        }

        /// <summary>
        /// Currently visible <see cref="Page"/> at the top of the <see cref="MainStack"/>
        /// or <see cref="ModalStack"/> if it has any <see cref="Page"/>s in it
        /// </summary>
        public Page TopPage
        {
            get
            {
                ThrowIfNotInitialized();
                if (ModalStack.Count > 0)
                    return ModalStack[ModalStack.Count - 1];

                return MainStack[MainStack.Count - 1];
            }
        }

        /// <summary>
        /// <see cref="Page"/> beneath the <see cref="TopPage"/>
        /// It will be the page that appears when the currently visible
        /// <see cref="Page"/> is Popped.
        /// Returns null if <see cref="TopPage"/> is the <see cref="Root"/>
        /// </summary>
        public Page HiddenPage
        {
            get
            {
                ThrowIfNotInitialized();
                if (ModalStack.Count > 0)
                {
                    if (ModalStack.Count > 1)
                        return ModalStack[ModalStack.Count - 2];

                    return MainStack[MainStack.Count - 1];
                }

                if (MainStack.Count > 1)
                    return MainStack[MainStack.Count - 2];

                return null;
            }
        }

        /// <summary>
        /// Viewmodel corresponding to the <see cref="Root"/>
        /// <exception><see cref="NullReferenceException"/> is thrown when the <see cref="Root"/>s
        /// BindingContext has not been set.</exception>
        /// </summary>
        public IAdaptorViewModel RootViewModel
        {
            get
            {
                try
                {
                    if (Root is NavigationPage)
                        return ((NavigationPage)Root).RootPage.BindingContext as IAdaptorViewModel;

                    return Root.BindingContext as IAdaptorViewModel;
                }
                catch (NullReferenceException ex)
                {
                    throw new NullReferenceException($"{nameof(Root)}'s BindingContext has not been set", ex);
                }
            }
        }

        /// <summary>
        /// Viewmodel corresponding to the <see cref="TopPage"/>
        /// <exception><see cref="NullReferenceException"/> is thrown when the <see cref="TopPage"/>s
        /// BindingContext has not been set.</exception>
        /// </summary>
        public IAdaptorViewModel TopViewModel
        {
            get
            {
                try { return TopPage.BindingContext as IAdaptorViewModel; }
                catch (NullReferenceException ex)
                {
                    throw new NullReferenceException($"{nameof(TopPage)}'s BindingContext has not been set", ex);
                }
            }
        }

        /// <summary>
        /// Viewmodel corresponding to the <see cref="HiddenPage"/>
        /// Returns null if <see cref="TopPage"/> is the <see cref="Root"/>
        /// <exception><see cref="NullReferenceException"/> is thrown when the <see cref="HiddenPage"/>s
        /// BindingContext has not been set.</exception>
        /// </summary>
        public IAdaptorViewModel HiddenViewModel
        {
            get
            {
                if (HiddenPage is null)
                    return null;

                try { return HiddenPage.BindingContext as IAdaptorViewModel; }
                catch (NullReferenceException ex)
                {
                    throw new NullReferenceException($"{nameof(HiddenPage)}'s BindingContext has not been set", ex);
                }
            }
        }

        private static Page InstantiatePage(Type viewModelType)
        {
            try
            {
                return Activator.CreateInstance(
                    GetPageTypeForViewModel(viewModelType))
                    as Page;
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Could not find the Page associated with the given ViewModel." +
                    "Check that your Namespace and File Names follow the required MvvmAdaptor conventions.", ex);
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidCastException("The View associated with your ViewModel doesn't appear to be of type" +
                    " Xamarin.Forms.Page", ex);
            }
        }

        private static Type GetPageTypeForViewModel(Type viewModelType)
        {
            var viewName = viewModelType.FullName
                                        .Replace(_viewModelSubNamespace, _viewSubNamespace)
                                        .Replace(_viewModelSuffix, _viewSuffix);
            var viewModelAssemblyName = viewModelType.GetTypeInfo().Assembly.FullName;
            var viewAssemblyName = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", viewName, viewModelAssemblyName);
            var viewType = Type.GetType(viewAssemblyName);
            return viewType;
        }

        private static void BindViewModelToPage(Page page, IAdaptorViewModel viewModel)
        {
            page.GetType().GetProperty("BindingContext").SetValue(page, viewModel);
        }

        #region Stack Manipulation Helpers
        /// <summary>
        /// Removes the <see cref="HiddenPage"/> from the <see cref="MainStack"/> but not the <see cref="ModalStack"/>
        /// </summary>
        /// <returns></returns>
        public void RemovePreviousPageFromMainStack()
        {
                if (MainStack.Count > 1)
                    Root.Navigation.RemovePage(
                        MainStack[MainStack.Count-2]);            
        }

        /// <summary>
        /// Removes all pages in the <see cref="MainStack"/> except for
        /// the <see cref="TopPage"/>, which becomes the <see cref="Root"/> of the stack.
        /// </summary>
        public void CollapseMainStack()
        {
            if (MainStack.Count > 1)
            {
                foreach (var page in MainStack)
                {
                    if (page != TopPage)
                        Root.Navigation.RemovePage(page);

                }
                //for (int i = 0; i < RootPage.Navigation.NavigationStack.Count - 1; i++)
                //{
                //    var page = RootPage.Navigation.NavigationStack[i];
                //    RootPage.Navigation.RemovePage(page);
                //}
            }
        }
        #endregion

        #region Static Helpers
        /// <summary>
        /// Instantiates the <see cref="Page"/> associated with a given <see cref="IAdaptorViewModel"/>
        /// and sets the ViewModel as its BindingContext
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public static Page DiCreatePageForAsync(IAdaptorViewModel viewModel)
        {
            var page = InstantiatePage(viewModel.GetType());
            BindViewModelToPage(page, viewModel);
            return page;
        }

        /// <summary>
        /// Trigers the <see cref="IAdaptorViewModel.InitializeAsync(object)"/> method
        /// in the <see cref="IAdaptorViewModel"/> associated with a given page
        /// </summary>
        /// <param name="page"></param>
        /// <param name="initialisationParameter"></param>
        /// <param name="continueOnCapturedContext"></param>
        /// <returns></returns>
        private async Task InitializeVmForPageAsync(Page page, object initialisationParameter, bool continueOnCapturedContext = false)
        {
            try
            {
                await (page.BindingContext as IAdaptorViewModel).InitializeAsync(initialisationParameter).ConfigureAwait(continueOnCapturedContext);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Check if your ViewModel is attached to the Page", ex);
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidCastException($"Check if your ViewModel implements {nameof(IAdaptorViewModel)}", ex);
            }
        }
        #endregion

        #region Forms.INavigation Adaptation
        /// <summary>
        /// Removes a <see cref="Page"/> from the <see cref="MainStack"/>
        /// that corresponds to a given ViewModel
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        public void RemovePageFor<TViewModel>() where TViewModel : IAdaptorViewModel
        {
            var pageType = GetPageTypeForViewModel(typeof(TViewModel));

            foreach (var item in MainStack)
            {
                if (item.GetType() == pageType)
                {
                    Root.Navigation.RemovePage(item);
                    return;
                }
            }
        }

        /// <summary>
        /// Asynchronously removes the <see cref="TopPage" /> from the navigation stack, with optional animation.
        /// </summary>
        /// <param name="animated">Whether to animate the pop.</param>
        public async Task PopAsync(bool animated = true)
        {
            var isPoppedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Root.Navigation.PopAsync(animated);
                    isPoppedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPoppedTcs.SetException(ex);
                }
            });

            if (await isPoppedTcs.Task)
                await TopViewModel.OnAppearingAsync().ConfigureAwait(false);

        }

        /// <summary>
        /// Pops the entire <see cref="MainStack"/>, leaving only the <see cref="Root"/>, with optional animation.
        /// </summary>
        /// <param name="animated">Whether to animate the pop.</param>
        /// <returns></returns>
        public async Task PopToRootAsync(bool animated = true)
        {
            var isPoppedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Root.Navigation.PopToRootAsync(animated);
                    isPoppedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPoppedTcs.SetException(ex);
                }
            });

            if (await isPoppedTcs.Task)
                await RootViewModel.OnAppearingAsync().ConfigureAwait(false);

        }

        /// <summary>
        /// Asynchronously dismisses the most recent modally presented <see cref="Page" />, with optional animation.
        /// </summary>
        /// <param name="animated">Whether to animate the pop.</param>
        public async Task PopModalAsync(bool animated = true)
        {
            var isPoppedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Root.Navigation.PopModalAsync(animated);
                    isPoppedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPoppedTcs.SetException(ex);
                }
            });

            if (await isPoppedTcs.Task)
                await TopViewModel.OnAppearingAsync().ConfigureAwait(false);
        }
        #endregion
    }

    /// <summary>The <see cref="NavController"/> is not initialized</summary>
    public class NotInitializedException : Exception
    {
        /// <summary>The <see cref="NavController"/> is not initialized</summary>
        public NotInitializedException()
        { }
        /// <summary>The <see cref="NavController"/> is not initialized</summary>
        public NotInitializedException(string message) : base(message)
        { }
        /// <summary>The <see cref="NavController"/> is not initialized</summary>
        public NotInitializedException(string message, Exception innerException) : base(message,innerException)
        { }
    }
}