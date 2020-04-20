using System;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.Tests.ViewModels
{
    public class ImplementsBaseViewModel : IOnViewPushed, IOnViewRemoved, IOnViewAppearing, IOnViewDisappearing
    {
        public ImplementsBaseViewModel()
        {
        }

        public void OnViewAppearing(object sender, EventArgs e)
        {
        }

        public void OnViewDisappearing(object sender, EventArgs e)
        {
        }

        public Task OnViewPushedAsync(object navigationData)
        {
            return Task.FromResult(true);
        }

        public Task OnViewRemovedAsync()
        {
            return Task.FromResult(true);
        }
    }
}
