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
        // hide irrelevant initialisation
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

        public Page Initialize(IMvvm[] mvvms)
        {
            var page = Initialize();

            foreach (var mvvm in mvvms)
            {
                if (!mvvm.IsInitialized)
                    throw new NotInitializedException($"Initialise {nameof(mvvm)} before adding as a tab");

                tabViewModels.Add(mvvm.RootViewModel);
                InternalAddTabPageChild(mvvm.NavigationRoot, mustWrapInNavigationPage: false);
            }

            return page;
        }


        public Page Initialize<T1>(bool mustWrapInNavigationPage = true) where T1 : IMvvmViewModelBase
        {
            var page = Initialize(mustWrapInNavigationPage);
            AddTab<T1>();
            return page;
        }

        public Page Initialize<T1, T2>(bool mustWrapInNavigationPage = true)
            where T1 : IMvvmViewModelBase where T2 : IMvvmViewModelBase
        {
            var page = Initialize<T1>(mustWrapInNavigationPage);
            AddTab<T2>();
            return page;
        }

        public Page Initialize<T1, T2, T3>(bool mustWrapInNavigationPage = true)
            where T1 : IMvvmViewModelBase where T2 : IMvvmViewModelBase
            where T3 : IMvvmViewModelBase
        {
            var page = Initialize<T1, T2>(mustWrapInNavigationPage);
            AddTab<T3>();
            return page;
        }

        public Page Initialize<T1, T2, T3, T4>(bool mustWrapInNavigationPage = true)
            where T1 : IMvvmViewModelBase where T2 : IMvvmViewModelBase
            where T3 : IMvvmViewModelBase where T4 : IMvvmViewModelBase
        {
            var page = Initialize<T1, T2, T3>(mustWrapInNavigationPage);
            AddTab<T4>();
            return page;
        }

        public Page Initialize<T1, T2, T3, T4, T5>(bool mustWrapInNavigationPage = true)
            where T1 : IMvvmViewModelBase where T2 : IMvvmViewModelBase
            where T3 : IMvvmViewModelBase where T4 : IMvvmViewModelBase
            where T5 : IMvvmViewModelBase
        {
            var page = Initialize<T1, T2, T3, T4>(mustWrapInNavigationPage);
            AddTab<T5>();
            return page;
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
                    if (value.Equals(TabViewModels[i]))
                        tabbedPage.CurrentPage = tabbedPage.Children[i];
                }
            }
        }


        public void AddTab<TViewModel>(string title, FileImageSource icon, bool mustWrapInNavigationPage = false) where TViewModel : IMvvmViewModelBase
        {
            AddTab<TViewModel>(mustWrapInNavigationPage);

            tabbedPage.Children.Last().Title = title;
            tabbedPage.Children.Last().Icon = icon;
        }

        public void AddTab<TViewModel>(string title, bool mustWrapInNavigationPage = false) where TViewModel : IMvvmViewModelBase
        {
            AddTab<TViewModel>(mustWrapInNavigationPage);

            tabbedPage.Children.Last().Title = title;
        }

        public void AddTab<TViewModel>(FileImageSource icon, bool mustWrapInNavigationPage = false) where TViewModel : IMvvmViewModelBase
        {
            AddTab<TViewModel>(mustWrapInNavigationPage);

            tabbedPage.Children.Last().Icon = icon;
        }

        public void AddTab<TViewModel>(bool mustWrapInNavigationPage = false) where TViewModel : IMvvmViewModelBase
        {
            var viewModel = ResolveViewModel(typeof(TViewModel));
            var page = CreatePageFor<TViewModel>();
            BindViewModelToPage(page, viewModel);
            WirePageEventsToViewModel(viewModel, page);

            this.tabViewModels.Add(viewModel);

            InternalAddTabPageChild(page, mustWrapInNavigationPage);
        }

        public void InsertTab<TViewModel>(int index, string title, FileImageSource icon, bool mustWrapInNavigationPage = false) where TViewModel : IMvvmViewModelBase
        {
            var viewModel = ResolveViewModel(typeof(TViewModel));
            var page = CreatePageFor<TViewModel>();
            BindViewModelToPage(page, viewModel);
            WirePageEventsToViewModel(viewModel, page);

            this.tabViewModels.Insert(index, viewModel);

            //InternalInsertTabPageChild(index, title, icon, mustWrapInNavigationPage, page);
            InternalInsertTabPageChild(index, page, mustWrapInNavigationPage);
            tabbedPage.Children[index].Title = title;
            tabbedPage.Children[index].Icon = icon;

        }

        public void InsertTab<TViewModel>(int index, string title, bool mustWrapInNavigationPage = false) where TViewModel : IMvvmViewModelBase
        {
            var viewModel = ResolveViewModel(typeof(TViewModel));
            var page = CreatePageFor<TViewModel>();
            BindViewModelToPage(page, viewModel);
            WirePageEventsToViewModel(viewModel, page);

            this.tabViewModels.Insert(index, viewModel);

            //InternalInsertTabPageChild(index, title, icon, mustWrapInNavigationPage, page);
            InternalInsertTabPageChild(index, page, mustWrapInNavigationPage);
            tabbedPage.Children[index].Title = title;
        }

        public void InsertTab<TViewModel>(int index, FileImageSource icon, bool mustWrapInNavigationPage = false) where TViewModel : IMvvmViewModelBase
        {
            var viewModel = ResolveViewModel(typeof(TViewModel));
            var page = CreatePageFor<TViewModel>();
            BindViewModelToPage(page, viewModel);
            WirePageEventsToViewModel(viewModel, page);

            this.tabViewModels.Insert(index, viewModel);

            //InternalInsertTabPageChild(index, title, icon, mustWrapInNavigationPage, page);
            InternalInsertTabPageChild(index, page, mustWrapInNavigationPage);
            tabbedPage.Children[index].Icon = icon;
        }

        public void InsertTab<TViewModel>(int index, bool mustWrapInNavigationPage = false) where TViewModel : IMvvmViewModelBase
        {
            var viewModel = ResolveViewModel(typeof(TViewModel));
            var page = CreatePageFor<TViewModel>();
            BindViewModelToPage(page, viewModel);
            WirePageEventsToViewModel(viewModel, page);

            this.tabViewModels.Insert(index, viewModel);

            //InternalInsertTabPageChild(index, title, icon, mustWrapInNavigationPage, page);
            InternalInsertTabPageChild(index, page, mustWrapInNavigationPage);
        }

        public void RemoveTab<TViewModel>() where TViewModel : IMvvmViewModelBase
        {
            for (int i = 0; i < tabViewModels.Count; i++)
            {
                if (tabViewModels[i].GetType() == typeof(TViewModel))
                {
                    tabViewModels.RemoveAt(i);
                    tabbedPage.Children.RemoveAt(i);
                    return;
                }
            }

            throw new ArgumentOutOfRangeException(
                $"{typeof(TViewModel).Name} could not be found " +
                $"in {nameof(TabViewModels)}");
        }

        public void RemoveTabAt(int index)
        {
            tabViewModels.RemoveAt(index);
            tabbedPage.Children.RemoveAt(index);
        }

        private void InternalAddTabPageChild(Page page, string title, FileImageSource icon, bool mustWrapInNavigationPage)
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

            InternalAddTabPageChild(page, mustWrapInNavigationPage);

            if (!string.IsNullOrEmpty(title))
                tabbedPage.Children.Last().Title = title;
            if (!string.IsNullOrEmpty(icon.File))
                tabbedPage.Children.Last().Icon = icon;
        }

        private void InternalAddTabPageChild(Page page, bool mustWrapInNavigationPage)
        {
            tabbedPage.Children.Add(
                mustWrapInNavigationPage
                ? new NavigationPage(page)
                : page);
        }

        private void InternalInsertTabPageChild(int index, string title, FileImageSource icon, bool mustWrapInNavigationPage, Page page)
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

            tabbedPage.Children.Insert(
                index,
                mustWrapInNavigationPage
                    ? new NavigationPage(page)
                    : page);
            tabbedPage.Children[index].Title = title;
            tabbedPage.Children[index].Icon = icon;
        }

        private void InternalInsertTabPageChild(int index, Page page, bool mustWrapInNavigationPage)
        {
            tabbedPage.Children.Insert(
                index,
                mustWrapInNavigationPage
                    ? new NavigationPage(page)
                    : page);
        }


        public void AddTab(IMvvm mvvm, string title, FileImageSource icon)
        {
            if (!mvvm.IsInitialized)
                throw new NotInitializedException($"Initialise {nameof(mvvm)} before adding as a tab");

            tabViewModels.Add(mvvm.RootViewModel);
            InternalAddTabPageChild(mvvm.NavigationRoot, mustWrapInNavigationPage: false);

            tabbedPage.Children.Last().Title = title;
            tabbedPage.Children.Last().Icon = icon;
        }

        public void AddTab(IMvvm mvvm, string title)
        {
            if (!mvvm.IsInitialized)
                throw new NotInitializedException($"Initialise {nameof(mvvm)} before adding as a tab");

            tabViewModels.Add(mvvm.RootViewModel);
            InternalAddTabPageChild(mvvm.NavigationRoot, mustWrapInNavigationPage: false);

            tabbedPage.Children.Last().Title = title;
        }

        public void AddTab(IMvvm mvvm, FileImageSource icon)
        {
            if (!mvvm.IsInitialized)
                throw new NotInitializedException($"Initialise {nameof(mvvm)} before adding as a tab");

            tabViewModels.Add(mvvm.RootViewModel);
            InternalAddTabPageChild(mvvm.NavigationRoot, mustWrapInNavigationPage: false);

            tabbedPage.Children.Last().Icon = icon;
        }


        public void AddTab(IMvvm mvvm)
        {
            if (!mvvm.IsInitialized)
                throw new NotInitializedException($"Initialise {nameof(mvvm)} before adding as a tab");

            tabViewModels.Add(mvvm.RootViewModel);
            InternalAddTabPageChild(mvvm.NavigationRoot, mustWrapInNavigationPage: false);
        }

        public void InsertTab(int index, IMvvm mvvm, string title, FileImageSource icon)
        {
            if (!mvvm.IsInitialized)
                throw new NotInitializedException($"Initialise {nameof(mvvm)} before adding as a tab");

            tabViewModels.Insert(index, mvvm.RootViewModel);
            //InternalInsertTabPageChild(index, title, icon, false, mvvm.NavigationRoot);
            InternalInsertTabPageChild(index, mvvm.NavigationRoot, mustWrapInNavigationPage: false);
            tabbedPage.Children[index].Title = title;
            tabbedPage.Children[index].Icon = icon;

        }
        public void InsertTab(int index, IMvvm mvvm, string title)
        {
            if (!mvvm.IsInitialized)
                throw new NotInitializedException($"Initialise {nameof(mvvm)} before adding as a tab");

            tabViewModels.Insert(index, mvvm.RootViewModel);
            //InternalInsertTabPageChild(index, title, icon, false, mvvm.NavigationRoot);
            InternalInsertTabPageChild(index, mvvm.NavigationRoot, mustWrapInNavigationPage: false);
            tabbedPage.Children[index].Title = title;

        }
        public void InsertTab(int index, IMvvm mvvm, FileImageSource icon)
        {
            if (!mvvm.IsInitialized)
                throw new NotInitializedException($"Initialise {nameof(mvvm)} before adding as a tab");

            tabViewModels.Insert(index, mvvm.RootViewModel);
            //InternalInsertTabPageChild(index, title, icon, false, mvvm.NavigationRoot);
            InternalInsertTabPageChild(index, mvvm.NavigationRoot, mustWrapInNavigationPage: false);
            tabbedPage.Children[index].Icon = icon;

        }

        public void InsertTab(int index, IMvvm mvvm)
        {
            if (!mvvm.IsInitialized)
                throw new NotInitializedException($"Initialise {nameof(mvvm)} before adding as a tab");

            tabViewModels.Insert(index, mvvm.RootViewModel);
            InternalInsertTabPageChild(index, mvvm.NavigationRoot, mustWrapInNavigationPage: false);
        }

        public void RemoveTab(IMvvm mvvm)
        {
            var index = tabViewModels.IndexOf(mvvm.RootViewModel);
            if (index == -1)
                throw new ArgumentOutOfRangeException(
                $"{nameof(mvvm)} could not be found " +
                $"in {nameof(TabViewModels)}");

            RemoveTabAt(index);
        }

        public void RemoveTab(IMvvmViewModelBase viewModel)
        {
            var index = tabViewModels.IndexOf(viewModel);
            if (index == -1)
                throw new ArgumentOutOfRangeException(
                $"{nameof(viewModel)} could not be found " +
                $"in {nameof(TabViewModels)}");

            RemoveTabAt(index);
        }


    }


}
