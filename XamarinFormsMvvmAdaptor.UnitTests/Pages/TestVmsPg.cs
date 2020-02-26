using System;

using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor.UnitTests.Pages
{
    public class TestVmsPg : ContentPage
    {
        public TestVmsPg()
        {
            BindingContext = new Vms.TestVmsVm();
            Content = new StackLayout
            {
                Children = {
                    new Label { Text = "Hello ContentPage" }
                }
            };
        }
    }
}

