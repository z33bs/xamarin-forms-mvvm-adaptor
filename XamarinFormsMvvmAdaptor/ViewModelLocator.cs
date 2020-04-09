using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using XamarinFormsMvvmAdaptor.FluentApi;
//todo
//IIoc friendly for AutoFac
//Option to wire from CodeBehind - simpler way
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

            //todo Only try interface version if specifically requested
            var viewModel = ResolveViewModel(viewModelType, mustTryInterfaceVariation: true);

            if (viewModel is null)
                throw new ViewModelBindingException(viewType);

            //todo Check if this works, because page is not fully constructed yet
            if (view is Page)
                WirePageEventsToViewModel(viewModel, view as Page);

            view.BindingContext = viewModel;
        }

        private static IMvvmViewModelBase ResolveViewModel(Type viewModelType, bool mustTryInterfaceVariation = false)
        {
            if (!typeof(IMvvmViewModelBase).IsAssignableFrom(viewModelType))
                throw new NoBaseViewModelException(viewModelType);

            //Only if don't know how the ViewModel was registered
            if (mustTryInterfaceVariation)
            {
                var viewModelInterfaceTypeName = string.Format(CultureInfo.InvariantCulture
                    , "{0}.{1}, {2}"
                    , viewModelType.Namespace
                    , $"I{viewModelType.Name}"
                    , viewModelType.GetTypeInfo().Assembly.FullName);

                var iviewModelType = Type.GetType(viewModelInterfaceTypeName);

                if (iviewModelType != null
                    && Ioc.IsRegistered(iviewModelType))
                    return Ioc.Resolve(iviewModelType) as IMvvmViewModelBase;
            }

            //if ResolveMode not strict then Ioc will attempt Activator.Create
            return Ioc.Resolve(viewModelType) as IMvvmViewModelBase;

            throw new InvalidOperationException(
                $"Could not Resolve {viewModelType.Name}" +
                $". It is not registered " +
                $"in {nameof(MvvmBase)}.{nameof(Ioc)}.");
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


        private static void WirePageEventsToViewModel(IMvvmViewModelBase viewModel, Page page)
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
