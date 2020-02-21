using System;

using Xamarin.Forms;
using XamarinFormsMvvmAdaptor.UnitTests.ViewModels;

namespace XamarinFormsMvvmAdaptor.UnitTests.Views
{
    public class TestPage3 : ContentPage
    {
        public TestPage3()
        {
            BindingContext = new TestViewModel3();

            Content = new StackLayout
            {
                Children = {
                    new Label { Text = "Hello ContentPage" }
                }
            };
        }
    }
}

