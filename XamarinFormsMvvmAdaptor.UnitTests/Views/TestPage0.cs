using System;

using Xamarin.Forms;
using XamarinFormsMvvmAdaptor.UnitTests.ViewModels;

namespace XamarinFormsMvvmAdaptor.UnitTests.Views
{
    public class TestPage0 : ContentPage
    {
        public TestPage0()
        {
            BindingContext = new TestViewModel0();

            Content = new StackLayout
            {
                Children = {
                    new Label { Text = "Hello ContentPage" }
                }
            };
        }
    }
}

