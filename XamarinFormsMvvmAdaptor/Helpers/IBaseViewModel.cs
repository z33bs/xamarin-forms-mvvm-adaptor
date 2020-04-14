namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Implements all utility methods that can be called by <see cref="INavigationService"/>
    /// </summary>
    public interface IBaseViewModel : ICommonObservablePropertyObject, IAppearing, IDisappearing, IPushed, IRemoved
    {
    }
}