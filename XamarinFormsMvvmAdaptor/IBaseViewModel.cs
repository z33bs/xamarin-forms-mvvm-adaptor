namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Implements all utility methods that can be called by <see cref="INavigationService"/>
    /// </summary>
    public interface IBaseViewModel : IAppearing, IDisappearing, IPushed, IRemoved
    {
    }
}