using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using XamarinFormsMvvmAdaptor.FluentApi;

//todo
//No need to search for interface version of ViewModel?
//Perhaps check if vm : BaseVm before wiring events - more flexible (vm as INavigationViewModel)?.OnAppearing();
namespace XamarinFormsMvvmAdaptor
{
    public static class ViewModelLocator
    {
        public static IIoc Ioc { get; } = new Ioc();

        public static ConfigOptions Configure()
        {
            return new ConfigOptions();
        }

        public static readonly BindableProperty AutoWireViewModelProperty =
            BindableProperty.CreateAttached("AutoWireViewModel", typeof(bool), typeof(ViewModelLocator), default(bool)
                , propertyChanged: OnAutoWireViewModelChanged);

        public static bool GetAutoWireViewModel(BindableObject bindable)
        {
            return (bool)bindable.GetValue(AutoWireViewModelProperty);
        }

        public static void SetAutoWireViewModel(BindableObject bindable, bool value)
        {
            bindable.SetValue(AutoWireViewModelProperty, value);
        }

        public static void AutoWireViewModel(Page view)
        {
            if (view != null)
                WireViewModel(view);
        }

        private static void OnAutoWireViewModelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is Element view && (bool)newValue == true))
                return;
            
            WireViewModel(view);
        }

        private static void WireViewModel(Element view)
        {
            var viewType = view.GetType();

            var viewModelType = GetViewModelTypeForPage(viewType);
            if (viewModelType is null)
                throw new ViewModelBindingException(viewType);

            var viewModel = Ioc.Resolve(viewModelType);

            if (viewModel is null) //unlikely to be true
                throw new ViewModelBindingException(viewType);

            if (view is Page
                && viewModel is IBaseViewModel baseViewModel)
                    WirePageEventsToViewModel(baseViewModel, view as Page);

            view.BindingContext = viewModel;
        }

        private static Type GetViewModelTypeForPage(Type pageType)
        {
            var name = Helpers.ReplaceLastOccurrence(
                            pageType.Name, Settings.ViewSuffix, Settings.ViewModelSuffix);

            var viewAssemblyName = string.Format(CultureInfo.InvariantCulture
                , "{0}.{1}, {2}"
                , Settings.ViewModelNamespace ??
                    pageType.Namespace
                    .Replace(Settings.ViewSubNamespace, Settings.ViewModelSubNamespace)
                , name
                , Settings.ViewModelAssemblyName ?? pageType.GetTypeInfo().Assembly.FullName);

            return Type.GetType(viewAssemblyName);
        }


        private static void WirePageEventsToViewModel(IBaseViewModel viewModel, Page page)
        {
            if (page == null
                || viewModel == null)
                return;

            page.Appearing += new WeakEventHandler<EventArgs>(
                viewModel.OnViewAppearing).Handler;
            page.Disappearing += new WeakEventHandler<EventArgs>(
                viewModel.OnViewDisappearing).Handler;
        }

    }
}
