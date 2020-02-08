using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor
{
    public interface IMvvmAdaptorViewModel
    {
        Task InitializeAsync(object navigationData);
    }
}