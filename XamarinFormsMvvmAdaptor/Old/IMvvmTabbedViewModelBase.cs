using System;
using System.Collections.Specialized;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public interface IMvvmTabbedViewModelBase : IBaseViewModel
    {
        void OnTabbedViewCurrentPageChanged(object sender, EventArgs e);
    }
}