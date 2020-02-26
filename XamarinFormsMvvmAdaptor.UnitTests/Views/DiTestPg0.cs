using System;

using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor.UnitTests.Views
{
    public class DiTestPg0 : ContentPage
    {
        public DiTestPg0()
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

