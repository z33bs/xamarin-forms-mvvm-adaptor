using System;

using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor.UnitTests.Views
{
    public class DiTestPage2 : ContentPage
    {
        public DiTestPage2()
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

