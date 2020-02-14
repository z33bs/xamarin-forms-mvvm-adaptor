using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor
{
    public interface IAdaptorViewModel
    {
        Task InitializeAsync(object navigationData);
        Task OnAppearing();
    }
}