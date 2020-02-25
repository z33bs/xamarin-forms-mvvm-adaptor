using System;

using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor.UnitTests.Views
{
    public class DiTestPage4 : ContentPage
    {
        public DiTestPage4()
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

