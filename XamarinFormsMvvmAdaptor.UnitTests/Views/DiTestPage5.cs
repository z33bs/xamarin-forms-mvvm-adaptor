using System;

using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor.UnitTests.Views
{
    public class DiTestPage5 : ContentPage
    {
        public DiTestPage5()
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

