using System.Threading.Tasks;
using MvvmHelpers;

namespace XamarinFormsMvvmAdaptor
{
    public abstract class AdaptorViewModel : BaseViewModel, IAdaptorViewModel
    {
        /// <summary>
        /// Runs automatically once the associated page is pushed onto the stack
        /// Useful to pass data from the previous ViewModel to this one
        /// Using the <param name="navigationData"></param>
        /// </summary>
        /// <param name="navigationData">Any data which could be useful for ViewModel Initialisation</param>
        /// <returns></returns>
        public virtual Task InitializeAsync(object navigationData)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Runs automatically after the associated page appears ontop of the stack
        /// More precisely, if the page is pushed, or if a page above it was popped
        /// </summary>
        /// <returns></returns>
        public virtual Task OnAppearing()
        {
            return Task.FromResult(false);
        }
    }
}
