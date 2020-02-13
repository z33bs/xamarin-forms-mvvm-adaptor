using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

using Page = Xamarin.Forms.Page;
using INavigation = Xamarin.Forms.INavigation;

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
        /// Exposes the root page's Xamarin.Forms' navigation engine which
        /// houses the NavigationStack, ModalStack, and Pop functionality
        /// </summary>
        public INavigation Navigation => RootPage.Navigation;

        Page rootPage;
        /// <summary>
        /// Page at the root of the navigation stack
        /// </summary>
        public Page RootPage
        {
            get
            {
                if (rootPage is null)
                    throw new RootPageNotSetException();
                return rootPage;
            }
            private set { rootPage = value; }
        }

        /// <summary>
        /// Viewmodel corresponding to the root page
        /// </summary>
        public IAdaptorViewModel RootViewModel => RootPage.BindingContext as IAdaptorViewModel;

        /// <summary>
        /// Viewmodel corresponding to the previous page in the
        /// navigation stack (n-1)
        /// </summary>
        public IAdaptorViewModel PreviousPageViewModel
        {
            get
            {
                if (RootPage.Navigation.NavigationStack.Count < 2)
                    return null;

                return RootPage.Navigation.NavigationStack
                    [RootPage.Navigation.NavigationStack.Count - 2].BindingContext
                    as IAdaptorViewModel;
            }
        }

        private static Page InstantiatePage(Type viewModelType)
        {
            var pageType = GetPageTypeForViewModel(viewModelType);
            if (pageType == null)
            {
                throw new Exception($"Cannot locate page type for {viewModelType.GetType()}");
            }
            //Create page
            return Activator.CreateInstance(pageType) as Page;
            //todo throw 2 errors null and type
        }

        private static Type GetPageTypeForViewModel(Type viewModelType)
        {
            var viewName = viewModelType.FullName
                                        .Replace(_viewModelSubNamespace, _viewSubNamespace)
                                        .Replace(_viewModelSuffix, _viewSuffix);
            var viewModelAssemblyName = viewModelType.GetTypeInfo().Assembly.FullName;
            var viewAssemblyName = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", viewName, viewModelAssemblyName);
            var viewType = Type.GetType(viewAssemblyName);
            //todo handle if cant find view
            return viewType;
        }

        private static void BindPageToViewModel(Page page, IAdaptorViewModel viewModel)
        {
            page.GetType().GetProperty("BindingContext").SetValue(page, viewModel);
        }

        #region Stack Manipulation Helpers
        /// <summary>
        /// Removes the previous page (n-1) of the navigation stack
        /// </summary>
        /// <returns></returns>
        public Task RemoveLastFromBackStackAsync()
        {
            if (RootPage != null)
            {
                RootPage.Navigation.RemovePage(
                    RootPage.Navigation.NavigationStack[RootPage.Navigation.NavigationStack.Count - 2]);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Removes all pages in the navigation stack except for
        /// the current page at the top of the stack
        /// </summary>
        /// <returns></returns>
        public Task RemoveBackStackAsync()
        {
            if (RootPage != null)
            {
                for (int i = 0; i < RootPage.Navigation.NavigationStack.Count - 1; i++)
                {
                    var page = RootPage.Navigation.NavigationStack[i];
                    RootPage.Navigation.RemovePage(page);
                }
            }

            return Task.FromResult(true);
        }
        #endregion

        #region Static Helpers
        /// <summary>
        /// Instantiates the page associated with a given ViewModel
        /// and sets the ViewModel as its binding context
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public static Page CreatePageForAsync(IAdaptorViewModel viewModel)
        {
            var page = InstantiatePage(viewModel.GetType());
            BindPageToViewModel(page, viewModel);
            return page;
        }

        /// <summary>
        /// Trigers the <see cref="AdaptorViewModel.InitializeAsync(object)"/> method
        /// in the <see cref="IAdaptorViewModel"/> associated with a given page
        /// </summary>
        /// <param name="page"></param>
        /// <param name="initialisationParameter"></param>
        /// <param name="continueOnCapturedContext"></param>
        /// <returns></returns>
        public static async Task InitializeVmForPage(Page page, object initialisationParameter, bool continueOnCapturedContext = false)
        {
            if (page.BindingContext != null
                && page.BindingContext is IAdaptorViewModel)
                await (page.BindingContext as IAdaptorViewModel).InitializeAsync(initialisationParameter).ConfigureAwait(continueOnCapturedContext);
            //todo throw two different errors
            //throw new InvalidCastException()
            //throw new NullReferenceException()
        }
        #endregion

        #region Forms.INavigation Adaptation
        /// <summary>
        /// Removes a page from the navigation stack
        /// that corresponds to a given ViewModel
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        public void RemovePageFor<TViewModel>() where TViewModel : IAdaptorViewModel
        {
            Type pageType = GetPageTypeForViewModel(typeof(TViewModel));

            foreach (var item in RootPage.Navigation.NavigationStack)
            {
                if (item.GetType() == pageType)
                {
                    RootPage.Navigation.RemovePage(item);
                    return;
                }
            }
        }
        #endregion
    }

    class RootPageNotSetException : Exception
    {
        public RootPageNotSetException() : base($"Root Page not Set.")
        {
        }
    }

    class RootViewModelNotSetException : Exception
    {
        public RootViewModelNotSetException() : base($"Root ViewModel not Set.")
        {
        }
    }


    public class MvvmNavigationException : Exception
    {
        public MvvmNavigationException(string message)
            : base(message)
        {
        }
    }
}