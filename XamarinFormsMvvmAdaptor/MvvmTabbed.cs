using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
//todo
// empty initialize
// NavigationRoot set to a new TabbedPage

namespace XamarinFormsMvvmAdaptor
{
    public class MvvmTabbed : Mvvm
    {
        private TabbedPage tabbedPage;
        new public TabbedPage RootPage => tabbedPage;
        //new public TabbedPage NavigationRoot
        //    => base.NavigationRoot as TabbedPage;

        //todo initialize with bunch of tabs
        public Page Initialize(bool mustWrapInNavigationPage = true)
        {
            tabbedPage = new TabbedPage();
            NavigationRoot = mustWrapInNavigationPage
                ? new NavigationPage(tabbedPage)
                : tabbedPage
                as Page;

            IsInitialized = true;

            return NavigationRoot;
        }

        private readonly IList<IMvvmViewModelBase> tabViewModels = new List<IMvvmViewModelBase>();
        public IReadOnlyList<IMvvmViewModelBase> TabViewModels
            => tabViewModels as IReadOnlyList<IMvvmViewModelBase>;

        public IMvvmViewModelBase CurrentTabViewModel
        {
            get
            {
                return (tabbedPage.CurrentPage is NavigationPage
                ? (tabbedPage.CurrentPage as NavigationPage).CurrentPage.BindingContext
                : tabbedPage.CurrentPage.BindingContext)
                as IMvvmViewModelBase;
            }
            set
            {
                for (int i = 0; i < TabViewModels.Count; i++)
                {
                    if(value.Equals(TabViewModels[i]))
                        tabbedPage.CurrentPage = tabbedPage.Children[i];
                }
            }
        }

        //todo add
        //public IList<Page> TabPages

        public void AddTab<TViewModel>(string title = null, FileImageSource icon = null, bool mustWrapInNavigationPage = false) where TViewModel : IMvvmViewModelBase
        {
            var viewModel = ResolveViewModel(typeof(TViewModel));
            var page = CreatePageFor<TViewModel>();
            BindViewModelToPage(page, viewModel);
            WirePageEventsToViewModel(viewModel, page);

            this.tabViewModels.Add(viewModel);

            InternalAddTabPageChild(title, icon, mustWrapInNavigationPage, page);
        }

        private void InternalAddTabPageChild(string title, FileImageSource icon, bool mustWrapInNavigationPage, Page page)
        {
            //if (!IsInitialized)
            //{
            //    NavigationRoot = new TabbedPage();
            //    IsInitialized = true;
            //}

            //var tabbedPage = (NavigationRoot is NavigationPage
            //    ? (NavigationRoot as NavigationPage).RootPage
            //    : NavigationRoot)
            //    as TabbedPage;

            tabbedPage.Children.Add(
                mustWrapInNavigationPage
                ? new NavigationPage(page)
                : page);
            tabbedPage.Children.Last().Title = title;
            tabbedPage.Children.Last().Icon = icon;
        }

        //
        public void AddTab(IMvvm mvvm, string title = null, FileImageSource icon = null)
        {
            if (!mvvm.IsInitialized)
                throw new NotInitializedException($"Initialise {nameof(mvvm)} before adding as a tab");

            tabViewModels.Add(mvvm.RootViewModel);
            InternalAddTabPageChild(title, icon, false, mvvm.NavigationRoot);
        }

        //todo Insert Remove Tabs (and rearrange viewModels the same)


    }


}
