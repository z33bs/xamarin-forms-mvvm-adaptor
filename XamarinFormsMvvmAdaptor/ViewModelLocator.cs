using System;
using System.Globalization;
using System.Reflection;
using Xamarin.Forms;
using XamarinFormsMvvmAdaptor.FluentApi;

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

            if (view is Page page)
            {
                if (viewModel is IAppearing appearingVm)
                    page.Appearing += new WeakEventHandler<EventArgs>(
                        appearingVm.OnViewAppearing).Handler;

                if (viewModel is IDisappearing disappearingVm)
                    page.Disappearing += new WeakEventHandler<EventArgs>(
                        disappearingVm.OnViewDisappearing).Handler;
            }
            
            view.BindingContext = viewModel;
        }

        private static Type GetViewModelTypeForPage(Type pageType)
        {
            var name = pageType.Name.ReplaceLastOccurrence(
                            Settings.ViewSuffix, Settings.ViewModelSuffix);

            var viewAssemblyName = string.Format(CultureInfo.InvariantCulture
                , "{0}.{1}, {2}"
                , Settings.ViewModelNamespace ??
                    pageType.Namespace
                    .Replace(Settings.ViewSubNamespace, Settings.ViewModelSubNamespace)
                , name
                , Settings.ViewModelAssemblyName ?? pageType.GetTypeInfo().Assembly.FullName);

            return Type.GetType(viewAssemblyName);
        }
    }
}
