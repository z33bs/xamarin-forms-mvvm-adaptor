using System;

using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor.UnitTests.Views
{
    public class DiTestPage0 : ContentPage
    {
        public DiTestPage0()
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

