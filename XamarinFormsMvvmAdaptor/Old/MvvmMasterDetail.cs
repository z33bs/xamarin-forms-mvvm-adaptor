using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    //todo Make it a Helper. Force user to use Navcontrollers for detail pages.
    public class MvvmMasterDetail : MvvmBase
    {
        private MasterDetailPage masterDetailPage;
        new public MasterDetailPage RootPage => masterDetailPage;

        public Page Initialize<TMaster,TDetail>(MasterBehavior masterBehavior = MasterBehavior.Default,bool mustWrapInNavigationPage = false) where TMaster : IBaseViewModel where TDetail : IBaseViewModel
        {
            masterDetailPage = new MasterDetailPage { MasterBehavior = masterBehavior };
            SetMaster<TMaster>();
            SetDetail<TDetail>();

            NavigationRoot = mustWrapInNavigationPage
                ? new NavigationPage(masterDetailPage)
                : masterDetailPage
                as Page;

            IsInitialized = true;

            return NavigationRoot;
        }

        public IBaseViewModel MasterViewModel
            => masterDetailPage.Master.BindingContext as IBaseViewModel;
        public IBaseViewModel DetailViewModel
            => masterDetailPage.Detail.BindingContext as IBaseViewModel;
           
        public void SetMaster<TViewModel>(bool mustWrapInNavigationPage = false) where TViewModel : IBaseViewModel
        {
            var viewModel = ResolveViewModel(typeof(TViewModel));
            var page = CreatePageFor<TViewModel>();
            BindViewModelToPage(page, viewModel);
            WirePageEventsToViewModel(viewModel, page);

            masterDetailPage.Master =
                mustWrapInNavigationPage
                ? new NavigationPage(page)
                : page;
        }

        public void SetDetail<TViewModel>(bool mustWrapInNavigationPage = true) where TViewModel : IBaseViewModel
        {
            var viewModel = ResolveViewModel(typeof(TViewModel));
            var page = CreatePageFor<TViewModel>();
            BindViewModelToPage(page, viewModel);
            WirePageEventsToViewModel(viewModel, page);

            masterDetailPage.Detail =
                mustWrapInNavigationPage
                ? new NavigationPage(page)
                : page;
        }

    }
}
