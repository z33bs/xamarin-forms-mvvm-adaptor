using System;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor
{
    public abstract class BaseViewModel : CommonObservablePropertyObject,  IBaseViewModel
    {
        public virtual Task OnViewPushedAsync(object navigationData)
        {
            return Task.FromResult(false);
        }

        public virtual Task OnViewRemovedAsync()
        {
            return Task.FromResult(true);
        }

        public virtual void OnViewAppearing(object sender, EventArgs e)
        {
        }

        public virtual void OnViewDisappearing(object sender, EventArgs e)
        {
        }
    }
}
