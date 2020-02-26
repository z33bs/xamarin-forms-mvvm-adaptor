using System;

using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor.UnitTests.Pages
{
    public class DiTestVmsPg : ContentPage
    {
        public DiTestVmsPg()
        {
            Content = new StackLayout
            {
                Children = {
                    new Label { Text = "Hello ContentPage" }
                }
            };
        }
    }
}

