using System;
using System.Threading.Tasks;
using MvvmHelpers;

namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// An optional BaseViewModel that implements the <see cref="IMvvmViewModelBase"/> interface
    /// and extends <see cref="BaseViewModel"/> from the dependency <see cref="MvvmHelpers"/>
    /// </summary>
    public abstract class MvvmViewModelBase : BaseViewModel, IMvvmViewModelBase
    {
        /// <summary>
        /// Runs automatically once the associated page is pushed onto the <see cref="Mvvm.MainStack"/>
        /// </summary>
        /// <param name="navigationData">Any data which could be useful for ViewModel Initialisation</param>
        /// <returns></returns>
        public virtual Task OnViewPushedAsync(object navigationData)
        {
            return Task.FromResult(false);
        }

        public virtual Task OnViewRemovedAsync()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Runs automatically after the associated page appears ontop of the stack
        /// More precisely, if the page is pushed, or if a page above it was popped
        /// </summary>
        /// <returns></returns>
        public virtual Task RefreshStateAsync(object data = null)
        {
            return Task.FromResult(false);
        }

        public virtual void OnViewAppearing(object sender, EventArgs e)
        {
        }

        public virtual void OnViewDisappearing(object sender, EventArgs e)
        {
        }
    }
}
