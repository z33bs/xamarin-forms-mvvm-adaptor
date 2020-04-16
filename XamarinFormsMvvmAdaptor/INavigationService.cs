using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Controlls Page Navigation from the ViewModel
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Gets the Current Shell's NavigationStack
        /// </summary>
        IReadOnlyList<Page> NavigationStack { get; }
        /// <summary>
        /// Gets the Current Shell's ModalStack
        /// </summary>
        IReadOnlyList<Page> ModalStack { get; }

        /// <summary>
        /// Navigates to a <see cref="Page"/>
        /// </summary>
        /// <param name="state">A URI representing either the current page or a destination for navigation in a Shell application.</param>
        /// <param name="animate"></param>
        /// <returns></returns>
        Task GoToAsync(ShellNavigationState state, bool animate);

        ///<inheritdoc cref="GoToAsync(ShellNavigationState, bool)"/>
        Task GoToAsync(ShellNavigationState state);

        /// <summary>
        /// Pops a <see cref="Page"/> off the <see cref="NavigationStack"/>
        /// </summary>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task PopAsync(bool animated = true);

        /// <summary>
        /// Pops a <see cref="Page"/> off the <see cref="ModalStack"/>
        /// </summary>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task PopModalAsync(bool animated = true);

        /// <summary>
        /// Pops all pages <see cref="Page"/> off the <see cref="NavigationStack"/>, leaving only the Root Page
        /// </summary>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task PopToRootAsync(bool animated = true);

        /// <summary>
        /// Pushes a <see cref="Page"/> onto the <see cref="NavigationStack"/>
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel corresponding the the Page to be Pushed</typeparam>
        /// <param name="navigationData">Object that will be recieved by the <see cref="IPushed.OnViewPushedAsync(object)"/> method</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task<TViewModel> PushAsync<TViewModel>(object navigationData, bool animated = true) where TViewModel : class, IPushed;

        ///<inheritdoc cref="PushAsync{TViewModel}(object, bool)"/>
        Task<TViewModel> PushAsync<TViewModel>(bool animated = true) where TViewModel : class;

        /// <summary>
        /// Pushes a <see cref="Page"/> onto the <see cref="ModalStack"/>
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel corresponding the the Page to be Pushed</typeparam>
        /// <param name="navigationData">Object that will be recieved by the <see cref="IPushed.OnViewPushedAsync(object)"/> method</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task<TViewModel> PushModalAsync<TViewModel>(object navigationData, bool animated = true) where TViewModel : class, IPushed;

        ///<inheritdoc cref="PushModalAsync{TViewModel}(object, bool)"/>
        Task<TViewModel> PushModalAsync<TViewModel>(bool animated = true) where TViewModel : class;

        /// <summary>
        /// Removes specific <see cref="Page"/> from the <see cref="NavigationStack"/>
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel corresponding the the Page to be Removed</typeparam>
        /// <returns></returns>
        Task RemovePageFor<TViewModel>();

        /// <summary>
        /// Removes the <see cref="Page"/> underneath the Top Page in the <see cref="NavigationStack"/>
        /// </summary>
        /// <returns></returns>
        Task RemovePreviousPageFromMainStack();
    }
}