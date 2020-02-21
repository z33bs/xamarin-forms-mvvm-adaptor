using System;

using Xamarin.Forms;
using XamarinFormsMvvmAdaptor.UnitTests.ViewModels;

namespace XamarinFormsMvvmAdaptor.UnitTests.Views
{
    public class TestPage2 : ContentPage
    {
        public TestPage2()
        {
            BindingContext = new TestViewModel2();

            Content = new StackLayout
            {
                Children = {
                    new Label { Text = "Hello ContentPage" }
                }
            };
        }
    }
}

