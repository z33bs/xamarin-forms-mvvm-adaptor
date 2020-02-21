using System;

using Xamarin.Forms;
using XamarinFormsMvvmAdaptor.UnitTests.ViewModels;

namespace XamarinFormsMvvmAdaptor.UnitTests.Views
{
    public class TestPage1 : ContentPage
    {
        public TestPage1()
        {
            BindingContext = new TestViewModel1();

            Content = new StackLayout
            {
                Children = {
                    new Label { Text = "Hello ContentPage" }
                }
            };
        }
    }
}

