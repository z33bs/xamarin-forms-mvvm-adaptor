using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using WordJumble.Services;
using WordJumble.Views;
using WordJumble.ViewModels;
using XamarinFormsMvvmAdaptor;

namespace WordJumble
{
    public partial class App : Application
    {
        //public static NavController MainNavController { get; } = new NavController(new NavigationPage(new MainPage()));
        public static NavController MainNavController { get; } = new NavController(new MainViewModel());

        public App()
        {
            InitializeComponent();

            MainPage = MainNavController.RootPage;
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
