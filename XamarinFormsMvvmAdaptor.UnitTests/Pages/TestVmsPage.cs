using System;

using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor.UnitTests.Pages
{
    public class TestVmsPage : ContentPage
    {
        public TestVmsPage()
        {
            BindingContext = new Vms.TestVmsViewModel();
            Content = new StackLayout
            {
                Children = {
                    new Label { Text = "Hello ContentPage" }
                }
            };
        }
    }
}

