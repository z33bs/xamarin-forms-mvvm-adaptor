using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor
{
    public interface IBaseViewModel
    {
        Task InitializeAsync(object navigationData);
    }
}