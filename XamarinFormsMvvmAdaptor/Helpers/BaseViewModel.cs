using System;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    ///<inheritdoc/>
    public abstract class BaseViewModel : CommonObservablePropertyObject,  IBaseViewModel
    {
        ///<inheritdoc/>
        public virtual Task OnViewPushedAsync(object navigationData)
        {
            return Task.FromResult(false);
        }

        ///<inheritdoc/>
        public virtual Task OnViewRemovedAsync()
        {
            return Task.FromResult(true);
        }

        ///<inheritdoc/>
        public virtual void OnViewAppearing(object sender, EventArgs e)
        {
        }

        ///<inheritdoc/>
        public virtual void OnViewDisappearing(object sender, EventArgs e)
        {
        }
    }
}
