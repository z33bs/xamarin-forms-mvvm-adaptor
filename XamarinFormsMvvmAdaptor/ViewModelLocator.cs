using System;
using System.Globalization;
using System.Reflection;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public static class ViewModelLocator
    {
		public static IIoc Ioc { get; } = new Ioc();

		public static readonly BindableProperty AutoWireViewModelProperty =
			BindableProperty.CreateAttached("AutoWireViewModel", typeof(bool), typeof(ViewModelLocator), default(bool)
                , propertyChanged: OnAutoWireViewModelChanged);

		public static readonly BindableProperty NavigationRootProperty =
			BindableProperty.CreateAttached("NavigationRoot", typeof(string), typeof(ViewModelLocator), default(string)
                ,propertyChanged:OnNavigationRootChanged);

		public static string GetNavigationRoot(BindableObject bindable)
		{
			return (string)bindable.GetValue(ViewModelLocator.NavigationRootProperty);
		}

		public static void SetNavigationRoot(BindableObject bindable, string value)
		{
			bindable.SetValue(ViewModelLocator.NavigationRootProperty, value);
		}


		private static void OnNavigationRootChanged(BindableObject bindable, object oldValue, object newValue)
        {
			var view = bindable as Element;
			if (view == null)
			{
				return;
			}
            
			//var viewType = view.GetType();

			var navigationService = new NavigationService(view as Page);
			Ioc.Register(navigationService);
		}

		public static bool GetAutoWireViewModel(BindableObject bindable)
		{
			return (bool)bindable.GetValue(ViewModelLocator.AutoWireViewModelProperty);
		}

		public static void SetAutoWireViewModel(BindableObject bindable, bool value)
		{
			bindable.SetValue(ViewModelLocator.AutoWireViewModelProperty, value);
		}

		private static void OnAutoWireViewModelChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var view = bindable as Element;
			if (view == null)
			{
				return;
			}

			var viewType = view.GetType();
			//var viewName = viewType.FullName.Replace(".Views.", ".ViewModels.");
			//var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
			//var viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}Model, {1}", viewName, viewAssemblyName);

			//var viewModelType = Type.GetType(viewModelName);
			var viewModelType = GetViewModelTypeForPage(viewType);
			if (viewModelType == null)
			{
				return;
			}
            //todo Only try interface version if specifically requested
			var viewModel = ResolveViewModel(viewModelType,mustTryInterfaceVariation:true);

            //todo Check if this works, because page is not fully constructed yet
            if(view is Page)
			    WirePageEventsToViewModel(viewModel, view as Page);

			view.BindingContext = viewModel;
		}

		private static IMvvmViewModelBase ResolveViewModel(Type viewModelType, bool mustTryInterfaceVariation = false)
		{
			if (typeof(IMvvmViewModelBase).IsAssignableFrom(viewModelType.GetType()))
				throw new InvalidOperationException("viewModelType is expected to implement IAdaptorViewModel");

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

			//if ResolveMode not strict then will attempt Activator.Create
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
			//todo consider just taking a page and assuming its wired-up (note assumption required)
			page.Appearing += new WeakEventHandler<EventArgs>(
				viewModel.OnViewAppearing).Handler;
			page.Disappearing += new WeakEventHandler<EventArgs>(
				viewModel.OnViewDisappearing).Handler;
		}

	}
}
