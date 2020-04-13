using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Implements <see cref="OnViewRemovedAsync"/>
    /// </summary>
    public interface IRemoved
    {
        Task OnViewRemovedAsync();
    }
}
