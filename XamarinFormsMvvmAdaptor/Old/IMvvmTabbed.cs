using System.Collections.Generic;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public interface IMvvmTabbed : IMvvmBase
    {
        TabbedPage RootPage { get; }
        IReadOnlyList<IBaseViewModel> TabViewModels { get; }
        IBaseViewModel CurrentTabViewModel { get; set; }

        void AddTab<TViewModel>(string title, FileImageSource icon, bool mustWrapInNavigationPage = false) where TViewModel : IBaseViewModel;
        void AddTab<TViewModel>(string title, bool mustWrapInNavigationPage = false) where TViewModel : IBaseViewModel;
        void AddTab<TViewModel>(FileImageSource icon, bool mustWrapInNavigationPage = false) where TViewModel : IBaseViewModel;
        void AddTab<TViewModel>(bool mustWrapInNavigationPage = false) where TViewModel : IBaseViewModel;
        void AddTab(IMvvm mvvm, string title, FileImageSource icon);
        void AddTab(IMvvm mvvm, string title);
        void AddTab(IMvvm mvvm, FileImageSource icon);
        void AddTab(IMvvm mvvm);
        Page Initialize(bool mustWrapInNavigationPage = true);
        Page Initialize(IMvvm[] mvvms);
        Page Initialize<T1>(bool mustWrapInNavigationPage = true) where T1 : IBaseViewModel;
        Page Initialize<T1, T2>(bool mustWrapInNavigationPage = true)
            where T1 : IBaseViewModel
            where T2 : IBaseViewModel;
        Page Initialize<T1, T2, T3>(bool mustWrapInNavigationPage = true)
            where T1 : IBaseViewModel
            where T2 : IBaseViewModel
            where T3 : IBaseViewModel;
        Page Initialize<T1, T2, T3, T4>(bool mustWrapInNavigationPage = true)
            where T1 : IBaseViewModel
            where T2 : IBaseViewModel
            where T3 : IBaseViewModel
            where T4 : IBaseViewModel;
        Page Initialize<T1, T2, T3, T4, T5>(bool mustWrapInNavigationPage = true)
            where T1 : IBaseViewModel
            where T2 : IBaseViewModel
            where T3 : IBaseViewModel
            where T4 : IBaseViewModel
            where T5 : IBaseViewModel;
        void InsertTab<TViewModel>(int index, string title, FileImageSource icon, bool mustWrapInNavigationPage = false) where TViewModel : IBaseViewModel;
        void InsertTab<TViewModel>(int index, string title, bool mustWrapInNavigationPage = false) where TViewModel : IBaseViewModel;
        void InsertTab<TViewModel>(int index, FileImageSource icon, bool mustWrapInNavigationPage = false) where TViewModel : IBaseViewModel;
        void InsertTab<TViewModel>(int index, bool mustWrapInNavigationPage = false) where TViewModel : IBaseViewModel;
        void InsertTab(int index, IMvvm mvvm, string title, FileImageSource icon);
        void InsertTab(int index, IMvvm mvvm, string title);
        void InsertTab(int index, IMvvm mvvm, FileImageSource icon);
        void InsertTab(int index, IMvvm mvvm);
        void RemoveTab<TViewModel>() where TViewModel : IBaseViewModel;
        void RemoveTab(IBaseViewModel viewModel);
        void RemoveTab(IMvvm mvvm);
        void RemoveTabAt(int index);
    }
}