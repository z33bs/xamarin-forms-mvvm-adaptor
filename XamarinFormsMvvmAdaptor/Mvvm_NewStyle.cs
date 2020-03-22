using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    ///<inheritdoc/>
    public partial class Mvvm : IMvvm
    {
        //todo Add option to not wrap in navigationPage?
        private void InitializeTabMvvmController(Page tabChild)
        {
            var isNavigationPage = tabChild is NavigationPage;
            Page page = isNavigationPage
                ? (tabChild as NavigationPage).RootPage
                : tabChild;

            var viewModel = ViewModelForPage(page);
            BindViewModelToPage(page, viewModel);
            WirePageEventsToViewModel(viewModel, page);

            NavigationRoot = isNavigationPage
                ? tabChild as NavigationPage
                : new NavigationPage(page);

            IsInitialized = true;
        }

        public Page Initialize<TViewModel>(bool mustWrapInNavigationPage = true) where TViewModel : class, IAdaptorViewModel
        {
            var viewModel = ResolveOrCreateViewModel<TViewModel>();
            var page = CreatePageFor<TViewModel>();

            BindViewModelToPage(page, viewModel);
            WirePageEventsToViewModel(viewModel, page);

            NavigationRoot = mustWrapInNavigationPage
                ? new NavigationPage(page)
                : page;

            IsInitialized = true;


            if (page is TabbedPage tabbedPage)
            {
                //Look for multi attribute
                var constructorInfos = viewModel.GetType()
                    .GetConstructors()
                    .Where(c => c.GetCustomAttribute<MultipleNavigationAttribute>() != null);

                if (constructorInfos.Any())
                {
                    var multiNavigationAttribute = constructorInfos
                        .First()
                        .GetCustomAttribute<MultipleNavigationAttribute>();

                    var multiNav = this.ResolveOrCreateObject(typeof(IMultiNavigation)) as IMultiNavigation;

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
                            ? ViewModelForPage((childPage as NavigationPage).RootPage)
                            : ViewModelForPage(childPage);
                        childPage.BindingContext = childViewModel;
                        WirePageEventsToViewModel(childViewModel, childPage);
                    }
                }
            }


            return NavigationRoot;
        }

        private TViewModel ResolveOrCreateViewModel<TViewModel>() where TViewModel : class, IAdaptorViewModel
        {
            return ResolveOrCreateViewModel(typeof(TViewModel)) as TViewModel;
            //if (Ioc.IsRegistered<TViewModel>())
            //    return Ioc.Resolve<TViewModel>();

            //if (IocLocal.IsRegistered<TViewModel>())
            //    return IocLocal.Resolve<TViewModel>();

            //if (HasParamaterlessConstructor<TViewModel>())
            //    return Activator.CreateInstance<TViewModel>();

            //throw new InvalidOperationException(
            //    $"Could not Resolve or Create {typeof(TViewModel).Name}" +
            //    $". It is not registered in {nameof(Ioc)} or" +
            //    $" in {nameof(IocLocal)}. Furthermore, {typeof(TViewModel).Name}" +
            //    $" does not have a paramaterless constructor. Either" +
            //    $" register the class, or give it a paramaterless" +
            //    $" constructor.");
        }

        private IAdaptorViewModel ResolveOrCreateViewModel(Type viewModelType, bool mustTryInterfaceVariation = false)
        {
            if (typeof(IAdaptorViewModel).IsAssignableFrom(viewModelType.GetType()))
                throw new InvalidOperationException("viewModelType is expected to implement IAdaptorViewModel");

            //if (Ioc.IsRegistered(viewModelType))
            //    return Ioc.Resolve(viewModelType) as IAdaptorViewModel;

            //if (IocLocal.IsRegistered(viewModelType))
            //    return IocLocal.Resolve(viewModelType) as IAdaptorViewModel;

            //if (HasParamaterlessConstructor(viewModelType))
            //    return Activator.CreateInstance(viewModelType) as IAdaptorViewModel;

            var viewModel = ResolveOrCreateObject(viewModelType);
            if (viewModel != null)
                return viewModel as IAdaptorViewModel;

            if (mustTryInterfaceVariation)
            {
                var viewModelInterfaceTypeName = string.Format(CultureInfo.InvariantCulture
                    , "{0}.{1}, {2}"
                    , viewModelType.Namespace
                    , $"I{viewModelType.Name}"
                    , viewModelType.GetTypeInfo().Assembly.FullName);

                viewModel = ResolveOrCreateObject(Type.GetType(viewModelInterfaceTypeName));
                if (viewModel != null)
                    return viewModel as IAdaptorViewModel;
            }

            throw new InvalidOperationException(
                $"Could not Resolve or Create {viewModelType.Name}" +
                $". It is not registered in {nameof(Ioc)} or" +
                $" in {nameof(IocLocal)}. Furthermore, {viewModelType.Name}" +
                $" does not have a paramaterless constructor. Either" +
                $" register the class, or give it a paramaterless" +
                $" constructor.");
        }

        private object ResolveOrCreateObject(Type type)
        {
            if (Ioc.IsRegistered(type))
                return Ioc.Resolve(type);

            if (IocLocal.IsRegistered(type))
                return IocLocal.Resolve(type);

            if (HasParamaterlessConstructor(type))
                return Activator.CreateInstance(type);

            return null;
        }

        private bool HasParamaterlessConstructor<T>()
            => typeof(T).GetConstructor(Type.EmptyTypes) != null;

        private bool HasParamaterlessConstructor(Type type)
            => type.GetConstructor(Type.EmptyTypes) != null;

        public Task<TViewModel> NewPushAsync<TViewModel>(TViewModel viewModel, object navigationData = null, bool animated = true)
            where TViewModel : class, IAdaptorViewModel
        {
            return InternalPushAsync(viewModel, navigationData, animated);
        }

        public Task<TViewModel> NewPushModalAsync<TViewModel>(TViewModel viewModel,
            object navigationData = null, bool animated = true)
            where TViewModel : class, IAdaptorViewModel
        {
            return InternalPushAsync(viewModel, navigationData, animated, isModal: true);
        }

        ///<inheritdoc/>
        public Task<TViewModel> NewPushAsync<TViewModel>(
            object navigationData = null, bool animated = true)
            where TViewModel : class, IAdaptorViewModel
        {
            var viewModel = ResolveOrCreateViewModel<TViewModel>();

            return InternalPushAsync(viewModel, navigationData, animated);
        }

        ///<inheritdoc/>
        public Task<TViewModel> NewPushModalAsync<TViewModel>(
            object navigationData = null, bool animated = true)
            where TViewModel : class, IAdaptorViewModel
        {
            var viewModel = ResolveOrCreateViewModel<TViewModel>();

            return InternalPushAsync(viewModel, navigationData, animated, isModal: true);
        }

        async Task<TViewModel> InternalPushAsync<TViewModel>(TViewModel viewModel,
            object navigationData = null, bool animated = true, bool isModal = false)
            where TViewModel : class, IAdaptorViewModel
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

        private IAdaptorViewModel ViewModelForPage(Page child)
        {
            var viewModelType = GetViewModelTypeForPage(child.GetType());
            return ResolveOrCreateViewModel(viewModelType, mustTryInterfaceVariation: true);
        }

        private static Page CreatePageFor<TViewModel>() where TViewModel : IAdaptorViewModel
        {
            return Activator.CreateInstance(
                    GetPageTypeForViewModel<TViewModel>()) as Page;
        }

        private static void WirePageEventsToViewModel(IAdaptorViewModel viewModel, Page page)
        {
            //todo consider just taking a page and assuming its wired-up (note assumption required)
            page.Appearing += new WeakEventHandler<EventArgs>(
                viewModel.OnViewAppearing).Handler;
            page.Disappearing += new WeakEventHandler<EventArgs>(
                viewModel.OnViewDisappearing).Handler;
        }
    }
}
