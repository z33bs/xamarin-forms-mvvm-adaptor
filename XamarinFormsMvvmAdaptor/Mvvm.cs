using System;
using System.Threading.Tasks;

using Page = Xamarin.Forms.Page;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Reflection;
//todo
//Add config for mustTryInterfaceVariation
namespace XamarinFormsMvvmAdaptor
{
    ///<inheritdoc/>
    public partial class Mvvm : IMvvm
    {
        #region SETTINGS
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
        #region PROPERTIES
        public IIoc Ioc { get; } = new Ioc();

        Page root;
        public Page NavigationRoot
        {
            get
            {
                ThrowIfNotInitialized();
                return root;
            }
            private set => root = value;
        }

        ///<inheritdoc/>
        public bool IsInitialized { get; private set; }

        ///<inheritdoc/>
        public IReadOnlyList<Page> MainStack => NavigationRoot.Navigation.NavigationStack;
        ///<inheritdoc/>
        public IReadOnlyList<Page> ModalStack => NavigationRoot.Navigation.ModalStack;

        ///<inheritdoc/>
        public Page RootPage => MainStack[0];
        ///<inheritdoc/>
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
        ///<inheritdoc/>
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

        ///<inheritdoc/>
        public IMvvmViewModelBase RootViewModel
        {
            get
            {
                try
                {
                    if (NavigationRoot is NavigationPage)
                        return ((NavigationPage)NavigationRoot).RootPage.BindingContext as IMvvmViewModelBase;

                    return NavigationRoot.BindingContext as IMvvmViewModelBase;
                }
                catch (NullReferenceException ex)
                {
                    throw new NullReferenceException($"{nameof(NavigationRoot)}'s BindingContext has not been set", ex);
                }
            }
        }
        ///<inheritdoc/>
        public IMvvmViewModelBase TopViewModel
        {
            get
            {
                try
                {
                    return
                        (TopPage is NavigationPage
                        ? (TopPage as NavigationPage).CurrentPage.BindingContext
                        : TopPage.BindingContext
                        ) as IMvvmViewModelBase;
                    //return TopPage.BindingContext as IAdaptorViewModel;
                }
                catch (NullReferenceException ex)
                {
                    throw new NullReferenceException($"{nameof(TopPage)}'s BindingContext has not been set", ex);
                }
            }
        }
        ///<inheritdoc/>
        public IMvvmViewModelBase HiddenViewModel
        {
            get
            {
                if (HiddenPage is null)
                    return null;

                try { return HiddenPage.BindingContext as IMvvmViewModelBase; }
                catch (NullReferenceException ex)
                {
                    throw new NullReferenceException($"{nameof(HiddenPage)}'s BindingContext has not been set", ex);
                }
            }
        }
        #endregion
        #region INITIALISATION


        public Page Initialize<TViewModel>(bool mustWrapInNavigationPage = true) where TViewModel : class, IMvvmViewModelBase
        {
            var viewModel = ResolveViewModel<TViewModel>();
            var page = CreatePageFor<TViewModel>();

            BindViewModelToPage(page, viewModel);
            WirePageEventsToViewModel(viewModel, page);

            NavigationRoot = mustWrapInNavigationPage
                ? new NavigationPage(page)
                : page;

            IsInitialized = true;


            if (page is TabbedPage tabbedPage)
            {
                if (viewModel is IMvvmTabbedViewModelBase)
                    WireTabbedPageEventsToViewModel(viewModel as IMvvmTabbedViewModelBase, page as TabbedPage);
                //Look for multi attribute
                var constructorsDecorated = viewModel.GetType()
                    .GetConstructors()
                    .Where(c => c.GetCustomAttribute<MultipleNavigationAttribute>() != null);

                if (constructorsDecorated.Any())
                {
                    var multiNavigationAttribute = constructorsDecorated
                        .First()
                        .GetCustomAttribute<MultipleNavigationAttribute>();

                    var multiNav = Ioc.Resolve(typeof(IMultiNavigation)) as IMultiNavigation;

                    int i = 0;
                    foreach (var item in multiNavigationAttribute.MvvmControllerKeys)
                    {
                        (multiNav.NavigationControllers[item] as Mvvm).InitializeTabMvvmController(tabbedPage.Children[i++]);
                    }
                    //foreach (var item in multi)
                    //{
                    //    if (item.Value.RootPage.GetType() == childPage.GetType())
                    //        Console.WriteLine(childPage.GetType().Name);
                    //}

                }
                else
                {
                    foreach (var childPage in tabbedPage.Children)
                    {
                        var childViewModel =
                            childPage is NavigationPage
                            ? CreateViewModelFor((childPage as NavigationPage).RootPage)
                            : CreateViewModelFor(childPage);
                        childPage.BindingContext = childViewModel;
                        WirePageEventsToViewModel(childViewModel, childPage);
                    }
                }
            }


            return NavigationRoot;
        }
        private void InitializeTabMvvmController(Page tabChild)
        {
            var isNavigationPage = tabChild is NavigationPage;
            Page page = isNavigationPage
                ? (tabChild as NavigationPage).RootPage
                : tabChild;

            var viewModel = CreateViewModelFor(page);
            BindViewModelToPage(page, viewModel);
            WirePageEventsToViewModel(viewModel, page);

            NavigationRoot = isNavigationPage
                ? tabChild as NavigationPage
                : new NavigationPage(page);

            IsInitialized = true;
        }

