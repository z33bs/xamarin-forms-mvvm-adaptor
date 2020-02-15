using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// All ViewModels must implement this interface for the <see cref="NavController"/>
    /// to work with them.
    /// </summary>
    public interface IAdaptorViewModel
    {
        /// <summary>
        /// Runs automatically when a <see cref="Xamarin.Forms.Page"/>
        /// is pushed onto the <see cref="NavController.NavigationStack"/>.
        /// </summary>
        /// <param name="navigationData">Any data required for ViewModel initialisation</param>
        /// <returns></returns>
        Task InitializeAsync(object navigationData);
        /// <summary>
        /// Runs automatically when a <see cref="Xamarin.Forms.Page"/> appears
        /// at the top of the <see cref="NavController.NavigationStack"/>
        /// or <see cref="NavController.ModalStack"/>
        /// </summary>
        /// <returns></returns>
        Task OnAppearing();
    }
}