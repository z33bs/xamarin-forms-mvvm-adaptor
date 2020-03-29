using System;
using System.Collections.Specialized;
using System.Diagnostics;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    public class MvvmTabbedViewModelBase : MvvmViewModelBase, IMvvmTabbedViewModelBase
    {
        public virtual void OnTabbedViewCurrentPageChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("Vm OnCurrentPageChanged");
        }
    }
}