        private void ThrowIfNotInitialized()
        {
            if (!IsInitialized)
                throw new NotInitializedException(
                    $"{nameof(Mvvm)} is not initialized. Please run {nameof(Initialize)} first.");
        }
        #endregion
        #region CONSTRUCTIVE
        public Task<TViewModel> PushAsync<TViewModel>(TViewModel viewModel, object navigationData = null, bool animated = true)
            where TViewModel : class, IMvvmViewModelBase
        {
            return InternalPushAsync(viewModel, navigationData, animated);
        }

        public Task<TViewModel> PushModalAsync<TViewModel>(TViewModel viewModel,
            object navigationData = null, bool animated = true)
            where TViewModel : class, IMvvmViewModelBase
        {
            return InternalPushAsync(viewModel, navigationData, animated, isModal: true);
        }

        ///<inheritdoc/>
        public Task<TViewModel> PushAsync<TViewModel>(
            object navigationData = null, bool animated = true)
            where TViewModel : class, IMvvmViewModelBase
        {
            var viewModel = ResolveViewModel<TViewModel>();

            return InternalPushAsync(viewModel, navigationData, animated);
        }

        ///<inheritdoc/>
        public Task<TViewModel> PushModalAsync<TViewModel>(
            object navigationData = null, bool animated = true)
            where TViewModel : class, IMvvmViewModelBase
        {
            var viewModel = ResolveViewModel<TViewModel>();

            return InternalPushAsync(viewModel, navigationData, animated, isModal: true);
        }

