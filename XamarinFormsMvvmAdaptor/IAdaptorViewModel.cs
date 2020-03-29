using System;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// All ViewModels must implement this interface for the <see cref="Mvvm"/>
    /// to work with them.
    /// </summary>
    public interface IMvvmViewModelBase
    {
        /// <summary>
        /// Runs automatically when a <see cref="Xamarin.Forms.Page"/>
        /// is pushed onto the <see cref="Mvvm.MainStack"/>.
        /// </summary>
        /// <param name="navigationData">Any data required for ViewModel initialisation</param>
        /// <returns></returns>
        Task OnViewPushedAsync(object navigationData);

        Task OnViewRemovedAsync();

        /// <summary>
        /// Runs automatically when a <see cref="Xamarin.Forms.Page"/> appears
        /// at the top of the <see cref="Mvvm.MainStack"/>
        /// or <see cref="Mvvm.ModalStack"/>
        /// </summary>
        /// <returns></returns>
        Task OnAppearingAsync();

        void OnViewAppearing(object sender, EventArgs e);
        void OnViewDisappearing(object sender, EventArgs e);
    }
}