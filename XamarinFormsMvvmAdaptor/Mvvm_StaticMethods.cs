using System;
using System.Globalization;
using System.Reflection;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public partial class Mvvm
    {
        public static IIoc Ioc { get; private set; } = new Ioc();

        public static void SetMainPage()
        {

            if (Ioc.IsRegistered<IMvvm>())
                Application.Current.MainPage = Ioc.Resolve<IMvvm>().NavigationRoot;

            else
                throw new Exception("Could not resolve a NavController!");
        }

        public static void SetMainPage(string key)
        {
            if (Ioc.IsRegistered<IMultiNavigation>())
            {
                var multi = Ioc.Resolve<IMultiNavigation>();
                if (multi.NavigationControllers.Count > 0)
                    Application.Current.MainPage = multi.NavigationControllers[key].NavigationRoot;
            }
            else
                throw new Exception("Could not resolve a NavController!");
        }

        //public static void Use3rdPartyIoc(IIocContainer iocContainer)
        //{
        //    if (iocContainer is IIocContainer)
        //        GlobalContainer = iocContainer;
        //    else
        //        throw new Exception($"Container provided does not implement {nameof(IIocContainer)}. Use the Adaptor pattern if needed");
        //}

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

        private static Type GetPageTypeForViewModel<TViewModel>()
        {
            return GetPageTypeForViewModel(typeof(TViewModel));
        }

        private static Type GetPageTypeForViewModel(Type viewModelType)
        {
            var nameSpace = viewModelType.Namespace
                            .Replace(_viewModelSubNamespace, _viewSubNamespace);
            var name =
                viewModelType.IsInterface && viewModelType.Name.StartsWith("I")
                ? ReplaceLastOccurrence(
                            viewModelType.Name.Substring(1), _viewModelSuffix, _viewSuffix)
                : ReplaceLastOccurrence(
                            viewModelType.Name, _viewModelSuffix, _viewSuffix);

            var viewAssemblyName = string.Format(CultureInfo.InvariantCulture
                , "{0}.{1}, {2}"
                , nameSpace
                , name
                , viewModelType.GetTypeInfo().Assembly.FullName);

            return Type.GetType(viewAssemblyName);
        }

        private static Type GetViewModelTypeForPage(Type pageType)
        {
            var nameSpace = pageType.Namespace
                            .Replace(_viewSubNamespace, _viewModelSubNamespace);
            var name = ReplaceLastOccurrence(
                            pageType.Name, _viewSuffix, _viewModelSuffix);

            var viewAssemblyName = string.Format(CultureInfo.InvariantCulture
                , "{0}.{1}, {2}"
                , nameSpace
                , name
                , pageType.GetTypeInfo().Assembly.FullName);

            return Type.GetType(viewAssemblyName);
        }

        private static string ReplaceLastOccurrence(string source, string find, string replace)
        {
            int place = source.LastIndexOf(find);

            if (place == -1)
                return source;

            return source.Remove(place, find.Length).Insert(place, replace);
        }

        private static void BindViewModelToPage(Page page, IAdaptorViewModel viewModel)
        {
            page.GetType().GetProperty("BindingContext").SetValue(page, viewModel);
        }

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
    }
}
