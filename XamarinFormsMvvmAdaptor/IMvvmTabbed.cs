using System.Collections.Generic;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public interface IMvvmTabbed : IMvvmBase
    {
        TabbedPage RootPage { get; }
        IReadOnlyList<IMvvmViewModelBase> TabViewModels { get; }
        IMvvmViewModelBase CurrentTabViewModel { get; set; }

        void AddTab<TViewModel>(string title, FileImageSource icon, bool mustWrapInNavigationPage = false) where TViewModel : IMvvmViewModelBase;
        void AddTab<TViewModel>(string title, bool mustWrapInNavigationPage = false) where TViewModel : IMvvmViewModelBase;
        void AddTab<TViewModel>(FileImageSource icon, bool mustWrapInNavigationPage = false) where TViewModel : IMvvmViewModelBase;
        void AddTab<TViewModel>(bool mustWrapInNavigationPage = false) where TViewModel : IMvvmViewModelBase;
        void AddTab(IMvvm mvvm, string title, FileImageSource icon);
        void AddTab(IMvvm mvvm, string title);
        void AddTab(IMvvm mvvm, FileImageSource icon);
        void AddTab(IMvvm mvvm);
        Page Initialize(bool mustWrapInNavigationPage = true);
        Page Initialize(IMvvm[] mvvms);
        Page Initialize<T1>(bool mustWrapInNavigationPage = true) where T1 : IMvvmViewModelBase;
        Page Initialize<T1, T2>(bool mustWrapInNavigationPage = true)
            where T1 : IMvvmViewModelBase
            where T2 : IMvvmViewModelBase;
        Page Initialize<T1, T2, T3>(bool mustWrapInNavigationPage = true)
            where T1 : IMvvmViewModelBase
            where T2 : IMvvmViewModelBase
            where T3 : IMvvmViewModelBase;
        Page Initialize<T1, T2, T3, T4>(bool mustWrapInNavigationPage = true)
            where T1 : IMvvmViewModelBase
            where T2 : IMvvmViewModelBase
            where T3 : IMvvmViewModelBase
            where T4 : IMvvmViewModelBase;
        Page Initialize<T1, T2, T3, T4, T5>(bool mustWrapInNavigationPage = true)
            where T1 : IMvvmViewModelBase
            where T2 : IMvvmViewModelBase
            where T3 : IMvvmViewModelBase
            where T4 : IMvvmViewModelBase
            where T5 : IMvvmViewModelBase;
        void InsertTab<TViewModel>(int index, string title, FileImageSource icon, bool mustWrapInNavigationPage = false) where TViewModel : IMvvmViewModelBase;
        void InsertTab<TViewModel>(int index, string title, bool mustWrapInNavigationPage = false) where TViewModel : IMvvmViewModelBase;
        void InsertTab<TViewModel>(int index, FileImageSource icon, bool mustWrapInNavigationPage = false) where TViewModel : IMvvmViewModelBase;
        void InsertTab<TViewModel>(int index, bool mustWrapInNavigationPage = false) where TViewModel : IMvvmViewModelBase;
        void InsertTab(int index, IMvvm mvvm, string title, FileImageSource icon);
        void InsertTab(int index, IMvvm mvvm, string title);
        void InsertTab(int index, IMvvm mvvm, FileImageSource icon);
        void InsertTab(int index, IMvvm mvvm);
        void RemoveTab<TViewModel>() where TViewModel : IMvvmViewModelBase;
        void RemoveTab(IMvvmViewModelBase viewModel);
        void RemoveTab(IMvvm mvvm);
        void RemoveTabAt(int index);
    }
}