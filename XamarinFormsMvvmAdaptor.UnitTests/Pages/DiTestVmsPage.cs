using System;

using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor.UnitTests.Pages
{
    public class DiTestVmsPage : ContentPage
    {
        public DiTestVmsPage()
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