        async Task<TViewModel> InternalPushAsync<TViewModel>(TViewModel viewModel,
            object navigationData = null, bool animated = true, bool isModal = false)
            where TViewModel : class, IMvvmViewModelBase
        {
            //todo clean up unused InstantiatePage and Other such things once finished with NewStyle stuff
            var page = CreatePageFor<TViewModel>();
            BindViewModelToPage(page, viewModel);
            WirePageEventsToViewModel(viewModel, page);

            var isPushedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (isModal)
                        await NavigationRoot.Navigation.PushModalAsync(
                            ModalStack.Any()
                            ? page
                            : new NavigationPage(page)
                            , animated);
                    else
                        await NavigationRoot.Navigation.PushAsync(page, animated);

                    isPushedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPushedTcs.SetException(ex);
                }
            });

            if (await isPushedTcs.Task)
                await viewModel.OnViewPushedAsync(navigationData).ConfigureAwait(false);

            return viewModel as TViewModel;
        }

        private IMvvmViewModelBase CreateViewModelFor(Page child)
        {
            var viewModelType = GetViewModelTypeForPage(child.GetType());
            return ResolveViewModel(viewModelType, mustTryInterfaceVariation: true);
        }

        private Page CreatePageFor<TViewModel>() where TViewModel : IMvvmViewModelBase
        {
            return Activator.CreateInstance(
                    GetPageTypeForViewModel<TViewModel>()) as Page;
        }

        private void WirePageEventsToViewModel(IMvvmViewModelBase viewModel, Page page)
        {
            //todo consider just taking a page and assuming its wired-up (note assumption required)
            page.Appearing += new WeakEventHandler<EventArgs>(
                viewModel.OnViewAppearing).Handler;
            page.Disappearing += new WeakEventHandler<EventArgs>(
                viewModel.OnViewDisappearing).Handler;
        }

        private void WireTabbedPageEventsToViewModel(IMvvmTabbedViewModelBase viewModel, TabbedPage page)
        {

            page.CurrentPageChanged += new WeakEventHandler<EventArgs>(
                viewModel.OnTabbedViewCurrentPageChanged).Handler;
        }

        private TViewModel ResolveViewModel<TViewModel>() where TViewModel : class, IMvvmViewModelBase
        {
            return ResolveViewModel(typeof(TViewModel)) as TViewModel;
        }

        private IMvvmViewModelBase ResolveViewModel(Type viewModelType, bool mustTryInterfaceVariation = false)
        {
            if (typeof(IMvvmViewModelBase).IsAssignableFrom(viewModelType.GetType()))
                throw new InvalidOperationException("viewModelType is expected to implement IAdaptorViewModel");


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
                $"in {nameof(Mvvm)}.{nameof(Ioc)}.");
        }

        private Type GetPageTypeForViewModel<TViewModel>()
        {
            return GetPageTypeForViewModel(typeof(TViewModel));
        }

        private Type GetPageTypeForViewModel(Type viewModelType)
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

        private Type GetViewModelTypeForPage(Type pageType)
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

        private void BindViewModelToPage(Page page, IMvvmViewModelBase viewModel)
        {
            page.GetType().GetProperty("BindingContext").SetValue(page, viewModel);
        }

        #endregion

        #region HELPERS

        private string ReplaceLastOccurrence(string source, string find, string replace)
        {
            int place = source.LastIndexOf(find);

            if (place == -1)
                return source;

            return source.Remove(place, find.Length).Insert(place, replace);
        }

        #endregion


        //private async Task InitializeVmAsync(Page page, object initialisationParameter, bool continueOnCapturedContext = false)
        //{
        //    try
        //    {
        //        await (page.BindingContext as IMvvmViewModelBase).OnViewPushedAsync(initialisationParameter).ConfigureAwait(continueOnCapturedContext);
        //    }
        //    catch (NullReferenceException ex)
        //    {
        //        throw new NullReferenceException("Check if your ViewModel is attached to the Page", ex);
        //    }
        //    catch (InvalidCastException ex)
        //    {
        //        throw new InvalidCastException($"Check if your ViewModel implements {nameof(IMvvmViewModelBase)}", ex);
        //    }
        //}
        #region DESTRUCTIVE
        ///<inheritdoc/>
        public async Task RemovePreviousPageFromMainStack()
        {
            var removedViewModel = MainStack.GetPreviousViewModel();

            if (MainStack.Count > 1)
                NavigationRoot.Navigation.RemovePage(
                    MainStack.GetPreviousPage());

            await removedViewModel.OnViewRemovedAsync();
        }

        ///<inheritdoc/>
        public async Task<IMvvmViewModelBase> CollapseMainStack()
        {
            if (MainStack.Count > 1)
            {
                while (MainStack.Count > 1)
                {
                    var removedViewModel = MainStack.GetPreviousViewModel();
                    NavigationRoot.Navigation.RemovePage(MainStack.GetPreviousPage());
                    await removedViewModel.OnViewRemovedAsync();
                }
            }
            return MainStack.GetCurrentViewModel();
        }

        ///<inheritdoc/>
        public async Task RemovePageFor<TViewModel>() where TViewModel : IMvvmViewModelBase
        {
            var pageType = GetPageTypeForViewModel(typeof(TViewModel));

            foreach (var item in MainStack)
            {
                if (item.GetType() == pageType)
                {
                    NavigationRoot.Navigation.RemovePage(item);
                    await (item.BindingContext as IMvvmViewModelBase).OnViewRemovedAsync();
                    break;
                }
            }
        }

        ///<inheritdoc/>
        public async Task<IMvvmViewModelBase> PopAsync(bool animated = true)
        {
            var poppedViewModel = MainStack.GetCurrentViewModel();

            var isPoppedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await NavigationRoot.Navigation.PopAsync(animated);
                    isPoppedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPoppedTcs.SetException(ex);
                }
            });

            if (await isPoppedTcs.Task)
                await poppedViewModel.OnViewRemovedAsync();

            return MainStack.GetCurrentViewModel();
        }

        ///<inheritdoc/>
        public async Task<IMvvmViewModelBase> PopMainStackToRootAsync(bool animated = true)
        {
            var poppedViewModel = MainStack.GetCurrentViewModel();

            var isPoppedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await NavigationRoot.Navigation.PopToRootAsync(animated);
                    isPoppedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPoppedTcs.SetException(ex);
                }
            });

            if (await isPoppedTcs.Task)
                await poppedViewModel.OnViewRemovedAsync();

            return RootViewModel;
        }

        ///<inheritdoc/>
        public async Task<IMvvmViewModelBase> PopModalAsync(bool animated = true)
        {
            if (!ModalStack.Any())
                throw new InvalidOperationException("Modal Stack is Empty");

            var poppedViewModel = ModalStack.GetCurrentViewModel();

            var isPoppedTcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await NavigationRoot.Navigation.PopModalAsync(animated);
                    isPoppedTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPoppedTcs.SetException(ex);
                }
            });

            if (await isPoppedTcs.Task)
                await poppedViewModel.OnViewRemovedAsync();

            return TopViewModel;
        }
        #endregion
    }
}