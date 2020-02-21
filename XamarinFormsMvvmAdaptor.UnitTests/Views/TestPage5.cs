using System;

using Xamarin.Forms;
using XamarinFormsMvvmAdaptor.UnitTests.ViewModels;

namespace XamarinFormsMvvmAdaptor.UnitTests.Views
{
    public class TestPage5 : ContentPage
    {
        public TestPage5()
        {
            BindingContext = new TestViewModel4();

            Content = new StackLayout
            {
                Children = {
                    new Label { Text = "Hello ContentPage" }
                }
            };
        }
    }
}

