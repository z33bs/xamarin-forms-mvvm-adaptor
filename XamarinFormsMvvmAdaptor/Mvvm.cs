using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public class Mvvm : MvvmBase, IMvvm
    {
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

            return NavigationRoot;
        }
    }
}
