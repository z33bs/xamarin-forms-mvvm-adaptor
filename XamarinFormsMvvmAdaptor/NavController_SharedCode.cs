using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

using Page = Xamarin.Forms.Page;
using INavigation = Xamarin.Forms.INavigation;

namespace XamarinFormsMvvmAdaptor
{
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

        public INavigation Navigation => RootPage.Navigation;

        Page rootPage;
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

        public IMvvmAdaptorViewModel RootViewModel => RootPage.BindingContext as IMvvmAdaptorViewModel;

        public IMvvmAdaptorViewModel PreviousPageViewModel
        {
            get
            {
                if (RootPage.Navigation.NavigationStack.Count < 2)
                    return null;

                return RootPage.Navigation.NavigationStack
                    [RootPage.Navigation.NavigationStack.Count - 2].BindingContext
                    as IMvvmAdaptorViewModel;
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

        private static void BindPageToViewModel(Page page, IMvvmAdaptorViewModel viewModel)
        {
            page.GetType().GetProperty("BindingContext").SetValue(page, viewModel);
        }

#region Stack Manipulation Helpers
        public Task RemoveLastFromBackStackAsync()
        {
            if (RootPage != null)
            {
                RootPage.Navigation.RemovePage(
                    RootPage.Navigation.NavigationStack[RootPage.Navigation.NavigationStack.Count - 2]);
            }

            return Task.FromResult(true);
        }

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
        public static Page CreatePageForAsync(IMvvmAdaptorViewModel viewModel)
        {
            var page = InstantiatePage(viewModel.GetType());
            BindPageToViewModel(page, viewModel);
            return page;
        }

        public static async Task InitializeVmForPage(Page page, object initialisationParameter, bool continueOnCapturedContext = false)
        {
            if (page.BindingContext != null
                && page.BindingContext is IMvvmAdaptorViewModel)
                await (page.BindingContext as IMvvmAdaptorViewModel).InitializeAsync(initialisationParameter).ConfigureAwait(continueOnCapturedContext);
            //todo throw two different errors
            //throw new InvalidCastException()
            //throw new NullReferenceException()
        }
#endregion

#region Forms.INavigation Adaptation
        public void RemovePageFor<TViewModel>() where TViewModel : IMvvmAdaptorViewModel
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