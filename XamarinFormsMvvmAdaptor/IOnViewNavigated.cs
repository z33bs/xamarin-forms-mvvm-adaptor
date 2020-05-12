using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Implements <see cref="OnViewNavigatedAsync(object)"/>
    /// </summary>
    public interface IOnViewNavigated
    {
        /// <summary>
        /// Runs automatically when a <see cref="Xamarin.Forms.Page"/>
        /// is pushed onto the <see cref="NavigationService.NavigationStack"/>.
        /// </summary>
        /// <param name="navigationData">Any data required for ViewModel initialisation</param>
        /// <returns></returns>
        Task OnViewNavigatedAsync(object navigationData);
    }
}
