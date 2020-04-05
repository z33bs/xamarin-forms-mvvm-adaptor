using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public interface IMvvm : IMvvmBase
    {
        Page Initialize<TViewModel>(bool mustWrapInNavigationPage = true) where TViewModel : class, IMvvmViewModelBase;
    }
}