using System;
using System.Collections.Specialized;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public interface IMvvmTabbedViewModelBase : IMvvmViewModelBase
    {
        void OnTabbedViewCurrentPageChanged(object sender, EventArgs e);
    }
}