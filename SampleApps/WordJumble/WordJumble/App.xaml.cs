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
        public static NavController NavController { get; } = new NavController(new MainViewModel());

        public App()
        {
            InitializeComponent();

            MainPage = NavController.RootPage;
        }

        protected override async void OnStart()
        {
            await NavController.InitAsync();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
