using System;
using System.Globalization;
using System.Reflection;
using Xamarin.Forms;
using XamarinFormsMvvmAdaptor.FluentApi;
using XamarinFormsMvvmAdaptor.Helpers;

namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Enables a View to find and attach its corresponding ViewModel
    /// </summary>
    public static class ViewModelLocator
    {
        static readonly IIoc defaultContainerImplementation = new Ioc();
        /// <summary>
        /// Overrides <see cref="Ioc"/> with the chosen <see cref="IIoc"/>
        /// </summary>
        public static IIoc ContainerImplementation { private get; set; } = defaultContainerImplementation;

        /// <summary>
        /// Dependency injection container
        /// </summary>
        public static IIoc Ioc => ContainerImplementation;

        /// <summary>
        /// Customises configuration
        /// </summary>
        /// <returns></returns>
        public static ConfigOptions Configure()
        {
            return new ConfigOptions();
        }

        /// <summary>
        /// Tells the <see cref="ViewModelLocator"/> to attach the corresponding ViewModel
        /// </summary>
        public static readonly BindableProperty AutoWireViewModelProperty =
            BindableProperty.CreateAttached("AutoWireViewModel", typeof(bool), typeof(ViewModelLocator), default(bool)
                , propertyChanged: OnAutoWireViewModelChanged);

        /// <summary>
        /// Gets the <see cref="AutoWireViewModelProperty"/>
        /// </summary>
        /// <param name="bindable"></param>
        /// <returns></returns>
        public static bool GetAutoWireViewModel(BindableObject bindable)
        {
            return (bool)bindable.GetValue(AutoWireViewModelProperty);
        }

        /// <summary>
        /// Sets the <see cref="AutoWireViewModelProperty"/>
        /// </summary>
        /// <param name="bindable"></param>
        /// <param name="value"></param>
        public static void SetAutoWireViewModel(BindableObject bindable, bool value)
        {
            bindable.SetValue(AutoWireViewModelProperty, value);
        }

        /// <summary>
        /// Sets the <see cref="AutoWireViewModelProperty"/> to true
        /// </summary>
        /// <param name="view">The view, commonly <see cref="this"/></param>
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
                if (viewModel is IOnViewAppearing appearingVm)
                    page.Appearing += new WeakEventHandler<EventArgs>(
                        appearingVm.OnViewAppearing).Handler;

                if (viewModel is IOnViewDisappearing disappearingVm)
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
