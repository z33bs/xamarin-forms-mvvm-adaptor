using System;
using System.Globalization;
using System.Reflection;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public partial class NavController
    {
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
